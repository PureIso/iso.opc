﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Iso.Opc.Core.Models;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Client.ComplexTypes;
using Opc.Ua.Configuration;
using Opc.Ua.Gds;
using Opc.Ua.Gds.Client;
using Opc.Ua.Security;
using ApplicationType = Opc.Ua.ApplicationType;
using CertificateIdentifier = Opc.Ua.CertificateIdentifier;
using CertificateStoreIdentifier = Opc.Ua.CertificateStoreIdentifier;
using ObjectIds = Opc.Ua.ObjectIds;
using Objects = Opc.Ua.Objects;
using ObjectTypeIds = Opc.Ua.ObjectTypeIds;

namespace Iso.Opc.Core
{
    public class ApplicationInstanceManager
    {
        #region Constants
        private const int ReconnectPeriod = 10;
        #endregion

        #region Fields
        private X509Certificate2 _certificate;
        #endregion

        #region Delegates and Handlers
        public EventHandler ReconnectStartingHandler;
        public EventHandler ReconnectCompleteHandler;
        public EventHandler KeepAliveCompleteHandler;
        public SessionReconnectHandler SessionReconnectHandler;
        #endregion

        #region Properties
        public ApplicationInstance ApplicationInstance { get; set; }
        public GlobalDiscoveryServerClient GlobalDiscoveryServerClient { get; set; }
        public RegisteredApplication RegisteredApplication { get; set; }
        public Session Session { get; set; }
        public EndpointDescription SessionEndpointDescription { get; set; }
        public Dictionary<string, ExtendedDataDescription> FlatExtendedDataDescriptionDictionary { get; set; }
        public bool AutomaticallyAddAppCertToTrustStore { get; set; }
        public Subscription Subscription { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="applicationUri"></param>
        /// <param name="baseAddress"></param>
        /// <param name="serverCapabilities"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="endpointApplicationUri"></param>
        /// <param name="discoveryUrls"></param>
        /// <param name="wellKnownDiscoveryUrls"></param>
        /// <param name="applicationType"></param>
        /// <param name="addAppCertToTrustedStore"></param>
        public ApplicationInstanceManager(string applicationName, string applicationUri, 
            StringCollection baseAddress, StringCollection serverCapabilities, string endpointUrl, 
            string endpointApplicationUri, StringCollection discoveryUrls, StringCollection wellKnownDiscoveryUrls,
            ApplicationType applicationType, bool addAppCertToTrustedStore = false)
        {
            AutomaticallyAddAppCertToTrustStore = addAppCertToTrustedStore;
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
            Utils.Trace("Checking application configuration.");
            ApplicationConfiguration applicationConfiguration = GetApplicationConfiguration(applicationName, applicationUri, applicationType);
            switch (applicationConfiguration.ApplicationType)
            {
                //Check server configuration
                case ApplicationType.Server:
                {
                        Utils.Trace("Checking application transport quota configuration.");
                        applicationConfiguration.TransportQuotas = GetTransportQuotas();
                        Utils.Trace("Checking application server configuration.");
                        applicationConfiguration.ServerConfiguration = GetServerConfiguration(baseAddress, serverCapabilities, null, applicationType);
                        //required in order to connect to the discovery server
                        //GDS session will have to validate the client configuration as part of the application configuration
                        Utils.Trace("Checking client configuration.");
                        applicationConfiguration.ClientConfiguration = GetClientConfiguration(new StringCollection(), new EndpointDescriptionCollection());
                        break;
                }
                case ApplicationType.DiscoveryServer:
                {
                        Utils.Trace("Checking application transport quota configuration.");
                        applicationConfiguration.TransportQuotas = GetTransportQuotas();
                        endpointUrl = Utils.ReplaceLocalhost(endpointUrl);
                        endpointApplicationUri = Utils.ReplaceLocalhost(endpointApplicationUri);
                        Utils.Trace("Checking discovery server configuration.");
                        EndpointDescription discoveryServerRegistrationEndpointDescription =
                            GetDiscoveryServerRegistrationEndpointDescription(endpointUrl, endpointApplicationUri, discoveryUrls);
                        applicationConfiguration.DiscoveryServerConfiguration = GetDiscoveryServerConfiguration(discoveryUrls);
                        Utils.Trace("Checking application server configuration.");
                        applicationConfiguration.ServerConfiguration = GetServerConfiguration(baseAddress, serverCapabilities,
                            discoveryServerRegistrationEndpointDescription, applicationType);
                        break;
                }
                case ApplicationType.Client:
                {
                        Utils.Trace("Checking client configuration.");
                        applicationConfiguration.ClientConfiguration = GetClientConfiguration(wellKnownDiscoveryUrls, new EndpointDescriptionCollection());
                        break;
                }
            }
            applicationConfiguration = CheckApplicationInstanceCertificate(applicationConfiguration);
            if (applicationConfiguration == null)
                return;
            Utils.Trace("Checking global discovery client configuration.");
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
            ApplicationConfiguration configuration = new ApplicationConfiguration
            {
                ApplicationName = applicationName,
                ApplicationType = applicationType,
                ProductUri = Utils.Format($"urn:{applicationName}:", Dns.GetHostName()),
                ApplicationUri = applicationUri,
                DisableHiResClock = true
            };
            //Setup Trace
            Utils.Trace("Checking application transport trace configuration.");
            configuration.TraceConfiguration = GetTraceConfiguration(applicationType.ToString().ToLower());
            //Setup security configuration
            Utils.Trace("Getting application security configuration.");
            SecurityConfiguration securityConfiguration = GetSecurityConfiguration(applicationName);
            CertificateValidator certificateValidator = new CertificateValidator();
            certificateValidator.CertificateValidation += CertificateValidatorCertificateValidation;
            certificateValidator.Update(securityConfiguration);
            configuration.CertificateValidator = certificateValidator;
            configuration.SecurityConfiguration = securityConfiguration;
            //Check the certificate.
            Utils.Trace("Checking application certificate.");
            X509Certificate2 certificate = securityConfiguration.ApplicationCertificate.Find(true).Result;
            if (certificate == null)
            {
                // create a new certificate.
                Utils.Trace("Creating a new certificate.");
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
            return configuration;
        }
        private SecurityConfiguration GetSecurityConfiguration(string applicationName)
        {
            SecurityConfiguration securityConfiguration = new SecurityConfiguration();
            //ApplicationCertificate
            string directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(directoryName))
                return null; 
            string applicationCertificateDirectory = Path.Combine(directoryName,"pki\\trusted");//own
            if (!Directory.Exists(applicationCertificateDirectory))
                Directory.CreateDirectory(applicationCertificateDirectory);

            securityConfiguration.ApplicationCertificate = new CertificateIdentifier
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = applicationCertificateDirectory,
                SubjectName = "CN="+ applicationName+", DC="+Dns.GetHostName(),
            };
            //TrustedIssuerCertificates
            string issuersCertificateDirectory = Path.Combine(directoryName, "pki\\trusted");//issuers
            if (!Directory.Exists(issuersCertificateDirectory))
                Directory.CreateDirectory(issuersCertificateDirectory);
            securityConfiguration.TrustedIssuerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = issuersCertificateDirectory
            };
            //TrustedPeerCertificates
            string trustedCertificateDirectory = Path.Combine(directoryName, "pki\\trusted");
            if (!Directory.Exists(trustedCertificateDirectory))
                Directory.CreateDirectory(trustedCertificateDirectory);
            securityConfiguration.TrustedPeerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = trustedCertificateDirectory
            };
            //RejectedCertificateStore
            string rejectedCertificateDirectory = Path.Combine(directoryName, "pki\\trusted");
            if (!Directory.Exists(rejectedCertificateDirectory))
                Directory.CreateDirectory(rejectedCertificateDirectory);
            securityConfiguration.RejectedCertificateStore = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = rejectedCertificateDirectory
            };
            //UserIssuerCertificates
            string userIssuerCertificateDirectory = Path.Combine(directoryName, "pki\\trusted");//issuedUser
            if (!Directory.Exists(userIssuerCertificateDirectory))
                Directory.CreateDirectory(userIssuerCertificateDirectory);
            securityConfiguration.UserIssuerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = userIssuerCertificateDirectory
            };
            //TrustedUserCertificates
            string trustedUserCertificateDirectory = Path.Combine(directoryName, "pki\\trusted");//trustedUser
            if (!Directory.Exists(trustedUserCertificateDirectory))
                Directory.CreateDirectory(trustedUserCertificateDirectory);
            securityConfiguration.TrustedUserCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = trustedUserCertificateDirectory
            };
            securityConfiguration.AddAppCertToTrustedStore = AutomaticallyAddAppCertToTrustStore;
            securityConfiguration.AutoAcceptUntrustedCertificates = true;
            securityConfiguration.RejectSHA1SignedCertificates = false;
            securityConfiguration.NonceLength = 32;
            //false for server
            securityConfiguration.RejectUnknownRevocationStatus = false;
            //For CA signed certificates, this flag controls whether the server shall send the complete certificate chain instead of just sending the certificate. 
            //This affects the GetEndpoints and CreateSession service.**/
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
            UserTokenPolicyCollection userTokenPolicyCollection = new UserTokenPolicyCollection
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
            ServerSecurityPolicyCollection serverSecurityPolicyCollection = new ServerSecurityPolicyCollection
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
            Utils.Trace("Checking discovery server configuration.");
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
        private static TraceConfiguration GetTraceConfiguration(string logName)
        {
            string directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(directoryName))
                return null;
            string traceLogsDirectory = Path.Combine(directoryName, "logs");
            if (!Directory.Exists(traceLogsDirectory))
                Directory.CreateDirectory(traceLogsDirectory);
            string traceLogFile = Path.Combine(traceLogsDirectory, $"{logName}.log.txt");
            if (!File.Exists(traceLogFile))
                File.Create(traceLogFile).Close();
            TraceConfiguration traceConfiguration = new TraceConfiguration
            {
                OutputFilePath = traceLogFile, DeleteOnLoad = false, TraceMasks = 515
                
            };
            traceConfiguration.ApplySettings();
            return traceConfiguration;
        }
        private static string SelectServerUrl(IList<string> discoveryUrls)
        {
            if (discoveryUrls == null || discoveryUrls.Count == 0)
                return null;
            string url = discoveryUrls.FirstOrDefault(discoveryUrl => discoveryUrl.StartsWith("opc.tcp://", StringComparison.Ordinal));
            // always use opc.tcp by default.
            // try HTTPS if no opc.tcp.
            if (url != null) 
                return url;
            url = discoveryUrls.FirstOrDefault(discoveryUrl => discoveryUrl.StartsWith("https://", StringComparison.Ordinal));
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
        private static bool ContainsPath(SimpleAttributeOperandCollection selectClause, QualifiedNameCollection browsePath)
        {
            for (int ii = 0; ii < selectClause.Count; ii++)
            {
                SimpleAttributeOperand field = selectClause[ii];

                if (field.BrowsePath.Count != browsePath.Count)
                {
                    continue;
                }

                bool match = true;

                for (int jj = 0; jj < field.BrowsePath.Count; jj++)
                {
                    if (field.BrowsePath[jj] != browsePath[jj])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return true;
                }
            }

            return false;
        }
        private void CollectFields(NodeId eventTypeId, SimpleAttributeOperandCollection eventFields)
        {
            // get the supertypes.
            ReferenceDescriptionCollection supertypes = new ReferenceDescriptionCollection();
            // find all of the children of the field.
            BrowseDescription nodeToBrowse = new BrowseDescription
            {
                NodeId = eventTypeId,
                BrowseDirection = BrowseDirection.Inverse,
                ReferenceTypeId = ReferenceTypeIds.HasSubtype,
                IncludeSubtypes = false,
                NodeClassMask = 0,
                ResultMask = (uint) BrowseResultMask.All
            };
            // more efficient to use IncludeSubtypes=False when possible.
            // the HasSubtype reference already restricts the targets to Types. 
            ReferenceDescriptionCollection references = Browse(nodeToBrowse);
            while (references != null && references.Count > 0)
            {
                // should never be more than one super type.
                supertypes.Add(references[0]);
                // only follow references within this server.
                if (references[0].NodeId.IsAbsolute)
                    break;
                // get the references for the next level up.
                nodeToBrowse.NodeId = (NodeId)references[0].NodeId;
                references = Browse(nodeToBrowse);
            }
            // process the types starting from the top of the tree.
            Dictionary<NodeId, QualifiedNameCollection> foundNodes = new Dictionary<NodeId, QualifiedNameCollection>();
            QualifiedNameCollection parentPath = new QualifiedNameCollection();
            for (int ii = supertypes.Count - 1; ii >= 0; ii--)
            {
                CollectFields((NodeId)supertypes[ii].NodeId, parentPath, eventFields, foundNodes);
            }
            // collect the fields for the selected type.
            CollectFields(eventTypeId, parentPath, eventFields, foundNodes);
        }
        private void CollectFields(
            NodeId nodeId,
            QualifiedNameCollection parentPath,
            SimpleAttributeOperandCollection eventFields,
            IDictionary<NodeId, QualifiedNameCollection> foundNodes)
        {
            // find all of the children of the field.
            BrowseDescription nodeToBrowse = new BrowseDescription
            {
                NodeId = nodeId,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.Aggregates,
                IncludeSubtypes = true,
                NodeClassMask = (uint) (NodeClass.Object | NodeClass.Variable),
                ResultMask = (uint) BrowseResultMask.All
            };
            ReferenceDescriptionCollection children = Browse(nodeToBrowse);
            if (children == null)
                return; 
            // process the children.
            foreach (ReferenceDescription child in children)
            {
                if (child.NodeId.IsAbsolute)
                    continue;
                // construct browse path.
                QualifiedNameCollection browsePath = new QualifiedNameCollection(parentPath) {child.BrowseName};
                // check if the browse path is already in the list.
                if (!ContainsPath(eventFields, browsePath))
                {
                    SimpleAttributeOperand field = new SimpleAttributeOperand
                    {
                        TypeDefinitionId = ObjectTypeIds.BaseEventType,
                        BrowsePath = browsePath,
                        AttributeId = child.NodeClass == NodeClass.Variable ? Attributes.Value : Attributes.NodeId
                    };
                    eventFields.Add(field);
                }
                // recursively find all of the children.
                NodeId targetId = (NodeId)child.NodeId;
                // need to guard against loops.
                if (foundNodes.ContainsKey(targetId)) 
                    continue;
                foundNodes.Add(targetId, browsePath);
                CollectFields((NodeId)child.NodeId, browsePath, eventFields, foundNodes);
            }
        }
        private static void MonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                if (!(e.NotificationValue is MonitoredItemNotification monitoredItemNotification))
                    return;
                Utils.Trace($"Monitored value: {monitoredItemNotification.Value.WrappedValue.ToString()}");
            }
            catch (Exception ex)
            {
                Utils.Trace($"Monitored Item Notification exception: {ex.StackTrace}");
            }
        }
        private void AddToFlatExtendedDataDescriptionDictionary(ExtendedDataDescription extendedDataDescription)
        {
            if (!FlatExtendedDataDescriptionDictionary.ContainsKey(extendedDataDescription.DataDescription.ReferenceDescription.NodeId.Identifier.ToString()))
            {
                FlatExtendedDataDescriptionDictionary[extendedDataDescription.DataDescription.ReferenceDescription.NodeId.Identifier.ToString()] = extendedDataDescription;
            }
        }
        #region Certificate Helper
        private void CertificateRequestChecker()
        {
            try
            {
                NodeId requestId = NodeId.Parse(RegisteredApplication.CertificateRequestId);
                byte[] certificate = GlobalDiscoveryServerClient.FinishRequest(
                    RegisteredApplication.ApplicationId,
                    requestId,
                    out byte[] privateKeyPFX,
                    out byte[][] issuerCertificates);

                if (certificate == null)
                {
                    Task.Delay(1000);
                    CertificateRequestChecker();
                    return;
                }
                X509Certificate2 newCert = new X509Certificate2(certificate);
                if (!string.IsNullOrEmpty(RegisteredApplication.CertificateStorePath) && !string.IsNullOrEmpty(RegisteredApplication.CertificateSubjectName))
                {
                    CertificateIdentifier cid = new CertificateIdentifier()
                    {
                        StorePath = RegisteredApplication.CertificateStorePath,
                        StoreType = CertificateStoreIdentifier.DetermineStoreType(RegisteredApplication.CertificateStorePath),
                        SubjectName = RegisteredApplication.CertificateSubjectName.Replace("localhost", Utils.GetHostName())
                    };
                    // update store
                    using (ICertificateStore store = CertificateStoreIdentifier.OpenStore(RegisteredApplication.CertificateStorePath))
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
                    byte[] exportedCert = string.Compare(file.Extension, ".PEM", StringComparison.OrdinalIgnoreCase) == 0 ?
                            CertificateFactory.ExportCertificateAsPEM(newCert) :
                            newCert.Export(X509ContentType.Cert);
                    File.WriteAllBytes(absoluteCertificatePublicKeyPath, exportedCert);
                }

                // update trust list.
                if (!string.IsNullOrEmpty(RegisteredApplication.TrustListStorePath))
                {
                    using (ICertificateStore store = CertificateStoreIdentifier.OpenStore(RegisteredApplication.TrustListStorePath))
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
                Utils.Trace($"CertificateRequestChecker exception {exception.StackTrace}");
            }
        }
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
            using DirectoryCertificateStore store = (DirectoryCertificateStore)CertificateStoreIdentifier.OpenStore(certificateStorePath);
            X509Certificate2Collection certificates = await store.Enumerate();
            foreach (X509Certificate2 certificate in certificates)
            {
                if (store.GetPrivateKeyFilePath(certificate.Thumbprint) != null)
                    continue;
                List<string> fields = Utils.ParseDistinguishedName(certificate.Subject);
                if (fields.Contains("CN=UA Local Discovery Server"))
                    continue;
                string path = Utils.GetAbsoluteFilePath(certificatePublicKeyPath, true, false, false);
                if (path != null)
                {
                    if (string.Compare(path, store.GetPublicKeyFilePath(certificate.Thumbprint), StringComparison.OrdinalIgnoreCase) == 0)
                        continue;
                }
                path = Utils.GetAbsoluteFilePath(certificatePrivateKeyPath, true, false, false);
                if (path != null)
                {
                    if (string.Compare(path, store.GetPrivateKeyFilePath(certificate.Thumbprint), StringComparison.OrdinalIgnoreCase) == 0)
                        continue;
                }
                await store.Delete(certificate.Thumbprint);
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
                foreach (X509Certificate2 x509Certificate2 in certificates)
                {
                    if (!Utils.CompareDistinguishedName(x509Certificate2, subjectName))
                        continue;
                    if (x509Certificate2.Thumbprint == certificate.Thumbprint)
                        return;
                    await store.Delete(x509Certificate2.Thumbprint);
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
            Utils.Trace("Checking application instance certificate.");
            if (applicationConfiguration == null)
                return null;
            //Find the existing certificate.
            CertificateIdentifier applicationCertificateIdentifier = applicationConfiguration.SecurityConfiguration.ApplicationCertificate;
            if (applicationCertificateIdentifier == null)
            {
                Utils.Trace("Configuration file does not specify a certificate.");
                return null;
            }
            X509Certificate2 applicationX509Certificate = applicationCertificateIdentifier.Find(true).Result;
            //Check that it is ok.
            if (applicationX509Certificate != null)
            {
                Utils.Trace($"Checking application instance certificate. {applicationX509Certificate.Subject}");
                try
                {
                    //Validate certificate.
                    applicationConfiguration.CertificateValidator.Validate(applicationX509Certificate);
                }
                catch (Exception ex)
                {
                    Utils.Trace($"Error validating certificate. Exception: { ex.Message}. Use certificate anyway?");
                    return null;
                }
                //Check key size.
                if (minimumKeySize > applicationX509Certificate.GetRSAPublicKey().KeySize)
                {
                    Utils.Trace($"The key size ({applicationX509Certificate.GetRSAPublicKey().KeySize}) in the certificate is less than the minimum provided ({minimumKeySize}). Use certificate anyway?");
                    return null;
                }
                //Check domains.
                if (applicationConfiguration.ApplicationType != ApplicationType.Client)
                {
                    Utils.Trace($"Checking domains in certificate. {applicationX509Certificate.Subject}");
                    List<string> serverDomainNames = applicationConfiguration.GetServerDomainNames().Distinct().ToList();
                    List<string> certificateDomainNames = Utils.GetDomainsFromCertficate(applicationX509Certificate).Distinct().ToList();
                    string computerName = Utils.GetHostName();
                    //Get IP addresses.
                    IPAddress[] addresses = null;
                    foreach (string serverDomainName in serverDomainNames)
                    {
                        if (Utils.FindStringIgnoreCase(certificateDomainNames, serverDomainName))
                            continue;
                        if (string.Compare(serverDomainName, "localhost", StringComparison.OrdinalIgnoreCase) == 0)
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
                        Utils.Trace($"The server is configured to use domain '{serverDomainName}' which does not appear in the certificate. Use certificate?");
                        break;
                    }
                }
                //Check uri.
                string applicationUri = Utils.GetApplicationUriFromCertificate(applicationX509Certificate);
                if (string.IsNullOrEmpty(applicationUri))
                {
                    Utils.Trace("The Application URI could not be read from the certificate.");
                }
                else
                {
                    Utils.Trace("The Application URI found in certificate.");
                    applicationConfiguration.ApplicationUri = applicationUri;
                }
            }
            else
            {
                // check for missing private key.
                applicationX509Certificate = applicationCertificateIdentifier.Find(false).Result;
                if (applicationX509Certificate != null)
                {
                    Utils.Trace($"Cannot access certificate private key. Subject={applicationX509Certificate.Subject}");
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
                        Utils.Trace("Thumbprint was explicitly specified in the configuration." +
                                          "\r\nAnother certificate with the same subject name was found." +
                                          "\r\nUse it instead?\r\n" +
                                          $"\r\nRequested: {applicationCertificateIdentifier.SubjectName}" +
                                          $"\r\nFound: {applicationX509Certificate.Subject}");
                        return null;
                    }
                    Utils.Trace("Thumbprint was explicitly specified in the configuration. Cannot generate a new certificate.");
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
            Utils.Trace("There is no cert with subject in the configuration." + "\r\n Please generate a cert for your application");
            return null;
        }
        #endregion
        #endregion

        #region Public Methods
        public List<ServerOnNetwork> QueryServers()
        {
            if (!GlobalDiscoveryServerClient.IsConnected)
                return null;
            uint maximumRecordsToReturn = 0;
            string applicationName = "";
            string applicationUri = "";
            string productUri = "";
            List<string> serverCapabilities = new List<string>();
            List<ServerOnNetwork> servers = GlobalDiscoveryServerClient.QueryServers(
                maximumRecordsToReturn, 
                applicationName, applicationUri, productUri, serverCapabilities).ToList();
            return servers;
        }
        public bool ConnectToServer(string serverDiscoveryEndpoint, string username, string password)
        {
            try
            {
                Session?.Close();
                IUserIdentity userIdentity = null; //UserTokenType.Anonymous as default
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
                if (!Session.Connected) 
                    return false;
                Session.KeepAlive += SessionKeepAlive;
                FlatExtendedDataDescriptionDictionary = new Dictionary<string, ExtendedDataDescription>();
                ComplexTypeSystem typeSystemLoader = new ComplexTypeSystem(Session);
                typeSystemLoader.Load();
                return true;
            }
            catch (Exception ex)
            {
                Utils.Trace($"Connecting to GDS exception: {ex.StackTrace}");
                return false;
            }
        }
        public List<ReferenceDescription> BrowseReferenceDescription(ReferenceDescription parentReferenceDescription = null)
        {
            BrowseDescriptionCollection browseDescriptionCollection = new BrowseDescriptionCollection();
            BrowseDescription browseDescription;
            //Define the node to browse
            if (parentReferenceDescription == null)
            {
                foreach (string sessionNamespaceUri in Session.NamespaceUris.ToArray())
                {
                    ushort namespaceIndex = Session.SystemContext.NamespaceUris.GetIndexOrAppend(sessionNamespaceUri);
                    browseDescription = new BrowseDescription
                    {
                        BrowseDirection = BrowseDirection.Forward,
                        ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                        IncludeSubtypes = true,
                        NodeClassMask = (uint)NodeClass.Unspecified,
                        ResultMask = (uint)BrowseResultMask.All
                    };
                    NodeId nodeToBrowse = new NodeId(Objects.RootFolder, namespaceIndex);
                    browseDescription.NodeId = nodeToBrowse;
                    browseDescriptionCollection.Add(browseDescription);
                }
            }
            else
            {
                foreach (string sessionNamespaceUri in Session.NamespaceUris.ToArray())
                {
                    ushort namespaceIndex = Session.SystemContext.NamespaceUris.GetIndexOrAppend(sessionNamespaceUri);
                    browseDescription = new BrowseDescription
                    {
                        BrowseDirection = BrowseDirection.Forward,
                        ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                        IncludeSubtypes = true,
                        NodeClassMask = (uint)NodeClass.Unspecified,
                        ResultMask = (uint)BrowseResultMask.All
                    };
                    NodeId nodeToBrowse = new NodeId(parentReferenceDescription.NodeId.Identifier, namespaceIndex);
                    browseDescription.NodeId = nodeToBrowse;
                    browseDescriptionCollection.Add(browseDescription);
                }
            }
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
            return browseResultCollection
                .Where(x => x.References != null)
                .Select(y => y.References.ToList()).SelectMany(i => i).ToList();
        }
        public List<DataDescription> GetVariablesDataDescriptions(DataDescription parentDataDescription)
        {
            List<ReferenceDescription> referenceDescriptions = BrowseReferenceDescription(parentDataDescription.ReferenceDescription);
            if (!referenceDescriptions.Any())
                return null;
            List<DataDescription> dataDescriptions = new List<DataDescription>();
            //We need to iterate through the list
            foreach (ReferenceDescription referenceDescription in referenceDescriptions)
            {
                if (referenceDescription.NodeClass != NodeClass.Variable)
                    continue;
                DataDescription dataDescription = new DataDescription
                {
                    AttributeData = ReadAttributes(referenceDescription),
                    ReferenceDescription = referenceDescription
                };
                ExtendedDataDescription variableDataDescription = new ExtendedDataDescription
                {
                    DataDescription = dataDescription
                };
                dataDescriptions.Add(dataDescription);
                AddToFlatExtendedDataDescriptionDictionary(variableDataDescription);
            }
            return dataDescriptions;
        }
        public List<ExtendedDataDescription> GetMethodsExtendedDescriptions(DataDescription parentDataDescription)
        {
            List<ExtendedDataDescription> extendedDataDescriptions = new List<ExtendedDataDescription>();
            List<ReferenceDescription> referenceDescriptions = BrowseReferenceDescription(parentDataDescription.ReferenceDescription);
            if (!referenceDescriptions.Any())
                return null;
            //We need to iterate through the list
            foreach (ReferenceDescription referenceDescription in referenceDescriptions)
            {
                if (referenceDescription.NodeClass != NodeClass.Method) 
                    continue;
                DataDescription dataDescription = new DataDescription
                {
                    AttributeData = ReadAttributes(referenceDescription),
                    ReferenceDescription = referenceDescription
                };
                ExtendedDataDescription methodDataDescription = new ExtendedDataDescription
                {
                    DataDescription = dataDescription,
                    VariableDataDescriptions = GetVariablesDataDescriptions(dataDescription)
                };
                extendedDataDescriptions.Add(methodDataDescription);
                AddToFlatExtendedDataDescriptionDictionary(methodDataDescription);
            }
            return extendedDataDescriptions;
        }
        public List<ExtendedDataDescription> GetObjectExtendedDataDescription(DataDescription parentDataDescription)
        {
            List<ReferenceDescription> referenceDescriptions = BrowseReferenceDescription(parentDataDescription.ReferenceDescription);
            if (!referenceDescriptions.Any())
                return null;
            List<ExtendedDataDescription> extendedDataDescriptions = new List<ExtendedDataDescription>();
            //We need to iterate through the list
            foreach (ReferenceDescription referenceDescription in referenceDescriptions)
            {
                if (referenceDescription.NodeClass != NodeClass.Object)
                    continue;
                DataDescription dataDescription = new DataDescription
                {
                    AttributeData = ReadAttributes(referenceDescription),
                    ReferenceDescription = referenceDescription
                };
                ExtendedDataDescription extendedDataDescription = new ExtendedDataDescription
                {
                    DataDescription = dataDescription,
                    VariableDataDescriptions = GetVariablesDataDescriptions(dataDescription),
                    MethodDataDescriptions = GetMethodsExtendedDescriptions(dataDescription),
                    ObjectDataDescriptions = GetObjectExtendedDataDescription(dataDescription)
                };
                if(extendedDataDescription.VariableDataDescriptions != null &&
                   extendedDataDescription.MethodDataDescriptions != null &&
                   extendedDataDescription.ObjectDataDescriptions != null)
                {
                    if (extendedDataDescription.VariableDataDescriptions.Any() ||
                        extendedDataDescription.MethodDataDescriptions.Any() ||
                        extendedDataDescription.ObjectDataDescriptions.Any())
                    {
                        extendedDataDescriptions.Add(extendedDataDescription);
                        AddToFlatExtendedDataDescriptionDictionary(extendedDataDescription);
                    }
                }
            }
            return extendedDataDescriptions;
        }
        public List<ExtendedDataDescription> GetRootExtendedDataDescriptions()
        {
            //Initialise the extended data descriptions
            List<ExtendedDataDescription> extendedDataDescriptions = new List<ExtendedDataDescription>();
            //Flatten the reference descriptions
            //This should be the root
            List<ReferenceDescription> referenceDescriptions = BrowseReferenceDescription();
            if (!referenceDescriptions.Any())
                return null;
            //We need to iterate through the list
            foreach (ReferenceDescription referenceDescription in referenceDescriptions)
            {
                DataDescription dataDescription = new DataDescription
                {
                    AttributeData = ReadAttributes(referenceDescription), //retrieve attributes
                    ReferenceDescription = referenceDescription
                };
                ExtendedDataDescription extendedDataDescription = new ExtendedDataDescription
                {
                    DataDescription = dataDescription,
                    VariableDataDescriptions = GetVariablesDataDescriptions(dataDescription),
                    MethodDataDescriptions = GetMethodsExtendedDescriptions(dataDescription),
                    ObjectDataDescriptions = GetObjectExtendedDataDescription(dataDescription)
                };
                if (extendedDataDescription.VariableDataDescriptions != null &&
                   extendedDataDescription.MethodDataDescriptions != null &&
                   extendedDataDescription.ObjectDataDescriptions != null)
                {
                    if (extendedDataDescription.VariableDataDescriptions.Any() ||
                        extendedDataDescription.MethodDataDescriptions.Any() ||
                        extendedDataDescription.ObjectDataDescriptions.Any())
                    {
                        extendedDataDescriptions.Add(extendedDataDescription);
                        AddToFlatExtendedDataDescriptionDictionary(extendedDataDescription);
                    }
                }
            }
            return extendedDataDescriptions;
        }
        public AttributeData ReadAttributes(ReferenceDescription referenceDescription)
        {
            NodeId nodeId;
            if (referenceDescription != null)
                nodeId = (NodeId)referenceDescription.NodeId;
            else
                return null;
            //build list of attributes to read.
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
            //This should generate the 27 attributes
            foreach (uint attributeId in Attributes.GetIdentifiers())
            {
                ReadValueId nodeToRead = new ReadValueId
                {
                    NodeId = nodeId,
                    AttributeId = attributeId
                };
                nodesToRead.Add(nodeToRead);
            }
            // read the attributes.
            Session.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                out DataValueCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            AttributeData attributeData = new AttributeData();
            attributeData.Initialise(results);
            return attributeData;
        }
        public ReferenceDescriptionCollection Browse(BrowseDescription nodeToBrowse)
        {
            ReferenceDescriptionCollection references = new ReferenceDescriptionCollection();
            try
            {
                // construct browse request.
                BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection {nodeToBrowse};
                // start the browse operation.
                Session.Browse(
                    null,
                    null,
                    0,
                    nodesToBrowse,
                    out BrowseResultCollection results,
                    out DiagnosticInfoCollection diagnosticInfos);
                ClientBase.ValidateResponse(results, nodesToBrowse);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);
                do
                {
                    // check for error.
                    if (StatusCode.IsBad(results[0].StatusCode))
                    {
                        throw new ServiceResultException(results[0].StatusCode);
                    }
                    // process results.
                    foreach (ReferenceDescription referenceDescription in results[0].References)
                    {
                        references.Add(referenceDescription);
                    }
                    // check if all references have been fetched.
                    if (results[0].References.Count == 0 || results[0].ContinuationPoint == null)
                    {
                        break;
                    }
                    // continue browse operation.
                    ByteStringCollection continuationPoints = new ByteStringCollection
                    {
                        results[0].ContinuationPoint
                    };
                    Session.BrowseNext(
                        null,
                        false,
                        continuationPoints,
                        out results,
                        out diagnosticInfos);
                    ClientBase.ValidateResponse(results, continuationPoints);
                    ClientBase.ValidateDiagnosticInfos(diagnosticInfos, continuationPoints);
                }
                while (true);
                //return complete list.
                
            }
            catch (Exception ex)
            {
                Utils.Trace($"Exception: {ex.StackTrace} Status Code: {StatusCodes.BadUnexpectedError}");
            }
            return references;
        }
        public bool SubscribeToNode(NodeId nodeId, MonitoredItemNotificationEventHandler callback=null, int publishingInterval = 1000)
        {
            try
            { 
                if (Session == null) 
                    return false;
                if (Subscription == null)
                {
                    Subscription = new Subscription
                    {
                        PublishingEnabled = true,
                        PublishingInterval = publishingInterval,
                        Priority = 1,
                        KeepAliveCount = 10,
                        LifetimeCount = 20,
                        MaxNotificationsPerPublish = 1000,
                        TimestampsToReturn = TimestampsToReturn.Both
                    };
                    Session.AddSubscription(Subscription);
                    Subscription.Create();
                }
                if (callback == null)
                    callback = MonitoredItemNotification;
                MonitoredItem monitoredItem = new MonitoredItem
                {
                    StartNodeId = nodeId, 
                    AttributeId = Attributes.Value
                };
                monitoredItem.Notification += callback;
                Subscription.AddItem(monitoredItem);
                Subscription.ApplyChanges();
                return true;
            }
            catch (Exception e)
            {
                Utils.Trace($"Monitored Item Notification exception: {e.StackTrace}");
                return false;
            }
        }
        public bool SubscribeToAuditUpdateMethodEvent(MonitoredItemNotificationEventHandler callback = null, int publishingInterval = 1000, EventFilter filter = null)
        {
            try
            {
                if (Session == null)
                    return false;
                if (Subscription == null)
                {
                    Subscription = new Subscription
                    {
                        PublishingEnabled = true,
                        PublishingInterval = publishingInterval,
                        Priority = 1,
                        KeepAliveCount = 10,
                        LifetimeCount = 20,
                        MaxNotificationsPerPublish = 1000,
                        TimestampsToReturn = TimestampsToReturn.Both
                    };
                    Session.AddSubscription(Subscription);
                    Subscription.Create();
                }
                if (callback == null)
                    callback = MonitoredItemNotification;
                if (filter == null)
                { 
                    filter = new EventFilter();
                    // browse the type model in the server address space to find the fields available for the event type.
                    SimpleAttributeOperandCollection selectClauses = new SimpleAttributeOperandCollection();
                    // must always request the NodeId for the condition instances.
                    // this can be done by specifying an operand with an empty browse path.
                    SimpleAttributeOperand operand = new SimpleAttributeOperand
                    {
                        TypeDefinitionId = ObjectTypeIds.BaseEventType,
                        AttributeId = Attributes.NodeId,
                        BrowsePath = new QualifiedNameCollection()
                    };
                    selectClauses.Add(operand);
                    CollectFields(ObjectTypeIds.AuditUpdateMethodEventType, selectClauses);
                    filter.SelectClauses = selectClauses;
                }
                MonitoredItem triggeringItemId = new MonitoredItem(Subscription.DefaultItem)
                {
                    NodeClass = NodeClass.Object,
                    StartNodeId = ObjectIds.Server,
                    AttributeId = Attributes.EventNotifier,
                    MonitoringMode = MonitoringMode.Reporting,
                    SamplingInterval = -1,
                    QueueSize = 100,
                    CacheQueueSize = 100,
                    Filter = filter
                };
                triggeringItemId.Notification += callback;
                Subscription.AddItem(triggeringItemId);
                Subscription.ApplyChanges();
                return true;
            }
            catch (Exception e)
            {
                Utils.Trace($"Monitored Event Notification exception: {e.StackTrace}");
                return false;
            }
        }
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
                GlobalDiscoveryServerClient.ServerStatusChanged += ServerStatusChanged;
                //connect to the GDS
                GlobalDiscoveryServerClient.Connect(configuredEndpoint).Wait();
                return true;
            }
            catch (Exception ex)
            {
                Utils.Trace($"Connecting to GDS exception: {ex.StackTrace}");
                return false;
            }
        }
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
                        StoreType = CertificateStoreIdentifier.DetermineStoreType(RegisteredApplication.CertificateStorePath),
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
                Utils.Trace($"Request new certificate pull mode exception: \r\n{ex.StackTrace}");
                return false;
            }
            return true;
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
            var certificateIdentifier = SecuredApplication.ToCertificateIdentifier(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate);
            RegisteredApplication.CertificateStorePath = certificateIdentifier.StorePath;
            RegisteredApplication.CertificateSubjectName = certificateIdentifier.SubjectName;
            if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates != null)
            {
                var certificateStoreIdentifier = SecuredApplication.ToCertificateStoreIdentifier(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates);
                RegisteredApplication.IssuerListStorePath = certificateStoreIdentifier.StorePath;
                if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.TrustedCertificates != null)
                {
                    CertificateList certificateList = SecuredApplication.ToCertificateList(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.TrustedCertificates);
                    //IssuerCertificates
                }
            }
            if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates != null)
            {
                var certificateStoreIdentifier = SecuredApplication.ToCertificateStoreIdentifier(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates);
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
                var certificateStoreIdentifier = SecuredApplication.ToCertificateStoreIdentifier(ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.RejectedCertificateStore);
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
            Utils.Trace("The trust list (include CRLs) was deleted locally.");
            return GetAndMergeWithGlobalDiscoveryTrustedList();
        }
        public bool GetAndMergeWithGlobalDiscoveryTrustedList()
        {
            if (!GlobalDiscoveryServerClient.IsConnected)
                return false;
            if (RegisteredApplication == null)
                return false;
            //Application user access required
            NodeId trustListId = GlobalDiscoveryServerClient.GetTrustList(RegisteredApplication.ApplicationId, null);
            if (trustListId == null)
                return false;
            TrustListDataType trustList = GlobalDiscoveryServerClient.ReadTrustList(trustListId);
            //Trust List Store Path
            if (!string.IsNullOrEmpty(RegisteredApplication.TrustListStorePath))
            {
                using (ICertificateStore trustListStore = CertificateStoreIdentifier.OpenStore(RegisteredApplication.TrustListStorePath))
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
                using ICertificateStore store = CertificateStoreIdentifier.OpenStore(RegisteredApplication.IssuerListStorePath);
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
            ReloadRegisteredApplicationCertificateList();
            Utils.Trace("The trust list (include CRLs) was downloaded from the GDS and saved locally.");
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
                    CertificateStoreIdentifier.CreateStore(CertificateStoreIdentifier.DetermineStoreType(RegisteredApplication.TrustListStorePath));
                if (!string.IsNullOrEmpty(RegisteredApplication.IssuerListStorePath))
                    CertificateStoreIdentifier.CreateStore(CertificateStoreIdentifier.DetermineStoreType(RegisteredApplication.IssuerListStorePath));
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
                if (!ReferenceEquals(sender, SessionReconnectHandler))
                    return;
                if (SessionReconnectHandler != null)
                {
                    Session = SessionReconnectHandler.Session;
                    SessionReconnectHandler.Dispose();
                }
                SessionReconnectHandler = null;
                // raise any additional notifications.
                ReconnectCompleteHandler?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Utils.Trace($"Reconnect exception: {ex.StackTrace}");
            }
        }
        private void CertificateValidatorCertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            Utils.Trace($"Untrusted Certificate: {e.Certificate}");
            try
            {
                if (ApplicationInstance == null)
                    return;
                if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration == null ||
                    !ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.AutoAcceptUntrustedCertificates ||
                    e.Error == null || 
                    e.Error.Code != StatusCodes.BadCertificateUntrusted)
                {
                    Utils.Trace(e.Error != null
                        ? $"Certificate not accepted: {e.Certificate.Subject} \r\n {e.Error.Code}"
                        : $"Certificate not accepted: {e.Certificate.Subject}");
                    return;
                }
                e.Accept = true;
                if (ApplicationInstance.ApplicationConfiguration.SecurityConfiguration.AddAppCertToTrustedStore)
                {
                    AddToTrustedStore(ApplicationInstance.ApplicationConfiguration, e.Certificate).Wait();
                    Utils.Trace($"Automatically accepted certificate and added to TrustStore: {e.Certificate.Subject}");
                }
                else
                {
                    Utils.Trace($"Automatically accepted certificate: {e.Certificate.Subject}");
                }
            }
            catch(Exception ex)
            {
                Utils.Trace($"Error accepting certificate.\r\nException:\r\n{ex.StackTrace}");
            }
        }
        private static void ServerStatusChanged(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification notification = (MonitoredItemNotification)e.NotificationValue;
            ServerStatusDataType serverStatusDataType = notification.Value.GetValue<ServerStatusDataType>(null);
            Utils.Trace($"GDS ServerStatusDataType: {serverStatusDataType}");
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
                    Utils.Trace($"{e.CurrentTime} Reconnecting in: {ReconnectPeriod}");
                    if (SessionReconnectHandler != null) 
                        return;
                    ReconnectStartingHandler?.Invoke(this, e);
                    SessionReconnectHandler = new SessionReconnectHandler();
                    SessionReconnectHandler.BeginReconnect(Session, ReconnectPeriod * 1000, ReconnectComplete);
                    return;
                }
                // raise any additional notifications.
                KeepAliveCompleteHandler?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Utils.Trace($"Keep Alive exception: {ex.StackTrace}");
            }
        }
        #endregion
    }
}

