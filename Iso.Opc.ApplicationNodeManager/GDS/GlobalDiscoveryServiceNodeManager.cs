using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Opc.Ua;
using Opc.Ua.Gds.Server;
using Opc.Ua.Gds.Server.Database;
using Opc.Ua.Server;

namespace Iso.Opc.ApplicationNodeManager.GDS
{
    public sealed partial class GlobalDiscoveryServiceNodeManager : CustomNodeManager2
    {
        #region Fields
        private readonly string _authoritiesStorePath;
        private readonly string _applicationCertificatesStorePath;
        private readonly string _defaultSubjectNameContext;
        private readonly CertificateGroupConfigurationCollection _certificateGroupConfigurationCollection;
        private StringCollection _knownHostNames;
        private readonly bool _autoApprove;
        private readonly IApplicationsDatabase _database;
        private readonly ICertificateRequest _request;
        private readonly ICertificateGroup _certificateGroupFactory;
        private readonly Dictionary<NodeId, CertificateGroup> _certificateGroups;
        private Dictionary<NodeId, string> _certTypeMap;
        private uint _nextNodeId;
        private readonly NodeId _defaultApplicationGroupId;
        private readonly NodeId _defaultHttpsGroupId;
        private readonly NodeId _defaultUserTokenGroupId;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the node manager.
        /// </summary>
        public GlobalDiscoveryServiceNodeManager(IServerInternal server, ApplicationConfiguration applicationConfiguration,
            IApplicationsDatabase database, ICertificateRequest request, ICertificateGroup certificateGroup, bool autoApprove = false)
            : base(server, applicationConfiguration)
        {
            NamespaceUris = new List<string> { $"http://{Dns.GetHostName()}/GDS/Default", global::Opc.Ua.Gds.Namespaces.OpcUaGds };
            _nextNodeId = 0;
            SystemContext.NodeIdFactory = this;
            _defaultSubjectNameContext = "CN=" + applicationConfiguration.ApplicationName + ", DC=" + Dns.GetHostName();
            _certificateGroupConfigurationCollection = new CertificateGroupConfigurationCollection();

            //Authorities Certificates Store Path
            string authoritiesStorePathDirectory = AppDomain.CurrentDomain.BaseDirectory + "pki\\authoritie";
            if (!Directory.Exists(authoritiesStorePathDirectory))
                Directory.CreateDirectory(authoritiesStorePathDirectory);
            _authoritiesStorePath = authoritiesStorePathDirectory;
            //Application Certificates Store Path
            string applicationCertificatesStorePathDirectory = AppDomain.CurrentDomain.BaseDirectory + "pki\\applications";
            if (!Directory.Exists(applicationCertificatesStorePathDirectory))
                Directory.CreateDirectory(applicationCertificatesStorePathDirectory);
            _applicationCertificatesStorePath = applicationCertificatesStorePathDirectory;
            //Base Certificates Store Path
            string baseCertificateGroupStorePathDirectory = AppDomain.CurrentDomain.BaseDirectory + "pki\\CA\\default";
            if (!Directory.Exists(baseCertificateGroupStorePathDirectory))
                Directory.CreateDirectory(baseCertificateGroupStorePathDirectory);
            _certificateGroupConfigurationCollection.Add(new CertificateGroupConfiguration {
                Id = "Default",
                CertificateType = "RsaSha256ApplicationCertificateType",
                SubjectName = _defaultSubjectNameContext,
                BaseStorePath = baseCertificateGroupStorePathDirectory,
                DefaultCertificateLifetime = 12,
                DefaultCertificateKeySize = 2048,
                DefaultCertificateHashSize = 256,
                CACertificateLifetime = 60,
                CACertificateKeySize = 2048,
                CACertificateHashSize = 256
            });
            _knownHostNames = new StringCollection();

            _defaultApplicationGroupId = ExpandedNodeId.ToNodeId(global::Opc.Ua.Gds.ObjectIds.Directory_CertificateGroups_DefaultApplicationGroup, Server.NamespaceUris);
            _defaultHttpsGroupId = ExpandedNodeId.ToNodeId(global::Opc.Ua.Gds.ObjectIds.Directory_CertificateGroups_DefaultHttpsGroup, Server.NamespaceUris);
            _defaultUserTokenGroupId = ExpandedNodeId.ToNodeId(global::Opc.Ua.Gds.ObjectIds.Directory_CertificateGroups_DefaultUserTokenGroup, Server.NamespaceUris);

            _autoApprove = autoApprove;
            _database = database;
            _request = request;
            _certificateGroupFactory = certificateGroup;
            _certificateGroups = new Dictionary<NodeId, CertificateGroup>();

            try
            {
                ServerOnNetwork[] results = _database.QueryServers(0, 5, null, null, null, null, out DateTime _);
                Utils.Trace($"QueryServers Returned: {results.Length} records");
                foreach (ServerOnNetwork result in results)
                {
                    Utils.Trace($"Server Found at {result.DiscoveryUrl}");
                }
            }
            catch (Exception e)
            {
                Utils.Trace($"Could not connect to the Database! Exception:\r\n{e.InnerException}");
                Utils.Trace("Initialize Database tables!");
                _database.Initialize();
                Utils.Trace("Database Initialized!");
            }
            Server.MessageContext.Factory.AddEncodeableTypes(typeof(global::Opc.Ua.Gds.ObjectIds).GetTypeInfo().Assembly);
        }
        #endregion
    }
}
