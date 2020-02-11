using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Gds;
using Opc.Ua.Gds.Server;
using Opc.Ua.Server;

namespace Iso.Opc.ApplicationNodeManager.GDS
{
    public partial class GlobalDiscoveryServiceNodeManager
    {
        private void HasApplicationAdminAccess(ISystemContext context)
        {
            if (context != null)
            {
                RoleBasedIdentity identity = context.UserIdentity as RoleBasedIdentity;

                if ((identity == null) || (identity.Role != GdsRole.ApplicationAdmin))
                {
                    throw new ServiceResultException(StatusCodes.BadUserAccessDenied, "Application Administrator access required.");
                }
            }
        }

        private void HasApplicationUserAccess(ISystemContext context)
        {
            if (context != null)
            {
                RoleBasedIdentity identity = context.UserIdentity as RoleBasedIdentity;

                if (identity == null)
                {
                    throw new ServiceResultException(StatusCodes.BadUserAccessDenied, "Application User access required.");
                }
            }
        }
        private ICertificateGroup GetGroupForCertificate(byte[] certificate)
        {
            if (certificate != null && certificate.Length > 0)
            {
                var x509 = new X509Certificate2(certificate);

                foreach (var certificateGroup in _certificateGroups.Values)
                {
                    if (Utils.CompareDistinguishedName(certificateGroup.Certificate.Subject, x509.Issuer))
                    {
                        return certificateGroup;
                    }
                }
            }

            return null;
        }
        private async Task RevokeCertificateAsync(byte[] certificate)
        {
            if (certificate != null && certificate.Length > 0)
            {
                ICertificateGroup certificateGroup = GetGroupForCertificate(certificate);
                if (certificateGroup != null)
                {
                    try
                    {
                        X509Certificate2 x509 = new X509Certificate2(certificate);
                        await certificateGroup.RevokeCertificateAsync(x509);
                    }
                    catch (Exception e)
                    {
                        Utils.Trace(e, $"Unexpected error revoking certificate. {new X509Certificate2(certificate).Subject} for Authority={certificateGroup.Id}");
                    }
                }
            }
        }

        private ServiceResult VerifyApprovedState(CertificateRequestState state)
        {
            switch (state)
            {
                case CertificateRequestState.New:
                    return new ServiceResult(StatusCodes.BadNothingToDo, "The request has not been approved by the administrator.");
                case CertificateRequestState.Rejected:
                    return new ServiceResult(StatusCodes.BadRequestNotAllowed, "The request has been rejected by the administrator.");
                case CertificateRequestState.Accepted:
                    return new ServiceResult(StatusCodes.BadInvalidArgument, "The request has already been accepted by the application.");
                case CertificateRequestState.Approved:
                    break;
            }
            return null;
        }
        private NodeId GetTrustListId(NodeId certificateGroupId)
        {

            if (NodeId.IsNull(certificateGroupId))
            {
                certificateGroupId = _defaultApplicationGroupId;
            }

            CertificateGroup certificateGroup = null;
            if (_certificateGroups.TryGetValue(certificateGroupId, out certificateGroup))
            {
                return certificateGroup.DefaultTrustList?.NodeId;
            }

            return null;
        }

        private Boolean? GetCertificateStatus(
            NodeId certificateGroupId,
            NodeId certificateTypeId)
        {
            CertificateGroup certificateGroup = null;
            if (_certificateGroups.TryGetValue(certificateGroupId, out certificateGroup))
            {
                if (!NodeId.IsNull(certificateTypeId))
                {
                    if (!Utils.IsEqual(certificateGroup.CertificateType, certificateTypeId))
                    {
                        return null;
                    }
                }
                return certificateGroup.UpdateRequired;
            }

            return null;
        }


        #region INodeManager Members

