using System;
using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Server;

namespace Iso.Opc.Interface
{
    public class AbstractApplicationNodeManagerPlugin : IApplicationNodeManagerPlugin
    {
        public virtual string ApplicationName { get; set; }
        public virtual string Description { get; set; }
        public virtual string Author { get; set; }
        public virtual string Version { get; set; }
        public virtual CustomNodeManager2 ApplicationNodeManager { get; set; }
        public virtual void Initialise(IDictionary<NodeId, IList<IReference>> externalReferences, ISystemContext context, NodeState predefinedNode)
        {
            //BaseObjectState passiveNode = predefinedNode as BaseObjectState;

            //if (passiveNode == null)
            //{
            //    return predefinedNode;
            //}

            //NodeId typeId = passiveNode.TypeDefinitionId;

            //if (!IsNodeIdInNamespace(typeId) || typeId.IdType != IdType.Numeric)
            //{
            //    return predefinedNode;
            //}

            //switch ((uint)typeId.Identifier)
            //{
            //    case global::Opc.Ua.Gds.ObjectTypes.CertificateDirectoryType:
            //        {
            //            if (passiveNode is global::Opc.Ua.Gds.CertificateDirectoryState)
            //            {
            //                break;
            //            }

            //            CertificateDirectoryState activeNode = new CertificateDirectoryState(passiveNode.Parent);

            //            activeNode.Create(context, passiveNode);
            //            activeNode.QueryServers.OnCall = new QueryServersMethodStateMethodCallHandler(OnQueryServers);
            //            activeNode.QueryApplications.OnCall = new QueryApplicationsMethodStateMethodCallHandler(OnQueryApplications);
            //            activeNode.RegisterApplication.OnCall = new RegisterApplicationMethodStateMethodCallHandler(OnRegisterApplication);
            //            activeNode.UpdateApplication.OnCall = new UpdateApplicationMethodStateMethodCallHandler(OnUpdateApplication);
            //            activeNode.UnregisterApplication.OnCall = new UnregisterApplicationMethodStateMethodCallHandler(OnUnregisterApplication);
            //            activeNode.FindApplications.OnCall = new FindApplicationsMethodStateMethodCallHandler(OnFindApplications);
            //            activeNode.GetApplication.OnCall = new GetApplicationMethodStateMethodCallHandler(OnGetApplication);
            //            activeNode.StartNewKeyPairRequest.OnCall = new StartNewKeyPairRequestMethodStateMethodCallHandler(OnStartNewKeyPairRequest);
            //            activeNode.FinishRequest.OnCall = new FinishRequestMethodStateMethodCallHandler(OnFinishRequest);
            //            activeNode.GetCertificateGroups.OnCall = new GetCertificateGroupsMethodStateMethodCallHandler(OnGetCertificateGroups);
            //            activeNode.GetTrustList.OnCall = new GetTrustListMethodStateMethodCallHandler(OnGetTrustList);
            //            activeNode.GetCertificateStatus.OnCall = new GetCertificateStatusMethodStateMethodCallHandler(OnGetCertificateStatus);
            //            activeNode.StartSigningRequest.OnCall = new StartSigningRequestMethodStateMethodCallHandler(OnStartSigningRequest);
            //            // TODO
            //            //activeNode.RevokeCertificate.OnCall = new RevokeCertificateMethodStateMethodCallHandler(OnRevokeCertificate);

            //            activeNode.CertificateGroups.DefaultApplicationGroup.CertificateTypes.Value = new NodeId[] { global::Opc.Ua.ObjectTypeIds.RsaSha256ApplicationCertificateType };
            //            activeNode.CertificateGroups.DefaultApplicationGroup.TrustList.LastUpdateTime.Value = DateTime.UtcNow;
            //            activeNode.CertificateGroups.DefaultApplicationGroup.TrustList.Writable.Value = false;
            //            activeNode.CertificateGroups.DefaultApplicationGroup.TrustList.UserWritable.Value = false;

            //            activeNode.CertificateGroups.DefaultHttpsGroup.CertificateTypes.Value = new NodeId[] { global::Opc.Ua.ObjectTypeIds.HttpsCertificateType };
            //            activeNode.CertificateGroups.DefaultHttpsGroup.TrustList.LastUpdateTime.Value = DateTime.UtcNow;
            //            activeNode.CertificateGroups.DefaultHttpsGroup.TrustList.Writable.Value = false;
            //            activeNode.CertificateGroups.DefaultHttpsGroup.TrustList.UserWritable.Value = false;

            //            activeNode.CertificateGroups.DefaultUserTokenGroup.CertificateTypes.Value = new NodeId[] { global::Opc.Ua.ObjectTypeIds.UserCredentialCertificateType };
            //            activeNode.CertificateGroups.DefaultUserTokenGroup.TrustList.LastUpdateTime.Value = DateTime.UtcNow;
            //            activeNode.CertificateGroups.DefaultUserTokenGroup.TrustList.Writable.Value = false;
            //            activeNode.CertificateGroups.DefaultUserTokenGroup.TrustList.UserWritable.Value = false;

            //            // replace the node in the parent.
            //            if (passiveNode.Parent != null)
            //            {
            //                passiveNode.Parent.ReplaceChild(context, activeNode);
            //            }

            //            return activeNode;
            //        }
            //}

            //return predefinedNode;
        }
    }
}
