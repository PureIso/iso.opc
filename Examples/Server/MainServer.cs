using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using Iso.Opc.ApplicationManager;
using Iso.Opc.ApplicationNodeManager.Server;
using Opc.Ua;
using Opc.Ua.Server;

namespace Server
{
    public class MainServer : StandardServer
    {
        #region Private Fields
        private readonly object _requestLock;
        private readonly Dictionary<uint, ImpersonationContext> _contexts;
        private readonly ApplicationInstanceManager _applicationInstanceManager;
        private X509CertificateValidator _certificateValidator;
        #endregion

        public MainServer(ApplicationInstanceManager applicationInstanceManager)
        {
            _requestLock = new object();
            _contexts = new Dictionary<uint, ImpersonationContext>();
            _applicationInstanceManager = applicationInstanceManager;
        }

        #region Overridden Methods
        /// <summary>
        /// Creates the node managers for the server.
        /// </summary>
        /// <remarks>
        /// This method allows the sub-class create any additional node managers which it uses. The SDK
        /// always creates a CoreNodeManager which handles the built-in nodes defined by the specification.
        /// Any additional NodeManagers are expected to handle application specific nodes.
        /// </remarks>
        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            Console.WriteLine("Creating the Node Managers.");
            List<INodeManager> nodeManagers = new List<INodeManager> {new ServerNodeManager(server, configuration)};
            // create master node manager.
            return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
        }
        /// <summary>
        /// Loads the non-configurable properties for the application.
        /// </summary>
        /// <remarks>
        /// These properties are exposed by the server but cannot be changed by administrators.
        /// </remarks>
        protected override ServerProperties LoadServerProperties()
        {
            ServerProperties properties = new ServerProperties
            {
                ProductName = _applicationInstanceManager.ApplicationInstance.ApplicationConfiguration.ApplicationName,
                ProductUri = _applicationInstanceManager.ApplicationInstance.ApplicationConfiguration.ProductUri,
                SoftwareVersion = Utils.GetAssemblySoftwareVersion(),
                BuildNumber = Utils.GetAssemblyBuildNumber(),
                BuildDate = Utils.GetAssemblyTimestamp()
            };
            // TBD - All applications have software certificates that need to added to the properties.
            return properties;
        }
        /// <summary>
        /// Initializes the server before it starts up.
        /// </summary>
        /// <remarks>
        /// This method is called before any startup processing occurs. The sub-class may update the 
        /// configuration object or do any other application specific startup tasks.
        /// </remarks>
        protected override void OnServerStarting(ApplicationConfiguration configuration)
        {
            Console.WriteLine("The server is starting.....");
            base.OnServerStarting(configuration);
            // it is up to the application to decide how to validate user identity tokens.
            // this function creates validator for X509 identity tokens.
            CreateUserIdentityValidators(configuration);
        }
        /// <summary>
        /// Called after the server has been started.
        /// </summary>
        protected override void OnServerStarted(IServerInternal server)
        {
            Console.WriteLine("The server is started.");
            base.OnServerStarted(server);
            //request notifications when the user identity is changed.all valid users are accepted by default.
            server.SessionManager.ImpersonateUser += SessionManager_ImpersonateUser;
        }
        /// <summary>
        /// Creates the resource manager for the server.
        /// </summary>
        protected override ResourceManager CreateResourceManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            ResourceManager resourceManager = new ResourceManager(server, configuration);
            // add some localized strings to the resource manager to demonstrate that localization occurs.
            resourceManager.Add("UnexpectedUserTokenError", "fr-FR", "Une erreur inattendue s'est produite lors de la validation utilisateur.");
            resourceManager.Add("BadUserAccessDenied", "fr-FR", "Utilisateur ne peut pas changer la valeur.");
            return resourceManager;
        }
        protected override OperationContext ValidateRequest(RequestHeader requestHeader, RequestType requestType)
        {
            OperationContext context = base.ValidateRequest(requestHeader, requestType);
            if (requestType != RequestType.Write)
                return context;
            // reject all writes if no user provided.
            if (context.UserIdentity.TokenType == UserTokenType.Anonymous)
            {
                // construct translation object with default text.
                TranslationInfo info = new TranslationInfo(
                    "NoWriteAllowed",
                    "en-US",
                    "Must provide a valid user before calling write.");
                // create an exception with a vendor defined sub-code.
                throw new ServiceResultException(new ServiceResult(
                    StatusCodes.BadUserAccessDenied,
                    "NoWriteAllowed",
                    Opc.Ua.Gds.Namespaces.OpcUa,
                    new LocalizedText(info)));
            }
            UserIdentityToken securityToken = context.UserIdentity.GetIdentityToken();
            // check for a user name token.
            if (!(securityToken is UserNameIdentityToken)) 
                return context;
            lock (_requestLock)
            {
                _contexts.Add(context.RequestId, new ImpersonationContext());
            }
            return context;
        }
        protected override void OnRequestComplete(OperationContext context)
        {
            lock (_requestLock)
            {
                if (_contexts.TryGetValue(context.RequestId, out ImpersonationContext _))
                    _contexts.Remove(context.RequestId);
            }
            base.OnRequestComplete(context);
        }
        #endregion

        #region User Validation Functions
        /// <summary>
        /// Creates the objects used to validate the user identity tokens supported by the server.
        /// </summary>
        private void CreateUserIdentityValidators(ApplicationConfiguration configuration)
        {
            foreach (UserTokenPolicy policy in configuration.ServerConfiguration.UserTokenPolicies)
            {
                // create a validator for a certificate token policy.
                if (policy.TokenType != UserTokenType.Certificate) 
                    continue;
                // check if user certificate trust lists are specified in configuration.
                if (configuration.SecurityConfiguration.TrustedUserCertificates == null ||
                    configuration.SecurityConfiguration.UserIssuerCertificates == null) 
                    continue;
                CertificateValidator certificateValidator = new CertificateValidator();
                certificateValidator.Update(configuration.SecurityConfiguration).Wait();
                certificateValidator.Update(configuration.SecurityConfiguration.UserIssuerCertificates,
                    configuration.SecurityConfiguration.TrustedUserCertificates,
                    configuration.SecurityConfiguration.RejectedCertificateStore);
                // set custom validator for user certificates.
                _certificateValidator = certificateValidator.GetChannelValidator();
            }
        }
        /// <summary>
        /// Called when a client tries to change its user identity.
        /// </summary>
        private void SessionManager_ImpersonateUser(Session session, ImpersonateEventArgs args)
        {
            try
            {
                switch (args.NewIdentity)
                {
                    //New connection entry point
                    // check for a user name token.
                    case UserNameIdentityToken userNameToken:
                        args.Identity = VerifyPassword(userNameToken);
                        return;
                    // check for x509 user token.
                    case X509IdentityToken x509Token:
                        VerifyUserTokenCertificate(x509Token.Certificate);
                        args.Identity = new UserIdentity(x509Token);
                        Utils.Trace($"X509 Token Accepted: {args.Identity.DisplayName}");
                        return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Session Manager Impersonate Exception:\r\n{e.StackTrace}");
            }
        }
        /// <summary>
        /// Validates the password for a username token.
        /// </summary>
        private IUserIdentity VerifyPassword(UserNameIdentityToken userNameToken)
        {
            try
            {
                string userName = userNameToken.UserName;
                string password = userNameToken.DecryptedPassword;

                if (string.IsNullOrEmpty(userName))
                {
                    // an empty username is not accepted.
                    throw ServiceResultException.Create(StatusCodes.BadIdentityTokenInvalid,
                        "Security token is not a valid username token. An empty username is not accepted.");
                }
                if (string.IsNullOrEmpty(password))
                {
                    // an empty password is not accepted.
                    throw ServiceResultException.Create(StatusCodes.BadIdentityTokenRejected,
                        "Security token is not a valid username token. An empty password is not accepted.");
                }
                switch (userName)
                {
                    // User with permission to configure server
                    case "sysadmin" when password == "demo":
                        return new SystemConfigurationIdentity(new UserIdentity(userNameToken));
                    // standard users for CTT verification
                    case "user1" when password == "password":
                    case "user2" when password == "password1":
                        return new UserIdentity(userNameToken);
                    default:
                        {
                            // construct translation object with default text.
                            TranslationInfo info = new TranslationInfo(
                                "InvalidPassword",
                                "en-US",
                                "Invalid username or password.",
                                userName);
                            // create an exception with a vendor defined sub-code.
                            throw new ServiceResultException(new ServiceResult(
                                StatusCodes.BadUserAccessDenied,
                                "InvalidPassword",
                                LoadServerProperties().ProductUri,
                                new LocalizedText(info)));
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Session Manager Impersonate Exception:\r\n{e.StackTrace}");
            }
            return null;
        }
        /// <summary>
        /// Verifies that a certificate user token is trusted.
        /// </summary>
        private void VerifyUserTokenCertificate(X509Certificate2 certificate)
        {
            try
            {
                if (_certificateValidator != null) 
                    _certificateValidator.Validate(certificate);
                else
                    CertificateValidator.Validate(certificate);
            }
            catch (Exception e)
            {
                TranslationInfo info;
                StatusCode result = StatusCodes.BadIdentityTokenRejected;
                if (e is ServiceResultException se && se.StatusCode == StatusCodes.BadCertificateUseNotAllowed)
                {
                    info = new TranslationInfo(
                        "InvalidCertificate",
                        "en-US",
                        $"'{certificate.Subject}' is an invalid user certificate.");
                    result = StatusCodes.BadIdentityTokenInvalid;
                }
                else
                {
                    // construct translation object with default text.
                    info = new TranslationInfo(
                        "UntrustedCertificate",
                        "en-US",
                        $"'{certificate.Subject}' is not a trusted user certificate.");
                }

                // create an exception with a vendor defined sub-code.
                throw new ServiceResultException(new ServiceResult(
                    result,
                    info.Key,
                    LoadServerProperties().ProductUri,
                    new LocalizedText(info)));
            }
        }
        #endregion
    }
}