        private void SetCertificateGroupNodes(ICertificateGroup certificateGroup)
        {
            NodeId certificateType = (typeof(global::Opc.Ua.ObjectTypeIds)).GetField(certificateGroup.Configuration.CertificateType).GetValue(null) as NodeId;
            certificateGroup.CertificateType = certificateType;
            certificateGroup.DefaultTrustList = null;
            if (Utils.Equals(certificateType, global::Opc.Ua.ObjectTypeIds.HttpsCertificateType))
            {
                certificateGroup.Id = _defaultHttpsGroupId;
                certificateGroup.DefaultTrustList = (TrustListState)FindPredefinedNode(ExpandedNodeId.ToNodeId(global::Opc.Ua.Gds.ObjectIds.Directory_CertificateGroups_DefaultHttpsGroup_TrustList, Server.NamespaceUris), typeof(TrustListState));
            }
            else if (Utils.Equals(certificateType, global::Opc.Ua.ObjectTypeIds.UserCredentialCertificateType))
            {
                certificateGroup.Id = _defaultUserTokenGroupId;
                certificateGroup.DefaultTrustList = (TrustListState)FindPredefinedNode(ExpandedNodeId.ToNodeId(global::Opc.Ua.Gds.ObjectIds.Directory_CertificateGroups_DefaultUserTokenGroup_TrustList, Server.NamespaceUris), typeof(TrustListState));
            }
            else if (Utils.Equals(certificateType, global::Opc.Ua.ObjectTypeIds.ApplicationCertificateType) ||
                Utils.Equals(certificateType, global::Opc.Ua.ObjectTypeIds.RsaMinApplicationCertificateType) ||
                Utils.Equals(certificateType, global::Opc.Ua.ObjectTypeIds.RsaSha256ApplicationCertificateType)
                )
            {
                certificateGroup.Id = _defaultApplicationGroupId;
                certificateGroup.DefaultTrustList = (TrustListState)FindPredefinedNode(ExpandedNodeId.ToNodeId(global::Opc.Ua.Gds.ObjectIds.Directory_CertificateGroups_DefaultApplicationGroup_TrustList, Server.NamespaceUris), typeof(TrustListState));
            }
            else
            {
                throw new NotImplementedException("Unknown certificate type {certificateGroup.Configuration.CertificateType}. Use ApplicationCertificateType, HttpsCertificateType or UserCredentialCertificateType");
            }

            if (certificateGroup.DefaultTrustList != null)
            {
                certificateGroup.DefaultTrustList.Handle = new TrustList(
                    certificateGroup.DefaultTrustList,
                    certificateGroup.Configuration.TrustedListPath,
                    certificateGroup.Configuration.IssuerListPath,
                    new TrustList.SecureAccess(HasApplicationUserAccess),
                    new TrustList.SecureAccess(HasApplicationAdminAccess));
            }
        }

