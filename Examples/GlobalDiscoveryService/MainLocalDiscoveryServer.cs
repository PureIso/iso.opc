using Opc.Ua;
using Opc.Ua.Gds.Server;
using Opc.Ua.Gds.Server.Database;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Iso.Opc.ApplicationNodeManager.GDS;

namespace LocalDiscoveryService
{
    public class MainLocalDiscoveryServer : StandardServer
    {
        #region Private Fields
        private readonly object _requestLock;
        private readonly Dictionary<uint, ImpersonationContext> _contexts;
        private readonly IApplicationsDatabase _database;
        private readonly ICertificateRequest _request;
        private readonly ICertificateGroup _certificateGroup;
        private readonly bool _autoApprove;
        #endregion 

        public MainLocalDiscoveryServer(IApplicationsDatabase database,ICertificateRequest request,ICertificateGroup certificateGroup,bool autoApprove = true)
        {
            _database = database;
            _request = request;
            _certificateGroup = certificateGroup;
            _autoApprove = autoApprove;

            _requestLock = new object();
            _contexts = new Dictionary<uint, ImpersonationContext>();
        }

        #region Overridden Methods
        protected override void OnServerStarted(IServerInternal server)
        {
            Console.WriteLine("The server is started.");
            base.OnServerStarted(server);
            // request notifications when the user identity is changed. all valid users are accepted by default.
            server.SessionManager.ImpersonateUser += SessionManager_ImpersonateUser;
        }
        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            Console.WriteLine("Creating the Node Managers.");
            List<INodeManager> nodeManagers = new List<INodeManager>
            {
                new GlobalDiscoveryServiceNodeManager(server, configuration,
                    _database,
                    _request,
                    _certificateGroup,
                    _autoApprove)
            };
            //create the custom node managers.
            // create master node manager.
            return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
        }
        protected override ServerProperties LoadServerProperties()
        {
            ServerProperties properties = new ServerProperties
            {
                ManufacturerName = "MyCompany",
                ProductName = "Basic Global Discovery Server",
                ProductUri = "http://MyCompany.com/LocalDiscoveryServer",
                SoftwareVersion = Utils.GetAssemblySoftwareVersion(),
                BuildNumber = Utils.GetAssemblyBuildNumber(),
                BuildDate = Utils.GetAssemblyTimestamp()
            };
            return properties;
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
                    Opc.Ua.Gds.Namespaces.OpcUaGds,
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
        private void SessionManager_ImpersonateUser(Session session, ImpersonateEventArgs args)
        {
            switch (args.NewIdentity)
            {
                // check for a user name token
                case UserNameIdentityToken userNameToken:
                {
                    if (VerifyPassword(userNameToken))
                    {
                        switch (userNameToken.UserName)
                        {
                            // Server configuration administrator, manages the GDS server security
                            case "sysadmin":
                            {
                                args.Identity = new SystemConfigurationIdentity(new UserIdentity(userNameToken));
                                Utils.Trace($"SystemConfigurationAdmin Token Accepted: {args.Identity.DisplayName}");
                                return;
                            }
                            // GDS administrator
                            case "appadmin":
                            {
                                //can register to GDS
                                args.Identity = new RoleBasedIdentity(new UserIdentity(userNameToken), GdsRole.ApplicationAdmin);
                                Utils.Trace($"ApplicationAdmin Token Accepted: {args.Identity.DisplayName}");
                                return;
                            }
                            // GDS user
                            case "appuser":
                            {
                                args.Identity = new RoleBasedIdentity(new UserIdentity(userNameToken), GdsRole.ApplicationUser);
                                Utils.Trace($"ApplicationUser Token Accepted: {args.Identity.DisplayName}");
                                return;
                            }
                        }
                    }

                    break;
                }
                // check for x509 user token.
                case X509IdentityToken x509Token:
                {
                    const GdsRole role = GdsRole.ApplicationUser;
                    VerifyUserTokenCertificate(x509Token.Certificate);

                    // todo: is cert listed in admin list? then 
                    // role = GdsRole.ApplicationAdmin;

                    Utils.Trace($"X509 Token Accepted: {args.Identity.DisplayName} as {role.ToString()}");
                    args.Identity = new RoleBasedIdentity(new UserIdentity(x509Token), role);
                    return;
                }
            }
        }
        private void VerifyUserTokenCertificate(X509Certificate2 certificate)
        {
            try
            {
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
        private bool VerifyPassword(UserNameIdentityToken userNameToken)
        {
            // TODO: check username/password permissions
            return userNameToken.DecryptedPassword == "demo";
        }
        #endregion
    }
}
