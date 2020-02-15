using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Iso.Opc.ApplicationManager.Models;
using Iso.Opc.ApplicationManager.Models.Controllers;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Client.ComplexTypes;
using Opc.Ua.Configuration;
using Opc.Ua.Gds;
using Opc.Ua.Gds.Client;
using Opc.Ua.Security;
using ApplicationType = Opc.Ua.ApplicationType;
using CertificateIdentifier = Opc.Ua.CertificateIdentifier;

namespace Iso.Opc.ApplicationManager
{
    public class ApplicationInstanceManager
    {
        public Dictionary<string, ReferenceDescription> ReferenceDescriptionDictionary;
        public List<ExtendedReferenceDescription> ExtendedReferenceDescriptions;
        private List<List<ReferenceDescription>> _referenceDescriptions;

        #region Fields
        private int _reconnectPeriod = 10;
        private int _discoverTimeout = 5000;
        private SessionReconnectHandler _reconnectHandler;
        private EventHandler _reconnectStarting;
        private EventHandler _reconnectComplete;
        private EventHandler _KeepAliveComplete;
        #endregion

        #region Properties
        public ApplicationInstance ApplicationInstance { get; set; }
        public GlobalDiscoveryServerClient GlobalDiscoveryServerClient { get; set; }
        public RegisteredApplication RegisteredApplication { get; set; }
        public Session Session { get; set; }
        public EndpointDescription SessionEndpointDescription { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="productUri"></param>
        /// <param name="applicationUri"></param>
        /// <param name="subjectName"></param>
        /// <param name="baseAddress"></param>
        /// <param name="serverCapabilities"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="endpointApplicationUri"></param>
        /// <param name="discoveryUrls"></param>
        /// <param name="wellKnownDiscoveryUrls"></param>
        /// <param name="applicationType"></param>
        public ApplicationInstanceManager(string applicationName, string applicationUri, 
            StringCollection baseAddress, StringCollection serverCapabilities, string endpointUrl, 
            string endpointApplicationUri, StringCollection discoveryUrls, StringCollection wellKnownDiscoveryUrls,
            ApplicationType applicationType)
        {
            applicationUri = Utils.ReplaceLocalhost(applicationUri);
            if (discoveryUrls != null)
            {
                //make sure we do not have duplicates
                discoveryUrls = discoveryUrls.Distinct().ToArray();
                for (int i = 0; i < discoveryUrls.Count; i++)
                {
                    discoveryUrls[i] = Utils.ReplaceLocalhost(discoveryUrls[i]);
                }
            }
            if (wellKnownDiscoveryUrls != null)
            {
                //make sure we do not have duplicates
                wellKnownDiscoveryUrls = wellKnownDiscoveryUrls.Distinct().ToArray();
                for (int i = 0; i < wellKnownDiscoveryUrls.Count; i++)
                {
                    wellKnownDiscoveryUrls[i] = Utils.ReplaceLocalhost(wellKnownDiscoveryUrls[i]);
                }
            }
            Console.WriteLine("Checking application configuration.");
            ApplicationConfiguration applicationConfiguration = GetApplicationConfiguration(applicationName, applicationUri, applicationType);
            switch (applicationConfiguration.ApplicationType)
            {
                //Check server configuration
                case ApplicationType.Server:
                {
                        Console.WriteLine("Checking application transport quota configuration.");
                        applicationConfiguration.TransportQuotas = GetTransportQuotas();
                        Console.WriteLine("Checking application server configuration.");
                        applicationConfiguration.ServerConfiguration = GetServerConfiguration(baseAddress, serverCapabilities, null, applicationType);
                        //required in order to connect to the discovery server
                        //GDS session will have to validate the client configuration as part of the application configuration
                        Console.WriteLine("Checking client configuration.");
                        applicationConfiguration.ClientConfiguration = GetClientConfiguration(new StringCollection(), new EndpointDescriptionCollection());
                        break;
                }
                case ApplicationType.DiscoveryServer:
                {
                        Console.WriteLine("Checking application transport quota configuration.");
                        applicationConfiguration.TransportQuotas = GetTransportQuotas();
                        endpointUrl = Utils.ReplaceLocalhost(endpointUrl);
                        endpointApplicationUri = Utils.ReplaceLocalhost(endpointApplicationUri);
                        Console.WriteLine("Checking discovery server configuration.");
                        EndpointDescription discoveryServerRegistrationEndpointDescription =
                            GetDiscoveryServerRegistrationEndpointDescription(endpointUrl, endpointApplicationUri, discoveryUrls);
                        applicationConfiguration.DiscoveryServerConfiguration = GetDiscoveryServerConfiguration(discoveryUrls);
                        Console.WriteLine("Checking application server configuration.");
                        applicationConfiguration.ServerConfiguration = GetServerConfiguration(baseAddress, serverCapabilities,
                            discoveryServerRegistrationEndpointDescription, applicationType);
                        break;
                }
                case ApplicationType.Client:
                {
                        Console.WriteLine("Checking client configuration.");
                        applicationConfiguration.ClientConfiguration = GetClientConfiguration(wellKnownDiscoveryUrls, new EndpointDescriptionCollection());
                        break;
                }
            }
            applicationConfiguration = CheckApplicationInstanceCertificate(applicationConfiguration);
            if (applicationConfiguration == null)
                return;
            Console.WriteLine("Checking global discovery client configuration.");
            ApplicationInstance = new ApplicationInstance
            {
                ApplicationType = applicationConfiguration.ApplicationType,
                ApplicationConfiguration = applicationConfiguration,
                ApplicationName = applicationName,
            };
        }
        #endregion

        #region Private Methods
        private ApplicationConfiguration GetApplicationConfiguration(string applicationName, string applicationUri, ApplicationType applicationType)
        {
            Console.WriteLine("Getting application configuration.");
            ApplicationConfiguration configuration = new ApplicationConfiguration
            {
                ApplicationName = applicationName,
                ApplicationType = applicationType,
                ProductUri = Utils.Format($"urn:{applicationName}:", Dns.GetHostName()),
                ApplicationUri = applicationUri,
                DisableHiResClock = true
            };
            //Setup security configuration
            Console.WriteLine("Getting application security configuration.");
            SecurityConfiguration securityConfiguration = GetSecurityConfiguration(applicationName);
            CertificateValidator certificateValidator = new CertificateValidator();
            certificateValidator.CertificateValidation += CertificateValidatorCertificateValidation;
            certificateValidator.Update(securityConfiguration);
            configuration.CertificateValidator = certificateValidator;
            configuration.SecurityConfiguration = securityConfiguration;
            //Check the certificate.
            Console.WriteLine("Checking application certificate.");
            X509Certificate2 certificate = securityConfiguration.ApplicationCertificate.Find(true).Result;
            if (certificate == null)
            {
                // create a new certificate.
                Console.WriteLine("Creating a new certificate.");
                certificate = CreateApplicationInstanceCertificate(configuration).Result;
                if (certificate == null)
                    throw new Exception("Cannot create certificate.");
            }
            else
            {
                // ensure the certificate is trusted.
                if (configuration.SecurityConfiguration.AddAppCertToTrustedStore)
                {
                    AddToTrustedStore(configuration, certificate).Wait();
                }
            }
            Console.WriteLine("Checking application transport trace configuration.");
            configuration.TraceConfiguration = GetTraceConfiguration();
            return configuration;
        }
        private static SecurityConfiguration GetSecurityConfiguration(string applicationName)
        {
            SecurityConfiguration securityConfiguration = new SecurityConfiguration();
            //ApplicationCertificate
            string applicationCertificateDirectory = AppDomain.CurrentDomain.BaseDirectory + "pki\\trusted";//own
            if (!Directory.Exists(applicationCertificateDirectory))
                Directory.CreateDirectory(applicationCertificateDirectory);

            securityConfiguration.ApplicationCertificate = new CertificateIdentifier
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = applicationCertificateDirectory,
                SubjectName = "CN="+ applicationName+", DC="+Dns.GetHostName(),
            };
            //TrustedIssuerCertificates
            string issuersCertificateDirectory = AppDomain.CurrentDomain.BaseDirectory + "pki\\trusted";//issuers
            if (!Directory.Exists(issuersCertificateDirectory))
                Directory.CreateDirectory(issuersCertificateDirectory);
            securityConfiguration.TrustedIssuerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = issuersCertificateDirectory
            };
            //TrustedPeerCertificates
            string trustedCertificateDirectory = AppDomain.CurrentDomain.BaseDirectory + "pki\\trusted";
            if (!Directory.Exists(trustedCertificateDirectory))
                Directory.CreateDirectory(trustedCertificateDirectory);
            securityConfiguration.TrustedPeerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = trustedCertificateDirectory
            };
            //RejectedCertificateStore
            string rejectedCertificateDirectory = AppDomain.CurrentDomain.BaseDirectory + "pki\\rejected";
            if (!Directory.Exists(rejectedCertificateDirectory))
                Directory.CreateDirectory(rejectedCertificateDirectory);
            securityConfiguration.RejectedCertificateStore = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = rejectedCertificateDirectory
            };
            //UserIssuerCertificates
            string userIssuerCertificateDirectory = AppDomain.CurrentDomain.BaseDirectory + "pki\\trusted";//issuedUser
            if (!Directory.Exists(userIssuerCertificateDirectory))
                Directory.CreateDirectory(userIssuerCertificateDirectory);
            securityConfiguration.UserIssuerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = userIssuerCertificateDirectory
            };
            //TrustedUserCertificates
            string trustedUserCertificateDirectory = AppDomain.CurrentDomain.BaseDirectory + "pki\\trusted";//trustedUser
            if (!Directory.Exists(trustedUserCertificateDirectory))
                Directory.CreateDirectory(trustedUserCertificateDirectory);
            securityConfiguration.TrustedUserCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = trustedUserCertificateDirectory
            };
            securityConfiguration.AddAppCertToTrustedStore = true;
            securityConfiguration.AutoAcceptUntrustedCertificates = true;
            securityConfiguration.RejectSHA1SignedCertificates = false;
            securityConfiguration.NonceLength = 32;
            //false for server
            securityConfiguration.RejectUnknownRevocationStatus = false;
            /** For CA signed certificates, this flag controls whether the server shall send the complete certificate chain instead of just sending the certificate. 
             * This affects the GetEndpoints and CreateSession service.**/
            securityConfiguration.SendCertificateChain = true;
            securityConfiguration.MinimumCertificateKeySize = CertificateFactory.defaultKeySize;
            securityConfiguration.Validate();
            return securityConfiguration;
        }
        private static ServerConfiguration GetServerConfiguration(StringCollection baseAddress, StringCollection serverCapabilities, EndpointDescription registrationEndpoint,ApplicationType applicationType)
        {
            ServerConfiguration serverConfiguration = new ServerConfiguration
            {
                BaseAddresses = baseAddress,
                MinRequestThreadCount = 5,
                MaxRequestThreadCount = 100,
                MaxQueuedRequestCount = 2000,
                ServerCapabilities = serverCapabilities,
                SupportedPrivateKeyFormats = new StringCollection { "PFX", "PEM" },
                MultiCastDnsEnabled = false,
                MaxRegistrationInterval = 30000,
                MinSubscriptionLifetime = 10000,
                MaxPublishRequestCount = 20,
                MaxSubscriptionCount = 100,
                MaxEventQueueSize = 10000,
                ServerProfileArray = new StringCollection { "Standard UA Server Profile" },
                ShutdownDelay = 5,
                DiagnosticsEnabled = false,
                MaxSessionCount = 100,
                MinSessionTimeout = 10000,
                MaxSessionTimeout = 3600000,
                MaxBrowseContinuationPoints = 10,
                MaxQueryContinuationPoints = 10,
                MaxHistoryContinuationPoints = 100,
                MaxRequestAge = 600000,
                MinPublishingInterval = 100,
                MaxPublishingInterval = 3600000,
                PublishingResolution = 50,
                MaxSubscriptionLifetime = 3600000,
                MaxMessageQueueSize = 10,
                MaxNotificationQueueSize = 1000,
                MaxNotificationsPerPublish = 1000,
                MinMetadataSamplingInterval = 1000,
                NodeManagerSaveFile = "Opc.Ua.Server.nodes.xml",
                MaxTrustListSize = 0,
                AvailableSamplingRates = new SamplingRateGroupCollection
                {
                    new SamplingRateGroup{Count = 20, Increment = 5, Start = 5},
                    new SamplingRateGroup{Count = 4, Increment = 100, Start = 100},
                    new SamplingRateGroup{Count = 2, Increment = 250, Start = 500},
                    new SamplingRateGroup{Count = 20, Increment = 500, Start = 1000}
                }
            };
            ServerSecurityPolicyCollection serverSecurityPolicyCollection = new ServerSecurityPolicyCollection();
            UserTokenPolicyCollection userTokenPolicyCollection = new UserTokenPolicyCollection();
            userTokenPolicyCollection = new UserTokenPolicyCollection
                {
                    new UserTokenPolicy
                    {
                        TokenType = UserTokenType.Anonymous,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#None"
                    },
                    new UserTokenPolicy
                    {
                        TokenType = UserTokenType.UserName,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Basic256Sha256"
                    },
                    new UserTokenPolicy
                    {
                        TokenType = UserTokenType.Certificate
                    }
                };
            serverSecurityPolicyCollection = new ServerSecurityPolicyCollection
                {
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.SignAndEncrypt,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Basic256Sha256"
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.SignAndEncrypt,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Aes128_Sha256_RsaOaep"
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.SignAndEncrypt,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Aes256_Sha256_RsaPss"
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.Sign,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Basic256Sha256"
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.Sign,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Aes128_Sha256_RsaOaep"
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.Sign,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Aes256_Sha256_RsaPss"
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.None,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#None"
                    }
                };
            serverConfiguration.RegistrationEndpoint = registrationEndpoint;
            if (applicationType == ApplicationType.DiscoveryServer)
            {
                serverConfiguration.MaxRegistrationInterval = 0;
                serverConfiguration.DiagnosticsEnabled = true;
            }
            serverConfiguration.SecurityPolicies = serverSecurityPolicyCollection;
            serverConfiguration.UserTokenPolicies = userTokenPolicyCollection;
            serverConfiguration.Validate();
            return serverConfiguration;
        }
        private static DiscoveryServerConfiguration GetDiscoveryServerConfiguration(StringCollection discoveryUrls)
        {
            Console.WriteLine("Checking discovery server configuration.");
            DiscoveryServerConfiguration discoveryServerConfiguration = new DiscoveryServerConfiguration
            {
                BaseAddresses = discoveryUrls,
                SecurityPolicies = new ServerSecurityPolicyCollection
                {
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.SignAndEncrypt,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Basic128Rsa15"
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.SignAndEncrypt,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Basic256",
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.Sign
                    },
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.None,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#None"
                    }
                },
                ServerNames = new LocalizedTextCollection(new LocalizedText[] { "UA Global Discovery Server" })
            };
            discoveryServerConfiguration.Validate();
            return discoveryServerConfiguration;
        }
        private static EndpointDescription GetDiscoveryServerRegistrationEndpointDescription(string endpointUrl, string endpointApplicationUri, StringCollection discoveryUrls)
        {
            EndpointDescription registrationEndpoint = new EndpointDescription
            {
                EndpointUrl = endpointUrl,
                Server = new ApplicationDescription
                {
                    ApplicationUri = endpointApplicationUri,
                    ApplicationType = ApplicationType.DiscoveryServer,
                    DiscoveryUrls = discoveryUrls
                },
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Basic256",
                UserIdentityTokens = new UserTokenPolicyCollection
                {
                    new UserTokenPolicy
                    {
                        TokenType = UserTokenType.Anonymous,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#None"
                    },
                    new UserTokenPolicy
                    {
                        TokenType = UserTokenType.UserName,
                        SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Basic256Sha256"
                    },
                    new UserTokenPolicy
                    {
                        TokenType = UserTokenType.Certificate
                    }
                }
            };
            return registrationEndpoint;
        }
        private static ClientConfiguration GetClientConfiguration(StringCollection wellKnownDiscoveryUrls, EndpointDescriptionCollection discoveryServersEndpointDescriptionCollection)
        {
            ClientConfiguration clientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = 600000,
                MinSubscriptionLifetime = 10000,
                WellKnownDiscoveryUrls = wellKnownDiscoveryUrls,
                DiscoveryServers = discoveryServersEndpointDescriptionCollection
            };
            clientConfiguration.Validate();
            return clientConfiguration;
        }
        private static TransportQuotas GetTransportQuotas()
        {
            TransportQuotas transportQuotas = new TransportQuotas
            {
                OperationTimeout = 600000,
                MaxStringLength = 1048576,
                MaxByteStringLength = 1048576,
                MaxArrayLength = 65535,
                MaxMessageSize = 4194304,
                MaxBufferSize = 65535,
                ChannelLifetime = 300000,
                SecurityTokenLifetime = 3600000
            };
            return transportQuotas;
        }
        private static TraceConfiguration GetTraceConfiguration()
        {
            string traceLogsDirectory = AppDomain.CurrentDomain.BaseDirectory + "logs";
            if (!Directory.Exists(traceLogsDirectory))
                Directory.CreateDirectory(traceLogsDirectory);
            string traceLogFile = traceLogsDirectory + "\\server.log.txt";
            if (!File.Exists(traceLogFile))
                File.Create(traceLogFile).Close();
            TraceConfiguration traceConfiguration = new TraceConfiguration
            {
                OutputFilePath = traceLogFile, DeleteOnLoad = true, TraceMasks = 515
            };
            return traceConfiguration;
        }

        #region Certificate Helper
        private static async Task DeleteApplicationInstanceCertificate(ApplicationConfiguration applicationConfiguration)
        {
            // create a default certificate id none specified.
            CertificateIdentifier certificateIdentifier = applicationConfiguration.SecurityConfiguration.ApplicationCertificate;
            if (certificateIdentifier == null)
                return;
            // delete private key.
            X509Certificate2 certificate = await certificateIdentifier.Find();
            // delete trusted peer certificate.
            if (applicationConfiguration.SecurityConfiguration?.TrustedPeerCertificates != null)
            {
                string thumbprint = certificateIdentifier.Thumbprint;
                if (certificate != null)
                    thumbprint = certificate.Thumbprint;
                using (ICertificateStore store = applicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.OpenStore())
                {
                    await store.Delete(thumbprint);
                }
            }
            // delete private key.
            if (certificate != null)
            {
                using (ICertificateStore store = certificateIdentifier.OpenStore())
                {
                    await store.Delete(certificate.Thumbprint);
                }
            }
        }
        private static async Task DeleteExistingCertificateFromStore(string certificateStorePath, string certificatePublicKeyPath, string certificatePrivateKeyPath)
        {
            if (string.IsNullOrEmpty(certificateStorePath))
                return;
            using (DirectoryCertificateStore store = (DirectoryCertificateStore)global::Opc.Ua.CertificateStoreIdentifier.OpenStore(certificateStorePath))
            {
                X509Certificate2Collection certificates = await store.Enumerate();
                foreach (X509Certificate2 certificate in certificates)
                {
                    if (store.GetPrivateKeyFilePath(certificate.Thumbprint) != null)
                        continue;
                    List<string> fields = Utils.ParseDistinguishedName(certificate.Subject);
                    if (fields.Contains("CN=UA Local Discovery Server"))
                        continue;
                    if (store is DirectoryCertificateStore ds)
                    {
                        string path = Utils.GetAbsoluteFilePath(certificatePublicKeyPath, true, false, false);
                        if (path != null)
                        {
                            if (string.Compare(path, ds.GetPublicKeyFilePath(certificate.Thumbprint), StringComparison.OrdinalIgnoreCase) == 0)
                                continue;
                        }
                        path = Utils.GetAbsoluteFilePath(certificatePrivateKeyPath, true, false, false);
                        if (path != null)
                        {
                            if (string.Compare(path, ds.GetPrivateKeyFilePath(certificate.Thumbprint), StringComparison.OrdinalIgnoreCase) == 0)
                                continue;
                        }
                    }
                    await store.Delete(certificate.Thumbprint);
                }
            }
        }
        private static async Task AddToTrustedStore(ApplicationConfiguration applicationConfiguration, X509Certificate2 certificate)
        {
            ICertificateStore store = applicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.OpenStore();
            try
            {
                // check if it already exists.
                X509Certificate2Collection existingCertificates = await store.FindByThumbprint(certificate.Thumbprint);
                if (existingCertificates.Count > 0)
                    return;
                List<string> subjectName = Utils.ParseDistinguishedName(certificate.Subject);
                // check for old certificate.
                X509Certificate2Collection certificates = await store.Enumerate();
                for (int index = 0; index < certificates.Count; index++)
                {
                    if (!Utils.CompareDistinguishedName(certificates[index], subjectName))
                        continue;
                    if (certificates[index].Thumbprint == certificate.Thumbprint)
                        return;
                    await store.Delete(certificates[index].Thumbprint);
                    break;
                }
                // add new certificate.
                X509Certificate2 publicKey = new X509Certificate2(certificate.RawData);
                await store.Add(publicKey);
            }
            finally
            {
                store.Close();
            }
        }
        private static async Task<X509Certificate2> CreateApplicationInstanceCertificate(ApplicationConfiguration applicationConfiguration,
            ushort minimumKeySize = CertificateFactory.defaultKeySize, ushort lifeTimeInMonths = CertificateFactory.defaultLifeTime)
        {
            // delete any existing certificate.
            await DeleteApplicationInstanceCertificate(applicationConfiguration);
            // get the domains from the configuration file.
            List<string> serverDomainNames = applicationConfiguration.GetServerDomainNames().Distinct().ToList();
            if (serverDomainNames.Count == 0)
                serverDomainNames.Add(Utils.GetHostName());
            //ApplicationCertificate
            CertificateIdentifier applicationCertificate = applicationConfiguration.SecurityConfiguration.ApplicationCertificate;
            if (applicationCertificate.StoreType == CertificateStoreType.Directory)
            {
                Utils.GetAbsoluteDirectoryPath(applicationCertificate.StorePath, true, true, true);
            }
            X509Certificate2 certificate = CertificateFactory.CreateCertificate(
                applicationCertificate.StoreType,
                applicationCertificate.StorePath,
                null,                               
                applicationConfiguration.ApplicationUri,
                applicationConfiguration.ApplicationName,
                applicationCertificate.SubjectName,
                serverDomainNames,
                minimumKeySize,
                DateTime.UtcNow - TimeSpan.FromDays(1),
                lifeTimeInMonths,
                CertificateFactory.defaultHashSize,
                false,
                null,
                null
            );
            applicationCertificate.Certificate = certificate;
            // ensure the certificate is trusted.
            if (applicationConfiguration.SecurityConfiguration.AddAppCertToTrustedStore)
            {
                await AddToTrustedStore(applicationConfiguration, certificate);
            }
            return certificate;
        }
        private static ApplicationConfiguration CheckApplicationInstanceCertificate(ApplicationConfiguration applicationConfiguration, 
            ushort minimumKeySize = CertificateFactory.defaultKeySize, ushort lifeTimeInMonths = CertificateFactory.defaultLifeTime)
        {
            Console.WriteLine("Checking application instance certificate.");
            if (applicationConfiguration == null)
                return null; 
            //Find the existing certificate.
            CertificateIdentifier applicationCertificateIdentifier = applicationConfiguration.SecurityConfiguration.ApplicationCertificate;
            if (applicationCertificateIdentifier == null)
            {
                Console.WriteLine("Configuration file does not specify a certificate.");
                return null;
            }
            X509Certificate2 applicationX509Certificate = applicationCertificateIdentifier.Find(true).Result;
            //Check that it is ok.
            if (applicationX509Certificate != null)
            {
                Console.WriteLine($"Checking application instance certificate. {applicationX509Certificate.Subject}");
                try
                {
                    //Validate certificate.
                    applicationConfiguration.CertificateValidator.Validate(applicationX509Certificate);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error validating certificate. Exception: { ex.Message}. Use certificate anyway?");
                    return null;
                }
                //Check key size.
                if (minimumKeySize > applicationX509Certificate.GetRSAPublicKey().KeySize)
                {
                    Console.WriteLine($"The key size ({applicationX509Certificate.GetRSAPublicKey().KeySize}) in the certificate is less than the minimum provided ({minimumKeySize}). Use certificate anyway?");
                    return null;
                }
                //Check domains.
                if (applicationConfiguration.ApplicationType != ApplicationType.Client)
                {
                    Console.WriteLine($"Checking domains in certificate. {applicationX509Certificate.Subject}");
                    List<string> serverDomainNames = applicationConfiguration.GetServerDomainNames().Distinct().ToList();
                    List<string> certificateDomainNames = Utils.GetDomainsFromCertficate(applicationX509Certificate).Distinct().ToList();
                    string computerName = Utils.GetHostName();
                    //Get IP addresses.
                    IPAddress[] addresses = null;
                    for (int index = 0; index < serverDomainNames.Count; index++)
                    {
                        if (Utils.FindStringIgnoreCase(certificateDomainNames, serverDomainNames[index]))
                            continue;
                        if (string.Compare(serverDomainNames[index], "localhost", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (Utils.FindStringIgnoreCase(certificateDomainNames, computerName))
                                continue;
                            //Check for aliases.
                            //Get IP addresses only if necessary.
                            if (addresses == null)
                                addresses = Utils.GetHostAddresses(computerName).Result;
                            // check for ip addresses.
                            bool found = addresses.Any(x => Utils.FindStringIgnoreCase(certificateDomainNames, x.ToString()));
                            if (found)
                                continue;
                        }
                        Console.WriteLine($"The server is configured to use domain '{serverDomainNames[index]}' which does not appear in the certificate. Use certificate?");
                        break;
                    }
                }
                //Check uri.
                string applicationUri = Utils.GetApplicationUriFromCertificate(applicationX509Certificate);
                if (string.IsNullOrEmpty(applicationUri))
                {
                    Console.WriteLine("The Application URI could not be read from the certificate.");
                }
                else
                {
                    Console.WriteLine("The Application URI found in certificate.");
                    applicationConfiguration.ApplicationUri = applicationUri;
                }
            }
            else
            {
                // check for missing private key.
                applicationX509Certificate = applicationCertificateIdentifier.Find(false).Result;
                if (applicationX509Certificate != null)
                {
                    Console.WriteLine($"Cannot access certificate private key. Subject={applicationX509Certificate.Subject}");
                }
                // check for missing thumbprint.
                if (!string.IsNullOrEmpty(applicationCertificateIdentifier.Thumbprint))
                {
                    if (!string.IsNullOrEmpty(applicationCertificateIdentifier.SubjectName))
                    {
                        CertificateIdentifier certificateIdentifier = new CertificateIdentifier
                        {
                            StoreType = applicationCertificateIdentifier.StoreType,
                            StorePath = applicationCertificateIdentifier.StorePath,
                            SubjectName = applicationCertificateIdentifier.SubjectName
                        };
                        applicationX509Certificate = certificateIdentifier.Find(true).Result;
                    }
                    if (applicationX509Certificate != null)
                    {
                        Console.WriteLine("Thumbprint was explicitly specified in the configuration." +
                                          "\r\nAnother certificate with the same subject name was found." +
                                          "\r\nUse it instead?\r\n" +
                                          $"\r\nRequested: {applicationCertificateIdentifier.SubjectName}" +
                                          $"\r\nFound: {applicationX509Certificate.Subject}");
                        return null;
                    }
                    Console.WriteLine("Thumbprint was explicitly specified in the configuration. Cannot generate a new certificate.");
                    return null;
                }
                applicationX509Certificate = CreateApplicationInstanceCertificate(applicationConfiguration, minimumKeySize, lifeTimeInMonths).Result;
            }
            if (applicationX509Certificate != null)
            {
                //Update configuration.
                applicationConfiguration.SecurityConfiguration.ApplicationCertificate.Certificate = applicationX509Certificate;
                return applicationConfiguration;
            }
            Console.WriteLine("There is no cert with subject in the configuration." + "\r\n Please generate a cert for your application");
            return null;
        }
        #endregion
        private static string SelectServerUrl(IList<string> discoveryUrls)
        {
            if (discoveryUrls == null || discoveryUrls.Count == 0)
                return null;
            string url = null;
            // always use opc.tcp by default.
            foreach (string discoveryUrl in discoveryUrls)
            {
                if (!discoveryUrl.StartsWith("opc.tcp://", StringComparison.Ordinal))
                    continue;
                url = discoveryUrl;
                break;
            }
            // try HTTPS if no opc.tcp.
            if (url == null)
            {
                foreach (string discoveryUrl in discoveryUrls)
                {
                    if (!discoveryUrl.StartsWith("https://", StringComparison.Ordinal))
                        continue;
                    url = discoveryUrl;
                    break;
                }
            }
            // use the first URL if nothing else.
            return url ?? discoveryUrls[0];
        }
        private ApplicationRecordDataType FindRegisteredApplications()
        {
            if (!GlobalDiscoveryServerClient.IsConnected)
                return null;
            //Application user access required
            ApplicationRecordDataType[] records = GlobalDiscoveryServerClient.FindApplication(ApplicationInstance.ApplicationConfiguration.ApplicationUri);
            if (records != null && records.Length > 0)
                return records[0];     
            ApplicationRecordDataType applicationRecordDataType = new ApplicationRecordDataType
            {
                ApplicationType = ApplicationInstance.ApplicationConfiguration.ApplicationType,
                ApplicationNames = new LocalizedText[] { ApplicationInstance.ApplicationConfiguration.ApplicationName },
                ApplicationUri = ApplicationInstance.ApplicationConfiguration.ApplicationUri,
                ProductUri = ApplicationInstance.ApplicationConfiguration.ProductUri,
            };
            if (ApplicationInstance.ApplicationConfiguration.ApplicationType != ApplicationType.Client)
            {
                if (ApplicationInstance.ApplicationConfiguration.ServerConfiguration == null)
                    return null;
                if (ApplicationInstance.ApplicationConfiguration.ServerConfiguration.BaseAddresses == null)
                    return null;
                applicationRecordDataType.DiscoveryUrls =
                    ApplicationInstance.ApplicationConfiguration.ServerConfiguration.BaseAddresses;
                applicationRecordDataType.ServerCapabilities =
                    ApplicationInstance.ApplicationConfiguration.ServerConfiguration.ServerCapabilities;
            }
            NodeId applicationId = GlobalDiscoveryServerClient.RegisterApplication(applicationRecordDataType);
            if (applicationId == null)
                return null;
            applicationRecordDataType.ApplicationId = applicationId;
            return applicationRecordDataType;
        }
        #endregion

        #region Public Client Methods
        /// <summary>
        /// The QueryServers Method is similar to the FindServers service except that it
        /// provides more advanced search and filter criteria
        /// </summary>
        /// <returns>returns list of Server On Network</returns>
        public List<ServerOnNetwork> QueryServers()
        {
            if (!GlobalDiscoveryServerClient.IsConnected)
                return null;
            List<ServerOnNetwork> servers = GlobalDiscoveryServerClient.QueryServers(0, "", "", "", new List<string>()).ToList();
            return servers;
        }
        public bool ConnectToServer(string serverDiscoveryEndpoint, string username, string password)
        {
            try
            {
                IUserIdentity userIdentity = null;
                //create end point configuration using application instance transport quotas
                EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(ApplicationInstance.ApplicationConfiguration);
                //connects to the end point and get the best available available configurations including URL scheme
                SessionEndpointDescription = CoreClientUtils.SelectEndpoint(serverDiscoveryEndpoint, true, 5000);
                //set user identity if user policy is required
                if (SessionEndpointDescription.FindUserTokenPolicy(UserTokenType.Certificate, (string)null) != null)
                {
                    userIdentity = new UserIdentity(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate.Certificate);
                }
                else if (SessionEndpointDescription.FindUserTokenPolicy(UserTokenType.UserName, (string)null) != null &&
                    !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(username))
                {
                    userIdentity = new UserIdentity(username, password);
                }
                else if (SessionEndpointDescription.FindUserTokenPolicy(UserTokenType.Anonymous, (string)null) != null)
                {
                    userIdentity = null;
                }
                ConfiguredEndpoint configuredEndpoint = new ConfiguredEndpoint(null, SessionEndpointDescription, endpointConfiguration);               
                Session = Session.Create(
                    ApplicationInstance.ApplicationConfiguration,
                    configuredEndpoint,
                    false,
                    ApplicationInstance.ApplicationName,
                    60000,
                    userIdentity,
                    new[] { "" }
                    ).Result;
                if (Session.Connected)
                {
                    Session.KeepAlive += SessionKeepAlive;
                    BrowseReferenceDescription();
                    ComplexTypeSystem typeSystemLoader = new ComplexTypeSystem(Session);
                    typeSystemLoader.Load();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connecting to GDS exception: {ex.StackTrace}");
                return false;
            }
        }
        public List<ReferenceDescription> GetRootObjectReferenceDescriptions()
        {
            return BrowseReferenceDescription(ReferenceDescriptionDictionary[Root.NameObjects]);
        }
        public List<ReferenceDescription> GetControllersReferenceDescriptions()
        {
            //We need to query the parent which will give us the information about the controllers
            GetRootObjectReferenceDescriptions();
            return BrowseReferenceDescription(ReferenceDescriptionDictionary[NameObject.Controllers]);
        }
        public List<ReferenceDescription> GetControllersAttributeReferenceDescriptions()
        {
            GetRootObjectReferenceDescriptions();
            GetControllersReferenceDescriptions();
            return BrowseReferenceDescription(ReferenceDescriptionDictionary[Process.Name]);
        }
        public List<ReferenceDescription> BrowseReferenceDescription(ReferenceDescription parentReferenceDescription = null)
        {
            BrowseDescriptionCollection browseDescriptionCollection = new BrowseDescriptionCollection();
            BrowseDescription browseDescription = new BrowseDescription
            {
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                IncludeSubtypes = true,
                NodeClassMask = (uint)NodeClass.Unspecified,
                ResultMask = (uint)BrowseResultMask.All
            };
            //Define the node to browse
            NodeId nodeToBrowse;
            if (_referenceDescriptions == null || parentReferenceDescription == null)
            {
                nodeToBrowse = new NodeId(global::Opc.Ua.Objects.RootFolder,0);
                ExtendedReferenceDescriptions = new List<ExtendedReferenceDescription>();
                ReferenceDescriptionDictionary = new Dictionary<string, ReferenceDescription>();
                _referenceDescriptions = new List<List<ReferenceDescription>>();
            }
            else
            {
                nodeToBrowse = ExpandedNodeId.ToNodeId(parentReferenceDescription.NodeId, Session.NamespaceUris); 
            }
            browseDescription.NodeId = nodeToBrowse;
            browseDescriptionCollection.Add(browseDescription);
            RequestHeader requestHeader = null;
            ViewDescription viewDescription = null;
            const uint requestedMaxReferencesPerNode = 100;
            // Call browse service.
            Session.Browse(
                requestHeader,
                viewDescription,
                requestedMaxReferencesPerNode,
                browseDescriptionCollection,
                out BrowseResultCollection browseResultCollection,
                out DiagnosticInfoCollection diagnosticInfoCollection);
            ClientBase.ValidateResponse(browseResultCollection, browseDescriptionCollection);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfoCollection, browseDescriptionCollection);
            if (browseResultCollection == null || !browseResultCollection.Any())
                return null;
            //Flatten the reference descriptions
            List<ReferenceDescription> referenceDescriptions = browseResultCollection
                .Where(x => x.References != null)
                .Select(y => y.References.ToList()).SelectMany(i => i).ToList();
            if (!referenceDescriptions.Any())
                return null;
            if (ReferenceDescriptionDictionary == null || !ReferenceDescriptionDictionary.Any())
            {
                //We know that this 3 types / object always exist
                if (!ReferenceDescriptionDictionary.ContainsKey(Root.NameObjects) || ReferenceDescriptionDictionary[Root.NameObjects] == null)
                    ReferenceDescriptionDictionary[Root.NameObjects] = referenceDescriptions.FirstOrDefault(x =>
                    string.Equals(x.BrowseName.Name, Root.NameObjects, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(x.DisplayName.Text, Root.NameObjects, StringComparison.CurrentCultureIgnoreCase));
                if (!ReferenceDescriptionDictionary.ContainsKey(Root.NameTypes) || ReferenceDescriptionDictionary[Root.NameTypes] == null)
                    ReferenceDescriptionDictionary[Root.NameTypes] = referenceDescriptions.FirstOrDefault(x =>
                    string.Equals(x.BrowseName.Name, Root.NameTypes, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(x.DisplayName.Text, Root.NameTypes, StringComparison.CurrentCultureIgnoreCase));
                if (!ReferenceDescriptionDictionary.ContainsKey(Root.NameViews) || ReferenceDescriptionDictionary[Root.NameViews] == null)
                    ReferenceDescriptionDictionary[Root.NameViews] = referenceDescriptions.FirstOrDefault(x =>
                    string.Equals(x.BrowseName.Name, Root.NameViews, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(x.DisplayName.Text, Root.NameViews, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                //This section contains only dynamic types / objects
                //We need to iterate through the list
                foreach (ReferenceDescription referenceDescription in referenceDescriptions)
                {
                    if (referenceDescription.NodeClass != NodeClass.Method &&
                        referenceDescription.NodeClass != NodeClass.Variable)
                    {
                        ReferenceDescriptionDictionary[referenceDescription.BrowseName.Name] = referenceDescription;
                    }
                    else
                    {
                        ExtendedReferenceDescription extendedReferenceDescription = ExtendedReferenceDescriptions
                            .FirstOrDefault(x => x.ParentReferenceDescription == parentReferenceDescription);
                        if (extendedReferenceDescription == null)
                        {
                            extendedReferenceDescription = new ExtendedReferenceDescription(parentReferenceDescription);
                            ExtendedReferenceDescriptions.Add(extendedReferenceDescription);
                        }
                        switch (referenceDescription.NodeClass)
                        {
                            case NodeClass.Method:
                                extendedReferenceDescription.MethodReferenceDescriptions.Add(referenceDescription);
                                break;
                            case NodeClass.Variable:
                                extendedReferenceDescription.VariableReferenceDescriptions.Add(referenceDescription);
                                break;
                        }                      
                    }         
                }
            }
            return referenceDescriptions;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Create a session with the GDS.
        /// GlobalDiscoveryServerClient class has encapsulated/wrapped the GDS call services
        /// A GDS is an OPC UA Server which allows Clients to search for Servers in the administrative domain.
        /// It may also provide Certificate Services
        /// It provides Methods that allow applications to search for other applications
        /// </summary>
        /// <param name="globalDiscoveryServerEndpoint"></param>
        /// <param name="username">username to identify user</param>
        /// <param name="password">user password</param>
        /// <returns>True if connection is successful</returns>
        public bool ConnectToGlobalDiscoveryServer(string globalDiscoveryServerEndpoint, string username, string password)
        {
            try
            {
                IUserIdentity userIdentity = null;
                //create end point configuration using application instance transport quotas
                EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(ApplicationInstance.ApplicationConfiguration);
                //connects to the end point and get the best available available configurations including URL scheme
                EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(globalDiscoveryServerEndpoint, true, 5000);
                //set user identity if user policy is required
                if (endpointDescription.FindUserTokenPolicy(UserTokenType.UserName, (string)null) != null)
                    userIdentity = new UserIdentity(username, password);
                ConfiguredEndpoint configuredEndpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);
                //create a global discovery server client instance and within this class is the session
                GlobalDiscoveryServerClient = new GlobalDiscoveryServerClient(ApplicationInstance, globalDiscoveryServerEndpoint, userIdentity);
                GlobalDiscoveryServerClient.PreferredLocales = new[] { "" };
                GlobalDiscoveryServerClient.KeepAlive += SessionKeepAlive;
                GlobalDiscoveryServerClient.ServerStatusChanged += MonitoredItemStatusNotification;
                //connect to the GDS
                GlobalDiscoveryServerClient.Connect(configuredEndpoint).Wait();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connecting to GDS exception: {ex.StackTrace}");
                return false;
            }
        }

        private X509Certificate2 _certificate;

        public bool RequestNewCertificatePullMode()
        {
            //The person asking for this certificate has to be authorized
            //user: appadmin pass:demo
            try
            {
                // check if we already have a private key
                NodeId requestId = null;
                if (!string.IsNullOrEmpty(RegisteredApplication.CertificateStorePath))
                {
                    CertificateIdentifier id = new CertificateIdentifier
                    {
                        StoreType = global::Opc.Ua.CertificateStoreIdentifier.DetermineStoreType(RegisteredApplication.CertificateStorePath),
                        StorePath = RegisteredApplication.CertificateStorePath,
                        SubjectName = RegisteredApplication.CertificateSubjectName.Replace("localhost", Utils.GetHostName())
                    };
                    _certificate = id.Find(true).Result;
                    if (_certificate != null && _certificate.HasPrivateKey)
                    {
                        _certificate = id.LoadPrivateKey(null).Result;
                    }
                }

                if (!string.IsNullOrEmpty(RegisteredApplication.CertificatePrivateKeyPath))
                {
                    FileInfo file = new FileInfo(RegisteredApplication.CertificatePrivateKeyPath);
                }

                List<string> domainNames = RegisteredApplication.GetDomainNames(_certificate);
                if (_certificate == null)
                {
                    // no private key
                    requestId = GlobalDiscoveryServerClient.StartNewKeyPairRequest(
                        RegisteredApplication.ApplicationId,
                        null,//certificate groupe id
                        null,//certificate type id
                        RegisteredApplication.CertificateSubjectName.Replace("localhost", Utils.GetHostName()),
                        domainNames,
                        "PFX",
                        null);
                }
                else
                {
                    X509Certificate2 csrCertificate = null;
                    if (_certificate.HasPrivateKey)
                    {
                        csrCertificate = _certificate;
                    }
                    byte[] certificateRequest = CertificateFactory.CreateSigningRequest(csrCertificate, domainNames);
                    requestId = GlobalDiscoveryServerClient.StartSigningRequest(RegisteredApplication.ApplicationId, null, null, certificateRequest);
                }
                RegisteredApplication.CertificateRequestId = requestId.ToString();
                CertificateRequestChecker();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request new certificate pull mode exception: \r\n{ex.StackTrace}");
                return false;
            }
            return true;
        }

        private void CertificateRequestChecker()
        {
            try
            {
                NodeId requestId = NodeId.Parse(RegisteredApplication.CertificateRequestId);
                byte[] privateKeyPFX = null;
                byte[][] issuerCertificates = null;
                byte[] certificate = GlobalDiscoveryServerClient.FinishRequest(
                    RegisteredApplication.ApplicationId,
                    requestId,
                    out privateKeyPFX,
                    out issuerCertificates);

                if (certificate == null)
                {
                    Task.Delay(1000);
                    CertificateRequestChecker();
                    return;
                }
                X509Certificate2 newCert = new X509Certificate2(certificate);
                if (!string.IsNullOrEmpty(RegisteredApplication.CertificateStorePath) && !String.IsNullOrEmpty(RegisteredApplication.CertificateSubjectName))
                {
                    CertificateIdentifier cid = new CertificateIdentifier()
                    {
                        StorePath = RegisteredApplication.CertificateStorePath,
                        StoreType = global::Opc.Ua.CertificateStoreIdentifier.DetermineStoreType(RegisteredApplication.CertificateStorePath),
                        SubjectName = RegisteredApplication.CertificateSubjectName.Replace("localhost", Utils.GetHostName())
                    };
                    // update store
                    using (var store = global::Opc.Ua.CertificateStoreIdentifier.OpenStore(RegisteredApplication.CertificateStorePath))
                    {
                        // if we used a CSR, we already have a private key and therefore didn't request one from the GDS
                        // in this case, privateKey is null
                        if (privateKeyPFX == null)
                        {
                            X509Certificate2 oldCertificate = cid.Find(true).Result;
                            if (oldCertificate != null && oldCertificate.HasPrivateKey)
                            {
                                oldCertificate = cid.LoadPrivateKey(string.Empty).Result;
                                newCert = CertificateFactory.CreateCertificateWithPrivateKey(newCert, oldCertificate);
                                store.Delete(oldCertificate.Thumbprint);
                            }
                            else
                            {
                                throw new ServiceResultException("Failed to merge signed certificate with the private key.");
                            }
                        }
                        else
                        {
                            newCert = new X509Certificate2(privateKeyPFX, string.Empty, X509KeyStorageFlags.Exportable);
                            newCert = CertificateFactory.Load(newCert, true);
                        }
                        store.Add(newCert);
                    }
                }
                else
                {
                    string absoluteCertificatePublicKeyPath = Utils.GetAbsoluteFilePath(RegisteredApplication.CertificatePublicKeyPath, true, false, false) ?? RegisteredApplication.CertificatePublicKeyPath;
                    FileInfo file = new FileInfo(absoluteCertificatePublicKeyPath);
                    bool replaceCertificate = true;
                    if (replaceCertificate)
                    {
                        byte[] exportedCert;
                        if (string.Compare(file.Extension, ".PEM", true) == 0)
                        {
                            exportedCert = CertificateFactory.ExportCertificateAsPEM(newCert);
                        }
                        else
                        {
                            exportedCert = newCert.Export(X509ContentType.Cert);
                        }
                        File.WriteAllBytes(absoluteCertificatePublicKeyPath, exportedCert);
                    }
                }

                // update trust list.
                if (!string.IsNullOrEmpty(RegisteredApplication.TrustListStorePath))
                {
                    using (ICertificateStore store = global::Opc.Ua.CertificateStoreIdentifier.OpenStore(RegisteredApplication.TrustListStorePath))
                    {
                        foreach (byte[] issuerCertificate in issuerCertificates)
                        {
                            X509Certificate2 x509 = new X509Certificate2(issuerCertificate);
                            X509Certificate2Collection certs = store.FindByThumbprint(x509.Thumbprint).Result;
                            if (certs.Count == 0)
                            {
                                store.Add(new X509Certificate2(issuerCertificate)).Wait();
                            }
                        }
                    }
                }

                _certificate = newCert;
            }
            catch (Exception exception)
            {
                if (exception is ServiceResultException sre && sre.StatusCode == StatusCodes.BadNothingToDo)
                {
                    return;
                }
            }
        }

        public bool RegisterApplication()
        {
            if (!GlobalDiscoveryServerClient.IsConnected)
                return false;
            //Application user access required
            RegisteredApplication = new RegisteredApplication(); 
            switch (ApplicationInstance.ApplicationConfiguration.ApplicationType)
            {
                case ApplicationType.Client:
                    RegisteredApplication.RegistrationType = RegistrationType.ClientPull;
                    break;
                case ApplicationType.Server:
                    RegisteredApplication.RegistrationType = RegistrationType.ServerPull;
                    break;
            }
            ApplicationRecordDataType applicationRecordDataType = FindRegisteredApplications();
            if (applicationRecordDataType == null)
                return false;
            if (applicationRecordDataType.ApplicationType != ApplicationType.Client)
            {
                RegisteredApplication.ServerUrl = SelectServerUrl(applicationRecordDataType.DiscoveryUrls);
                RegisteredApplication.ServerCapability = applicationRecordDataType.ServerCapabilities?.ToArray();
            }
            RegisteredApplication.ApplicationId = applicationRecordDataType.ApplicationId?.ToString();
            RegisteredApplication.ApplicationUri = applicationRecordDataType.ApplicationUri;
            RegisteredApplication.ApplicationName = applicationRecordDataType.ApplicationNames != null &&
                                                    applicationRecordDataType.ApplicationNames.Count > 0 && applicationRecordDataType.ApplicationNames[0].Text != null
                    ? applicationRecordDataType.ApplicationNames[0].Text : null;
            RegisteredApplication.ProductUri = applicationRecordDataType.ProductUri;
            RegisteredApplication.DiscoveryUrl = applicationRecordDataType.DiscoveryUrls?.ToArray();  
            // copy the security settings.
            RegisteredApplication.ConfigurationFile = null;
            if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration == null)
                return true;
            global::Opc.Ua.Security.CertificateIdentifier certificateIdentifier = SecuredApplication.ToCertificateIdentifier(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate);
            RegisteredApplication.CertificateStorePath = certificateIdentifier.StorePath;
            RegisteredApplication.CertificateSubjectName = certificateIdentifier.SubjectName;
            if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates != null)
            {
                global::Opc.Ua.Security.CertificateStoreIdentifier certificateStoreIdentifier = SecuredApplication.ToCertificateStoreIdentifier(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates);
                RegisteredApplication.IssuerListStorePath = certificateStoreIdentifier.StorePath;
                if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.TrustedCertificates != null)
                {
                    CertificateList certificateList = SecuredApplication.ToCertificateList(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.TrustedCertificates);
                    //IssuerCertificates
                }
            }
            if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates != null)
            {
                global::Opc.Ua.Security.CertificateStoreIdentifier certificateStoreIdentifier = SecuredApplication.ToCertificateStoreIdentifier(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates);
                RegisteredApplication.TrustListStorePath = certificateStoreIdentifier.StorePath;
                if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.TrustedCertificates != null)
                {
                    CertificateList certificateList = SecuredApplication.ToCertificateList(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.TrustedCertificates);
                    //application.TrustedCertificates
                }
            }

            if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.RejectedCertificateStore != null)
            {
                //application.RejectedCertificatesStore 
                global::Opc.Ua.Security.CertificateStoreIdentifier certificateStoreIdentifier = SecuredApplication.ToCertificateStoreIdentifier(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.RejectedCertificateStore);
            }
            return true;
        }
        public bool GetAndReplaceWithGlobalDiscoveryTrustedList()
        {
            if (!GlobalDiscoveryServerClient.IsConnected)
                return false;
            //Application user access required
            NodeId trustListId = GlobalDiscoveryServerClient.GetTrustList(RegisteredApplication.ApplicationId, null);
            if (trustListId == null)
                return false;
            if (string.IsNullOrEmpty(RegisteredApplication.TrustListStorePath))
                return false;
            DeleteExistingCertificateFromStore(RegisteredApplication.TrustListStorePath, RegisteredApplication.CertificatePublicKeyPath, RegisteredApplication.CertificatePrivateKeyPath).Wait();
            DeleteExistingCertificateFromStore(RegisteredApplication.IssuerListStorePath, RegisteredApplication.CertificatePublicKeyPath, RegisteredApplication.CertificatePrivateKeyPath).Wait();
            Console.WriteLine("The trust list (include CRLs) was deleted locally.");
            return GetAndMergeWithGlobalDiscoveryTrustedList();
        }
        public bool GetAndMergeWithGlobalDiscoveryTrustedList()
        {
            if (!GlobalDiscoveryServerClient.IsConnected)
                return false;
            //Application user access required
            NodeId trustListId = GlobalDiscoveryServerClient.GetTrustList(RegisteredApplication.ApplicationId, null);
            if (trustListId == null)
                return false;
            TrustListDataType trustList = GlobalDiscoveryServerClient.ReadTrustList(trustListId);
            //Trust List Store Path
            if (!string.IsNullOrEmpty(RegisteredApplication.TrustListStorePath))
            {
                using (ICertificateStore trustListStore = global::Opc.Ua.CertificateStoreIdentifier.OpenStore(RegisteredApplication.TrustListStorePath))
                {
                    if ((trustList.SpecifiedLists & (uint)TrustListMasks.TrustedCertificates) != 0)
                    {
                        foreach (byte[] certificate in trustList.TrustedCertificates)
                        {
                            X509Certificate2 x509 = new X509Certificate2(certificate);
                            X509Certificate2Collection certs = trustListStore.FindByThumbprint(x509.Thumbprint).Result;
                            if (certs.Count == 0)
                                trustListStore.Add(x509).Wait();
                        }
                    }
                    if ((trustList.SpecifiedLists & (uint)TrustListMasks.TrustedCrls) != 0)
                    {
                        foreach (byte[] certificateRevocation in trustList.TrustedCrls)
                        {
                            trustListStore.AddCRL(new X509CRL(certificateRevocation));
                        }
                    }
                }
            }
            //Issuer List Store Path
            if (!string.IsNullOrEmpty(RegisteredApplication.IssuerListStorePath))
            {
                using (ICertificateStore store = global::Opc.Ua.CertificateStoreIdentifier.OpenStore(RegisteredApplication.IssuerListStorePath))
                {
                    if ((trustList.SpecifiedLists & (uint)TrustListMasks.IssuerCertificates) != 0)
                    {
                        foreach (byte[] issuerCertificate in trustList.IssuerCertificates)
                        {
                            X509Certificate2 x509Certificate = new X509Certificate2(issuerCertificate);
                            X509Certificate2Collection certs = store.FindByThumbprint(x509Certificate.Thumbprint).Result;
                            if (certs.Count == 0)
                                store.Add(x509Certificate).Wait();
                        }
                    }
                    if ((trustList.SpecifiedLists & (uint)TrustListMasks.IssuerCrls) != 0)
                    {
                        foreach (byte[] issuerCertificateRevocation in trustList.IssuerCrls)
                        {
                            store.AddCRL(new X509CRL(issuerCertificateRevocation));
                        }
                    }
                }
            }
            ReloadRegisteredApplicationCertificateList();
            Console.WriteLine("The trust list (include CRLs) was downloaded from the GDS and saved locally.");
            return true;
        }
        public void ReloadRegisteredApplicationCertificateList()
        {
            if (RegisteredApplication == null)
                return;
            if (RegisteredApplication.RegistrationType == RegistrationType.ServerPush)
            {
                //TrustListDataType trustList = _serverPushConfigurationClient.ReadTrustList();
                //X509Certificate2Collection rejectedList = _serverPushConfigurationClient.GetRejectedList();
            }
            else
            {
                if (!string.IsNullOrEmpty(RegisteredApplication.TrustListStorePath))
                    global::Opc.Ua.CertificateStoreIdentifier.CreateStore(global::Opc.Ua.CertificateStoreIdentifier.DetermineStoreType(RegisteredApplication.TrustListStorePath));
                if (!string.IsNullOrEmpty(RegisteredApplication.IssuerListStorePath))
                    global::Opc.Ua.CertificateStoreIdentifier.CreateStore(global::Opc.Ua.CertificateStoreIdentifier.DetermineStoreType(RegisteredApplication.IssuerListStorePath));
                ApplicationRecordDataType applicationRecordDataType = new ApplicationRecordDataType
                {
                    ApplicationType = ApplicationInstance.ApplicationConfiguration.ApplicationType,
                    ApplicationNames = new LocalizedText[] { ApplicationInstance.ApplicationConfiguration.ApplicationName },
                    ApplicationUri = ApplicationInstance.ApplicationConfiguration.ApplicationUri,
                    ProductUri = ApplicationInstance.ApplicationConfiguration.ProductUri,
                    ApplicationId = RegisteredApplication.ApplicationId,
                    DiscoveryUrls = RegisteredApplication.DiscoveryUrl,
                    ServerCapabilities = RegisteredApplication.ServerCapability
                };
                GlobalDiscoveryServerClient.UpdateApplication(applicationRecordDataType);
            }
        }
        #endregion

        #region Event Handler
        private void ReconnectComplete(object sender, EventArgs e)
        {
            try
            {
                // ignore callbacks from discarded objects.
                if (!ReferenceEquals(sender, _reconnectHandler))
                    return;
                Session = _reconnectHandler.Session;
                _reconnectHandler.Dispose();
                _reconnectHandler = null;
                // raise any additional notifications.
                _reconnectComplete?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reconnect exception: {ex.StackTrace}");
            }
        }
        private void CertificateValidatorCertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            Console.WriteLine($"Untrusted Certificate: {e.Certificate}");
            try
            {
                if (ApplicationInstance == null)
                    return;
                if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration == null ||
                    !ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.AutoAcceptUntrustedCertificates ||
                    e.Error == null || 
                    e.Error.Code != StatusCodes.BadCertificateUntrusted)
                {
                    Console.WriteLine($"Certificate not accepted: { e.Certificate.Subject} \r\n { e.Error.Code}");
                    return;
                }
                e.Accept = true;
                //Experiment
                //if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.AddAppCertToTrustedStore)
                //{
                //     AddToTrustedStore(ApplicationInstance.ApplicationConfiguration,e.Certificate).Wait();
                //}
                Console.WriteLine($"Automatically accepted certificate: {e.Certificate.Subject}");
            }
            catch
            {
                Console.WriteLine("Error accepting certificate.");
            }
        }
        private static void MonitoredItemStatusNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification notification = (MonitoredItemNotification)e.NotificationValue;
            ServerStatusDataType serverStatusDataType = notification.Value.GetValue<ServerStatusDataType>(null);
            Console.WriteLine($"GDS ServerStatusDataType: {serverStatusDataType}");
        }
        private void SessionKeepAlive(Session session, KeepAliveEventArgs e)
        {
            try
            {
                // check for events from discarded sessions.
                if (!ReferenceEquals(session, Session))
                    return;
                // start reconnect sequence on communication error.
                if (ServiceResult.IsBad(e.Status))
                {
                    if (_reconnectPeriod <= 0)
                    {
                        Console.WriteLine($"{e.CurrentTime} Session communication error: {e.Status}");
                        return;
                    }
                    Console.WriteLine($"{e.CurrentTime} Reconnecting in: {_reconnectPeriod}");
                    if (_reconnectHandler == null)
                    {
                        _reconnectStarting?.Invoke(this, e);
                        _reconnectHandler = new SessionReconnectHandler();
                        _reconnectHandler.BeginReconnect(Session, _reconnectPeriod * 1000, ReconnectComplete);
                    }
                    return;
                }
                // update status.
                Console.WriteLine($"{e.CurrentTime} Connected: {session.Endpoint.EndpointUrl}");
                // raise any additional notifications.
                _KeepAliveComplete?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Keep Alive exception: {ex.StackTrace}");
            }
            Console.WriteLine($"Session endpoint:{session.ConfiguredEndpoint} - Session Status: {e.Status}");
        }
        #endregion
    }
}