        /// <summary>
        /// Does any initialization required before the address space can be used.
        /// </summary>
        /// <remarks>
        /// The externalReferences is an out parameter that allows the node manager to link to nodes
        /// in other node managers. For example, the 'Objects' node is managed by the CoreNodeManager and
        /// should have a reference to the root folder node(s) exposed by this node manager.  
        /// </remarks>
        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                base.CreateAddressSpace(externalReferences);
                _database.NamespaceIndex = this.NamespaceIndexes[0];
                _request.NamespaceIndex = this.NamespaceIndexes[0];
                foreach (CertificateGroupConfiguration certificateGroupConfiguration in _certificateGroupConfigurationCollection)
                {
                    try
                    {
                        CertificateGroup certificateGroup = _certificateGroupFactory.Create(
							_authoritiesStorePath, certificateGroupConfiguration);
                        SetCertificateGroupNodes(certificateGroup);
                        certificateGroup.Init();
                        _certificateGroups[certificateGroup.Id] = certificateGroup;
                    }
                    catch (Exception e)
                    {
                        Utils.Trace(e, "Unexpected error initializing certificateGroup: " + certificateGroupConfiguration.Id + "\r\n" + e.StackTrace);
                        // make sure gds server doesn't start without cert groups!
                        throw e;
                    }
                }
                _certTypeMap = new Dictionary<NodeId, string>
                {
                    // list of supported cert type mappings (V1.04)
                    { global::Opc.Ua.ObjectTypeIds.HttpsCertificateType, nameof(global::Opc.Ua.ObjectTypeIds.HttpsCertificateType) },
                    { global::Opc.Ua.ObjectTypeIds.UserCredentialCertificateType, nameof(global::Opc.Ua.ObjectTypeIds.UserCredentialCertificateType) },
                    { global::Opc.Ua.ObjectTypeIds.ApplicationCertificateType, nameof(global::Opc.Ua.ObjectTypeIds.ApplicationCertificateType) },
                    { global::Opc.Ua.ObjectTypeIds.RsaMinApplicationCertificateType, nameof(global::Opc.Ua.ObjectTypeIds.RsaMinApplicationCertificateType) },
                    { global::Opc.Ua.ObjectTypeIds.RsaSha256ApplicationCertificateType, nameof(global::Opc.Ua.ObjectTypeIds.RsaSha256ApplicationCertificateType) }
                };
            }
        }

        /// <summary>
        /// Loads a node set from a file or resource and adds them to the set of predefined nodes.
        /// </summary>
        protected override NodeStateCollection LoadPredefinedNodes(ISystemContext context)
        {
            NodeStateCollection predefinedNodes = new NodeStateCollection();
            predefinedNodes.LoadFromBinaryResource(context, "Opc.Ua.Gds.Server.Model.Opc.Ua.Gds.PredefinedNodes.uanodes", typeof(ApplicationsNodeManager).GetTypeInfo().Assembly, true);
            return predefinedNodes;
        }

        /// <summary>
        /// Replaces the generic node with a node specific to the model.
        /// </summary>
        protected override NodeState AddBehaviourToPredefinedNode(ISystemContext context, NodeState predefinedNode)
        {
            BaseObjectState passiveNode = predefinedNode as BaseObjectState;

            if (passiveNode == null)
            {
                return predefinedNode;
            }

            NodeId typeId = passiveNode.TypeDefinitionId;

            if (!IsNodeIdInNamespace(typeId) || typeId.IdType != IdType.Numeric)
            {
                return predefinedNode;
            }

            switch ((uint)typeId.Identifier)
            {
                case global::Opc.Ua.Gds.ObjectTypes.CertificateDirectoryType:
                    {
                        if (passiveNode is global::Opc.Ua.Gds.CertificateDirectoryState)
                        {
                            break;
                        }

                        CertificateDirectoryState activeNode = new CertificateDirectoryState(passiveNode.Parent);

                        activeNode.Create(context, passiveNode);
                        activeNode.QueryServers.OnCall = new QueryServersMethodStateMethodCallHandler(OnQueryServers);
                        activeNode.QueryApplications.OnCall = new QueryApplicationsMethodStateMethodCallHandler(OnQueryApplications);
                        activeNode.RegisterApplication.OnCall = new RegisterApplicationMethodStateMethodCallHandler(OnRegisterApplication);
                        activeNode.UpdateApplication.OnCall = new UpdateApplicationMethodStateMethodCallHandler(OnUpdateApplication);
                        activeNode.UnregisterApplication.OnCall = new UnregisterApplicationMethodStateMethodCallHandler(OnUnregisterApplication);
                        activeNode.FindApplications.OnCall = new FindApplicationsMethodStateMethodCallHandler(OnFindApplications);
                        activeNode.GetApplication.OnCall = new GetApplicationMethodStateMethodCallHandler(OnGetApplication);
                        activeNode.StartNewKeyPairRequest.OnCall = new StartNewKeyPairRequestMethodStateMethodCallHandler(OnStartNewKeyPairRequest);
                        activeNode.FinishRequest.OnCall = new FinishRequestMethodStateMethodCallHandler(OnFinishRequest);
                        activeNode.GetCertificateGroups.OnCall = new GetCertificateGroupsMethodStateMethodCallHandler(OnGetCertificateGroups);
                        activeNode.GetTrustList.OnCall = new GetTrustListMethodStateMethodCallHandler(OnGetTrustList);
                        activeNode.GetCertificateStatus.OnCall = new GetCertificateStatusMethodStateMethodCallHandler(OnGetCertificateStatus);
                        activeNode.StartSigningRequest.OnCall = new StartSigningRequestMethodStateMethodCallHandler(OnStartSigningRequest);
                        // TODO
                        //activeNode.RevokeCertificate.OnCall = new RevokeCertificateMethodStateMethodCallHandler(OnRevokeCertificate);

                        activeNode.CertificateGroups.DefaultApplicationGroup.CertificateTypes.Value = new NodeId[] { global::Opc.Ua.ObjectTypeIds.RsaSha256ApplicationCertificateType };
                        activeNode.CertificateGroups.DefaultApplicationGroup.TrustList.LastUpdateTime.Value = DateTime.UtcNow;
                        activeNode.CertificateGroups.DefaultApplicationGroup.TrustList.Writable.Value = false;
                        activeNode.CertificateGroups.DefaultApplicationGroup.TrustList.UserWritable.Value = false;

                        activeNode.CertificateGroups.DefaultHttpsGroup.CertificateTypes.Value = new NodeId[] { global::Opc.Ua.ObjectTypeIds.HttpsCertificateType };
                        activeNode.CertificateGroups.DefaultHttpsGroup.TrustList.LastUpdateTime.Value = DateTime.UtcNow;
                        activeNode.CertificateGroups.DefaultHttpsGroup.TrustList.Writable.Value = false;
                        activeNode.CertificateGroups.DefaultHttpsGroup.TrustList.UserWritable.Value = false;

                        activeNode.CertificateGroups.DefaultUserTokenGroup.CertificateTypes.Value = new NodeId[] { global::Opc.Ua.ObjectTypeIds.UserCredentialCertificateType };
                        activeNode.CertificateGroups.DefaultUserTokenGroup.TrustList.LastUpdateTime.Value = DateTime.UtcNow;
                        activeNode.CertificateGroups.DefaultUserTokenGroup.TrustList.Writable.Value = false;
                        activeNode.CertificateGroups.DefaultUserTokenGroup.TrustList.UserWritable.Value = false;

                        // replace the node in the parent.
                        if (passiveNode.Parent != null)
                        {
                            passiveNode.Parent.ReplaceChild(context, activeNode);
                        }

                        return activeNode;
                    }
            }

            return predefinedNode;
        }

        private ServiceResult OnQueryServers(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            uint startingRecordId,
            uint maxRecordsToReturn,
            string applicationName,
            string applicationUri,
            string productUri,
            string[] serverCapabilities,
            ref DateTime lastCounterResetTime,
            ref ServerOnNetwork[] servers)
        {

            Utils.Trace(Utils.TraceMasks.Information, $"QueryServers: {applicationUri} {applicationName}");

            servers = _database.QueryServers(
                startingRecordId,
                maxRecordsToReturn,
                applicationName,
                applicationUri,
                productUri,
                serverCapabilities,
                out lastCounterResetTime);

            return ServiceResult.Good;
        }

        private ServiceResult OnQueryApplications(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            uint startingRecordId,
            uint maxRecordsToReturn,
            string applicationName,
            string applicationUri,
            uint applicationType,
            string productUri,
            string[] serverCapabilities,
            ref DateTime lastCounterResetTime,
            ref uint nextRecordId,
            ref ApplicationDescription[] applications
            )
        {
            Utils.Trace(Utils.TraceMasks.Information, $"QueryServers: {applicationUri} {applicationName}");

            applications = _database.QueryApplications(
                startingRecordId,
                maxRecordsToReturn,
                applicationName,
                applicationUri,
                applicationType,
                productUri,
                serverCapabilities,
                out lastCounterResetTime,
                out nextRecordId
                );
            return ServiceResult.Good;
        }

        private ServiceResult OnRegisterApplication(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            ApplicationRecordDataType application,
            ref NodeId applicationId)
        {
            HasApplicationAdminAccess(context);

            Utils.Trace(Utils.TraceMasks.Information, $"OnRegisterApplication: {application.ApplicationUri}");

            applicationId = _database.RegisterApplication(application);

            return ServiceResult.Good;
        }

        private ServiceResult OnUpdateApplication(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            ApplicationRecordDataType application)
        {
            HasApplicationAdminAccess(context);

            Utils.Trace(Utils.TraceMasks.Information, $"OnRegisterApplication: {application.ApplicationUri}");

            var record = _database.GetApplication(application.ApplicationId);

            if (record == null)
            {
                return new ServiceResult(StatusCodes.BadNotFound, "The application id does not exist.");
            }

            _database.RegisterApplication(application);

            return ServiceResult.Good;
        }

        private ServiceResult OnUnregisterApplication(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId applicationId)
        {
            HasApplicationAdminAccess(context);

            Utils.Trace(Utils.TraceMasks.Information, $"OnRegisterApplication: {applicationId.ToString()}");


            foreach (var certType in _certTypeMap)
            {
                try
                {
                    byte[] certificate;
                    if (_database.GetApplicationCertificate(applicationId, certType.Value, out certificate))
                    {
                        if (certificate != null)
                        {
                            RevokeCertificateAsync(certificate).Wait();
                        }
                    }
                }
                catch
                {
                    Utils.Trace(Utils.TraceMasks.Error, $"Failed to revoke: { certType.Value}");
                }
            }

            _database.UnregisterApplication(applicationId);

            return ServiceResult.Good;
        }

        private ServiceResult OnFindApplications(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            string applicationUri,
            ref ApplicationRecordDataType[] applications)
        {
            HasApplicationUserAccess(context);
            Utils.Trace(Utils.TraceMasks.Information, $"OnFindApplications: {applicationUri}");
            applications = _database.FindApplications(applicationUri);
            return ServiceResult.Good;
        }

        private ServiceResult OnGetApplication(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId applicationId,
            ref ApplicationRecordDataType application)
        {
            HasApplicationUserAccess(context);
            Utils.Trace(Utils.TraceMasks.Information, $"OnGetApplication: {applicationId}");
            application = _database.GetApplication(applicationId);
            return ServiceResult.Good;
        }

        private ServiceResult CheckHttpsDomain(ApplicationRecordDataType application, string commonName)
        {
            if (application.ApplicationType == ApplicationType.Client)
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument, "Cannot issue HTTPS certificates to client applications.");
            }

            bool found = false;

            if (application.DiscoveryUrls != null)
            {
                foreach (var discoveryUrl in application.DiscoveryUrls)
                {
                    if (Uri.IsWellFormedUriString(discoveryUrl, UriKind.Absolute))
                    {
                        Uri url = new Uri(discoveryUrl);

                        if (url.Scheme == Utils.UriSchemeHttps)
                        {
                            if (Utils.AreDomainsEqual(commonName, url.DnsSafeHost))
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument, "Cannot issue HTTPS certificates to server applications without a matching HTTPS discovery URL.");
            }

            return ServiceResult.Good;
        }

        private string GetDefaultHttpsDomain(ApplicationRecordDataType application)
        {
            if (application.DiscoveryUrls != null)
            {
                foreach (var discoveryUrl in application.DiscoveryUrls)
                {
                    if (Uri.IsWellFormedUriString(discoveryUrl, UriKind.Absolute))
                    {
                        Uri url = new Uri(discoveryUrl);

                        if (url.Scheme == Utils.UriSchemeHttps)
                        {
                            return url.DnsSafeHost;
                        }
                    }
                }
            }

            throw new ServiceResultException(StatusCodes.BadInvalidArgument, "Cannot issue HTTPS certificates to server applications without a HTTPS discovery URL.");
        }

        private string GetDefaultUserToken()
        {
            return "USER";
        }

        private string GetSubjectName(ApplicationRecordDataType application, CertificateGroup certificateGroup, string subjectName)
        {
            bool contextFound = false;

            var fields = Utils.ParseDistinguishedName(subjectName);

            StringBuilder builder = new StringBuilder();

            foreach (var field in fields)
            {
                if (builder.Length > 0)
                {
                    builder.Append(",");
                }

                if (field.StartsWith("CN=", StringComparison.Ordinal))
                {
                    if (certificateGroup.Id == _defaultHttpsGroupId)
                    {
                        var error = CheckHttpsDomain(application, field.Substring(3));

                        if (StatusCode.IsBad(error.StatusCode))
                        {
                            builder.Append("CN=");
                            builder.Append(GetDefaultHttpsDomain(application));
                            continue;
                        }
                    }
                }

                contextFound |= (field.StartsWith("DC=", StringComparison.Ordinal) || field.StartsWith("O=", StringComparison.Ordinal));

                builder.Append(field);
            }

            if (!contextFound)
            {
                if (!String.IsNullOrEmpty(_defaultSubjectNameContext))
                {
                    builder.Append(_defaultSubjectNameContext);
                }
            }

            return builder.ToString();
        }

        private string[] GetDefaultDomainNames(ApplicationRecordDataType application)
        {
            List<string> names = new List<string>();

            if (application.DiscoveryUrls != null && application.DiscoveryUrls.Count > 0)
            {
                foreach (var discoveryUrl in application.DiscoveryUrls)
                {
                    if (Uri.IsWellFormedUriString(discoveryUrl, UriKind.Absolute))
                    {
                        Uri url = new Uri(discoveryUrl);

                        foreach (var name in names)
                        {
                            if (Utils.AreDomainsEqual(name, url.DnsSafeHost))
                            {
                                url = null;
                                break;
                            }
                        }

                        if (url != null)
                        {
                            names.Add(url.DnsSafeHost);
                        }
                    }
                }
            }

            return names.ToArray();
        }

        private ServiceResult OnStartNewKeyPairRequest(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId applicationId,
            NodeId certificateGroupId,
            NodeId certificateTypeId,
            string subjectName,
            string[] domainNames,
            string privateKeyFormat,
            string privateKeyPassword,
            ref NodeId requestId)
        {
            HasApplicationAdminAccess(context);

            var application = _database.GetApplication(applicationId);

            if (application == null)
            {
                return new ServiceResult(StatusCodes.BadNotFound, "The ApplicationId does not refer to a valid application.");
            }

            if (NodeId.IsNull(certificateGroupId))
            {
                certificateGroupId = ExpandedNodeId.ToNodeId(global::Opc.Ua.Gds.ObjectIds.Directory_CertificateGroups_DefaultApplicationGroup, Server.NamespaceUris);
            }

            CertificateGroup certificateGroup = null;
            if (!_certificateGroups.TryGetValue(certificateGroupId, out certificateGroup))
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument, "The certificateGroup is not supported.");
            }

            if (!NodeId.IsNull(certificateTypeId))
            {
                if (!Server.TypeTree.IsTypeOf(certificateGroup.CertificateType, certificateTypeId))
                {
                    return new ServiceResult(StatusCodes.BadInvalidArgument, "The CertificateType is not supported by the certificateGroup.");
                }
            }
            else
            {
                certificateTypeId = certificateGroup.CertificateType;
            }

            string certificateTypeNameId;
            if (!_certTypeMap.TryGetValue(certificateTypeId, out certificateTypeNameId))
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument, "The CertificateType is invalid.");
            }

            if (!String.IsNullOrEmpty(subjectName))
            {
                subjectName = GetSubjectName(application, certificateGroup, subjectName);
            }
            else
            {
                StringBuilder buffer = new StringBuilder();

                buffer.Append("CN=");

                if ((NodeId.IsNull(certificateGroup.Id) || (certificateGroup.Id == _defaultApplicationGroupId)) && (application.ApplicationNames.Count > 0))
                {
                    buffer.Append(application.ApplicationNames[0]);
                }
                else if (certificateGroup.Id == _defaultHttpsGroupId)
                {
                    buffer.Append(GetDefaultHttpsDomain(application));
                }
                else if (certificateGroup.Id == _defaultUserTokenGroupId)
                {
                    buffer.Append(GetDefaultUserToken());
                }

                if (!String.IsNullOrEmpty(_defaultSubjectNameContext))
                {
                    buffer.Append(_defaultSubjectNameContext);
                }

                subjectName = buffer.ToString();
            }

            if (domainNames != null && domainNames.Length > 0)
            {
                foreach (var domainName in domainNames)
                {
                    if (Uri.CheckHostName(domainName) == UriHostNameType.Unknown)
                    {
                        return new ServiceResult(StatusCodes.BadInvalidArgument, $"The domainName ({domainName}) is not a valid DNS Name or IPAddress.");
                    }
                }
            }
            else
            {
                domainNames = GetDefaultDomainNames(application);
            }

            requestId = _request.StartNewKeyPairRequest(
                applicationId,
                certificateGroup.Configuration.Id,
                certificateTypeNameId,
                subjectName,
                domainNames,
                privateKeyFormat,
                privateKeyPassword,
                context.UserIdentity?.DisplayName);

            if (_autoApprove)
            {
                try
                {
                    _request.ApproveRequest(requestId, false);
                }
                catch
                {
                    // ignore error as user may not have authorization to approve requests
                }
            }

            return ServiceResult.Good;
        }

        private ServiceResult OnStartSigningRequest(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId applicationId,
            NodeId certificateGroupId,
            NodeId certificateTypeId,
            byte[] certificateRequest,
            ref NodeId requestId)
        {
            HasApplicationAdminAccess(context);

            var application = _database.GetApplication(applicationId);

            if (application == null)
            {
                return new ServiceResult(StatusCodes.BadNotFound, "The ApplicationId does not refer to a valid application.");
            }

            if (NodeId.IsNull(certificateGroupId))
            {
                certificateGroupId = ExpandedNodeId.ToNodeId(global::Opc.Ua.Gds.ObjectIds.Directory_CertificateGroups_DefaultApplicationGroup,Server.NamespaceUris);
            }

            CertificateGroup certificateGroup = null;
            if (!_certificateGroups.TryGetValue(certificateGroupId, out certificateGroup))
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument, "The CertificateGroupId does not refer to a supported certificateGroup.");
            }

            if (!NodeId.IsNull(certificateTypeId))
            {
                if (!Server.TypeTree.IsTypeOf(certificateGroup.CertificateType, certificateTypeId))
                {
                    return new ServiceResult(StatusCodes.BadInvalidArgument, "The CertificateTypeId is not supported by the certificateGroup.");
                }
            }
            else
            {
                certificateTypeId = certificateGroup.CertificateType;
            }

            string certificateTypeNameId;
            if (!_certTypeMap.TryGetValue(certificateTypeId, out certificateTypeNameId))
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument, "The CertificateType is invalid.");
            }


            // verify the CSR integrity for the application
            certificateGroup.VerifySigningRequestAsync(
                application,
                certificateRequest
                ).Wait();

            // store request in the queue for approval
            requestId = _request.StartSigningRequest(
                applicationId,
                certificateGroup.Configuration.Id,
                certificateTypeNameId,
                certificateRequest,
                context.UserIdentity?.DisplayName);

            if (_autoApprove)
            {
                try
                {
                    _request.ApproveRequest(requestId, false);
                }
                catch
                {
                    // ignore error as user may not have authorization to approve requests
                }
            }

            return ServiceResult.Good;
        }

        private ServiceResult OnFinishRequest(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId applicationId,
            NodeId requestId,
            ref byte[] signedCertificate,
            ref byte[] privateKey,
            ref byte[][] issuerCertificates)
        {
            signedCertificate = null;
            issuerCertificates = null;
            privateKey = null;
            HasApplicationAdminAccess(context);

            var application = _database.GetApplication(applicationId);
            if (application == null)
            {
                return new ServiceResult(StatusCodes.BadNotFound, "The ApplicationId does not refer to a valid application.");
            }

            string certificateGroupId;
            string certificateTypeId;

            var state = _request.FinishRequest(
                applicationId,
                requestId,
                out certificateGroupId,
                out certificateTypeId,
                out signedCertificate,
                out privateKey);

            var approvalState = VerifyApprovedState(state);
            if (approvalState != null)
            {
                return approvalState;
            }

            CertificateGroup certificateGroup = null;
            if (!String.IsNullOrWhiteSpace(certificateGroupId))
            {
                foreach (var group in _certificateGroups)
                {
                    if (String.Compare(group.Value.Configuration.Id, certificateGroupId, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        certificateGroup = group.Value;
                        break;
                    }
                }
            }

            if (certificateGroup == null)
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument, "The CertificateGroupId does not refer to a supported certificate group.");
            }

            NodeId certificateTypeNodeId;
            certificateTypeNodeId = _certTypeMap.Where(
                pair => pair.Value.Equals(certificateTypeId, StringComparison.OrdinalIgnoreCase))
                .Select(pair => pair.Key).SingleOrDefault();

            if (!NodeId.IsNull(certificateTypeNodeId))
            {
                if (!Server.TypeTree.IsTypeOf(certificateGroup.CertificateType, certificateTypeNodeId))
                {
                    return new ServiceResult(StatusCodes.BadInvalidArgument, "The CertificateTypeId is not supported by the certificateGroup.");
                }
            }

            // distinguish cert creation at approval/complete time
            X509Certificate2 certificate = null;
            if (signedCertificate == null)
            {
                byte[] certificateRequest;
                string subjectName;
                string[] domainNames;
                string privateKeyFormat;
                string privateKeyPassword;

                state = _request.ReadRequest(
                    applicationId,
                    requestId,
                    out certificateGroupId,
                    out certificateTypeId,
                    out certificateRequest,
                    out subjectName,
                    out domainNames,
                    out privateKeyFormat,
                    out privateKeyPassword
                    );

                approvalState = VerifyApprovedState(state);
                if (approvalState != null)
                {
                    return approvalState;
                }

                if (certificateRequest != null)
                {
                    try
                    {
                        string[] defaultDomainNames = GetDefaultDomainNames(application);
                        certificate = certificateGroup.SigningRequestAsync(
                            application,
                            defaultDomainNames,
                            certificateRequest
                            ).Result;
                    }
                    catch (Exception e)
                    {
                        StringBuilder error = new StringBuilder();

                        error.Append("Error Generating Certificate=" + e.Message);
                        error.Append("\r\nApplicationId=" + applicationId.ToString());
                        error.Append("\r\nApplicationUri=" + application.ApplicationUri);
                        error.Append("\r\nApplicationName=" + application.ApplicationNames[0].Text);

                        return new ServiceResult(StatusCodes.BadConfigurationError, error.ToString());
                    }
                }
                else
                {
                    X509Certificate2KeyPair newKeyPair = null;
                    try
                    {
                        newKeyPair = certificateGroup.NewKeyPairRequestAsync(
                            application,
                            subjectName,
                            domainNames,
                            privateKeyFormat,
                            privateKeyPassword).Result;
                    }
                    catch (Exception e)
                    {
                        StringBuilder error = new StringBuilder();

                        error.Append("Error Generating New Key Pair Certificate=" + e.Message);
                        error.Append("\r\nApplicationId=" + applicationId.ToString());
                        error.Append("\r\nApplicationUri=" + application.ApplicationUri);

                        return new ServiceResult(StatusCodes.BadConfigurationError, error.ToString());
                    }

                    certificate = newKeyPair.Certificate;
                    privateKey = newKeyPair.PrivateKey;

                }

                signedCertificate = certificate.RawData;
            }
            else
            {
                certificate = new X509Certificate2(signedCertificate);
            }

            // TODO: return chain, verify issuer chain cert is up to date, otherwise update local chain
            issuerCertificates = new byte[1][];
            issuerCertificates[0] = certificateGroup.Certificate.RawData;

            // store new app certificate
            using (ICertificateStore store = CertificateStoreIdentifier.OpenStore(_applicationCertificatesStorePath))
            {
                store.Add(certificate).Wait();
            }

            _database.SetApplicationCertificate(applicationId, _certTypeMap[certificateGroup.CertificateType], signedCertificate);
            _request.AcceptRequest(requestId, signedCertificate);
            return ServiceResult.Good;
        }

        public ServiceResult OnGetCertificateGroups(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId applicationId,
            ref NodeId[] certificateGroupIds)
        {
            HasApplicationUserAccess(context);

            var application = _database.GetApplication(applicationId);

            if (application == null)
            {
                return new ServiceResult(StatusCodes.BadNotFound, "The ApplicationId does not refer to a valid application.");
            }

            var certificateGroupIdList = new List<NodeId>();
            foreach (var certificateGroup in _certificateGroups)
            {
                NodeId key = certificateGroup.Key;
                certificateGroupIdList.Add(key);
            }
            certificateGroupIds = certificateGroupIdList.ToArray();

            return ServiceResult.Good;
        }

        public ServiceResult OnGetTrustList(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId applicationId,
            NodeId certificateGroupId,
            ref NodeId trustListId)
        {
            HasApplicationUserAccess(context);

            var application = _database.GetApplication(applicationId);

            if (application == null)
            {
                return new ServiceResult(StatusCodes.BadNotFound, "The ApplicationId does not refer to a valid application.");
            }

            if (NodeId.IsNull(certificateGroupId))
            {
                certificateGroupId = _defaultApplicationGroupId;
            }

            trustListId = GetTrustListId(certificateGroupId);

            if (trustListId == null)
            {
                return new ServiceResult(StatusCodes.BadNotFound, "The CertificateGroupId does not refer to a group that is valid for the application.");
            }

            return ServiceResult.Good;
        }

        public ServiceResult OnGetCertificateStatus(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId applicationId,
            NodeId certificateGroupId,
            NodeId certificateTypeId,
            ref Boolean updateRequired)
        {
            HasApplicationUserAccess(context);

            var application = _database.GetApplication(applicationId);

            if (application == null)
            {
                return new ServiceResult(StatusCodes.BadNotFound, "The ApplicationId does not refer to a valid application.");
            }

            if (NodeId.IsNull(certificateGroupId))
            {
                certificateGroupId = _defaultApplicationGroupId;
            }

            Boolean? updateRequiredResult = GetCertificateStatus(certificateGroupId, certificateTypeId);
            if (updateRequiredResult == null)
            {
                return new ServiceResult(StatusCodes.BadNotFound, "The CertificateGroupId and CertificateTypeId do not refer to a group and type that is valid for the application.");
            }

            updateRequired = (Boolean)updateRequiredResult;

            return ServiceResult.Good;
        }

        /// <summary>
        /// Frees any resources allocated for the address space.
        /// </summary>
        public override void DeleteAddressSpace()
        {
            lock (Lock)
            {
                // TBD
            }
        }

        /// <summary>
        /// Returns a unique handle for the node.
        /// </summary>
        protected override NodeHandle GetManagerHandle(ServerSystemContext context, NodeId nodeId, IDictionary<NodeId, NodeState> cache)
        {
            lock (Lock)
            {
                // quickly exclude nodes that are not in the namespace. 
                if (!IsNodeIdInNamespace(nodeId))
                {
                    return null;
                }

                NodeState node = null;

                // check cache (the cache is used because the same node id can appear many times in a single request).
                if (cache != null)
                {
                    if (cache.TryGetValue(nodeId, out node))
                    {
                        return new NodeHandle(nodeId, node);
                    }
                }

                // look up predefined node.
                if (PredefinedNodes.TryGetValue(nodeId, out node))
                {
                    NodeHandle handle = new NodeHandle(nodeId, node);

                    if (cache != null)
                    {
                        cache.Add(nodeId, node);
                    }

                    return handle;
                }

                // node not found.
                return null;
            }
        }

        /// <summary>
        /// Verifies that the specified node exists.
        /// </summary>
        protected override NodeState ValidateNode(
            ServerSystemContext context,
            NodeHandle handle,
            IDictionary<NodeId, NodeState> cache)
        {
            // not valid if no root.
            if (handle == null)
            {
                return null;
            }

            // check if previously validated.
            if (handle.Validated)
            {
                return handle.Node;
            }

            // lookup in operation cache.
            NodeState target = FindNodeInCache(context, handle, cache);

            if (target != null)
            {
                handle.Node = target;
                handle.Validated = true;
                return handle.Node;
            }

            // put root into operation cache.
            if (cache != null)
            {
                cache[handle.NodeId] = target;
            }

            handle.Node = target;
            handle.Validated = true;
            return handle.Node;
        }
        #endregion

        #region Overridden Methods
        #endregion
    }
}
