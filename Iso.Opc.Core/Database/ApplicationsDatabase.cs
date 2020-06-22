using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iso.Opc.Core.Models;
using Newtonsoft.Json;
using Opc.Ua;
using Opc.Ua.Gds;
using Opc.Ua.Gds.Server;
using Opc.Ua.Gds.Server.Database;

namespace Iso.Opc.Core.Database
{
    [Serializable]
    public class ApplicationsDatabase : IApplicationsDatabase, ICertificateRequest
    {
        #region Private Fields
        [JsonIgnore]
        private string _databaseFileName;
        #endregion

        #region Constructor
        public ApplicationsDatabase(string filename)
        {
            _databaseFileName = filename;
        }

        #endregion

        #region Internal Fields
        [NonSerialized]
        internal object Lock = new object();
        [NonSerialized]
        internal DateTime queryCounterResetTime = DateTime.UtcNow;
        [JsonIgnore]
        internal string FileName { get { return _databaseFileName; } private set { _databaseFileName = value; } }
        [JsonProperty]
        internal ICollection<Application> Applications = new HashSet<Application>();
        [JsonProperty]
        internal ICollection<ApplicationName> ApplicationNames = new List<ApplicationName>();
        [JsonProperty]
        internal ICollection<ServerEndpoint> ServerEndpoints = new List<ServerEndpoint>();
        [JsonProperty]
        internal ICollection<CertificateRequest> CertificateRequests = new HashSet<CertificateRequest>();
        [JsonProperty]
        internal ICollection<CertificateStore> CertificateStores = new HashSet<CertificateStore>();
        #endregion

        #region Public Members
        /// <summary>
        /// Returns true if the target string matches the UA pattern string. 
        /// The pattern string may include UA wildcards %_\[]!
        /// </summary>
        /// <param name="target">String to check for a pattern match.</param>
        /// <param name="pattern">Pattern to match with the target string.</param>
        /// <returns>true if the target string matches the pattern, otherwise false.</returns>
        public static bool Match(string target, string pattern)
        {
            if (string.IsNullOrEmpty(target))
                return false;
            if (string.IsNullOrEmpty(pattern))
                return true;
            List<string> tokens = Parse(pattern);
            int targetIndex = 0;
            for (int ii = 0; ii < tokens.Count; ii++)
            {
                targetIndex = Match(target, targetIndex, tokens, ref ii);
                if (targetIndex < 0)
                    return false;
            }
            if (targetIndex < target.Length)
                return false;
            return true;
        }
        #endregion

        #region Private Members
        private static int Match(string target, int targetIndex, IList<string> tokens, ref int tokenIndex)
        {
            if (tokens == null || tokenIndex < 0 || tokenIndex >= tokens.Count)
                return -1;
            if (target == null || targetIndex < 0 || targetIndex >= target.Length)
            {
                if (tokens[tokenIndex] == "%" && tokenIndex == tokens.Count - 1)
                    return targetIndex;
                return -1;
            }
            string token = tokens[tokenIndex];
            if (token == "_")
            {
                if (targetIndex >= target.Length)
                    return -1;
                return targetIndex + 1;
            }
            if (token == "%")
                return SkipToNext(target, targetIndex, tokens, ref tokenIndex);
            if (token.StartsWith("[", StringComparison.Ordinal))
            {
                bool inverse = false;
                bool match = false;
                for (int ii = 1; ii < token.Length - 1; ii++)
                {
                    if (token[ii] == '^')
                    {
                        inverse = true;
                        continue;
                    }
                    if (!inverse && target[targetIndex] == token[ii])
                        return targetIndex + 1;
                    match |= (inverse && target[targetIndex] == token[ii]);
                }
                if (inverse && !match)
                    return targetIndex + 1;
                return -1;
            }
            if (target.Substring(targetIndex).StartsWith(token, StringComparison.Ordinal))
                return targetIndex + token.Length;
            return -1;
        }
        private static int SkipToNext(string target, int targetIndex, IList<string> tokens, ref int tokenIndex)
        {
            if (targetIndex >= target.Length - 1)
                return targetIndex + 1;
            if (tokenIndex >= tokens.Count - 1)
                return target.Length + 1;
            if (!tokens[tokenIndex + 1].StartsWith("[^", StringComparison.Ordinal))
            {
                int nextTokenIndex = tokenIndex + 1;
                // skip over unmatched chars.
                while (targetIndex < target.Length && Match(target, targetIndex, tokens, ref nextTokenIndex) < 0)
                {
                    targetIndex++;
                    nextTokenIndex = tokenIndex + 1;
                }
                nextTokenIndex = tokenIndex + 1;
                // skip over duplicate matches.
                while (targetIndex < target.Length && Match(target, targetIndex, tokens, ref nextTokenIndex) >= 0)
                {
                    targetIndex++;
                    nextTokenIndex = tokenIndex + 1;
                }
                // return last match.
                if (targetIndex <= target.Length)
                    return targetIndex - 1;
            }
            else
            {
                int start = targetIndex;
                int nextTokenIndex = tokenIndex + 1;
                // skip over matches.
                while (targetIndex < target.Length && Match(target, targetIndex, tokens, ref nextTokenIndex) >= 0)
                {
                    targetIndex++;
                    nextTokenIndex = tokenIndex + 1;
                }
                // no match in string.
                if (targetIndex < target.Length)
                    return -1;
                // try the next token.
                if (tokenIndex >= tokens.Count - 2)
                    return target.Length + 1;
                tokenIndex++;
                return SkipToNext(target, start, tokens, ref tokenIndex);
            }
            return -1;
        }
        private static List<string> Parse(string pattern)
        {
            List<string> tokens = new List<string>();
            int ii = 0;
            StringBuilder buffer = new StringBuilder();

            while (ii < pattern.Length)
            {
                char ch = pattern[ii];
                if (ch == '\\')
                {
                    ii++;
                    if (ii >= pattern.Length)
                        break;
                    buffer.Append(pattern[ii]);
                    ii++;
                    continue;
                }
                if (ch == '_')
                {
                    if (buffer.Length > 0)
                    {
                        tokens.Add(buffer.ToString());
                        buffer.Length = 0;
                    }
                    tokens.Add("_");
                    ii++;
                    continue;
                }

                if (ch == '%')
                {
                    if (buffer.Length > 0)
                    {
                        tokens.Add(buffer.ToString());
                        buffer.Length = 0;
                    }
                    tokens.Add("%");
                    ii++;
                    while (ii < pattern.Length && pattern[ii] == '%')
                    {
                        ii++;
                    }
                    continue;
                }

                if (ch == '[')
                {
                    if (buffer.Length > 0)
                    {
                        tokens.Add(buffer.ToString());
                        buffer.Length = 0;
                    }
                    buffer.Append(ch);
                    ii++;
                    int start = 0;
                    int end = 0;
                    while (ii < pattern.Length && pattern[ii] != ']')
                    {
                        if (pattern[ii] == '-' && ii > 0 && ii < pattern.Length - 1)
                        {
                            start = Convert.ToInt32(pattern[ii - 1]) + 1;
                            end = Convert.ToInt32(pattern[ii + 1]);
                            while (start < end)
                            {
                                buffer.Append(Convert.ToChar(start));
                                start++;
                            }
                            buffer.Append(Convert.ToChar(end));
                            ii += 2;
                            continue;
                        }
                        buffer.Append(pattern[ii]);
                        ii++;
                    }
                    buffer.Append("]");
                    tokens.Add(buffer.ToString());
                    buffer.Length = 0;
                    ii++;
                    continue;
                }
                buffer.Append(ch);
                ii++;
            }
            if (buffer.Length > 0)
            {
                tokens.Add(buffer.ToString());
                buffer.Length = 0;
            }
            return tokens;
        }
        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(_databaseFileName, json);
        }
        public static ApplicationsDatabase Load(string fileName)
        {
            try
            {
                string json = File.ReadAllText(fileName);
                ApplicationsDatabase db = JsonConvert.DeserializeObject<ApplicationsDatabase>(json);
                if(db == null)
                    return new ApplicationsDatabase(fileName);
                db.FileName = fileName;
                return db;
            }
            catch
            {
                return new ApplicationsDatabase(fileName);
            }
        }
        private void SaveChanges()
        {
            lock (Lock)
            {
                queryCounterResetTime = DateTime.UtcNow;
                // assign IDs to new apps
                var queryNewApps = from x in Applications
                                   where x.ID == 0
                                   select x;
                if (Applications.Count > 0)
                {
                    uint appMax = Applications.Max(a => a.ID);
                    foreach (var application in queryNewApps)
                    {
                        appMax++;
                        application.ID = appMax;
                    }
                }
                Save();
            }
        }
        #endregion

        #region ICertificateRequest Properties
        public ushort NamespaceIndex { get; set; }
        #endregion

        #region ICertificateRequest
        public void AcceptRequest(NodeId requestId, byte[] certificate)
        {
            Guid id = (Guid)requestId.Identifier;
            lock (Lock)
            {
                CertificateRequest request = (from x in CertificateRequests where x.RequestId == id select x).SingleOrDefault();
                if (request == null)
                    throw new ServiceResultException(StatusCodes.BadNodeIdUnknown);
                request.State = (int)CertificateRequestState.Accepted;
                // save certificate for audit trail
                request.Certificate = certificate;
                // erase information which is ot required anymore
                request.CertificateSigningRequest = null;
                request.PrivateKeyPassword = null;
                SaveChanges();
            }
        }
        public void ApproveRequest(NodeId requestId, bool isRejected)
        {
            Guid id = (Guid)requestId.Identifier;
            lock (Lock)
            {
                CertificateRequest request = (from x in CertificateRequests where x.RequestId == id select x).SingleOrDefault();
                if (request == null)
                    throw new ServiceResultException(StatusCodes.BadNodeIdUnknown);
                if (isRejected)
                {
                    request.State = (int)CertificateRequestState.Rejected;
                    // erase information which is ot required anymore
                    request.CertificateSigningRequest = null;
                    request.PrivateKeyPassword = null;
                }
                else
                {
                    request.State = (int)CertificateRequestState.Approved;
                }
                SaveChanges();
            }
        }
        public CertificateRequestState FinishRequest(NodeId applicationId, NodeId requestId, out string certificateGroupId, out string certificateTypeId, out byte[] signedCertificate, out byte[] privateKey)
        {
            certificateGroupId = null;
            certificateTypeId = null;
            signedCertificate = null;
            privateKey = null;
            Guid reqId = (Guid)requestId.Identifier;
            Guid appId = (Guid)applicationId.Identifier;
            lock (Lock)
            {
                CertificateRequest request = (from x in CertificateRequests where x.RequestId == reqId select x).SingleOrDefault();
                if (request == null)
                    throw new ServiceResultException(StatusCodes.BadInvalidArgument);
                switch (request.State)
                {
                    case (int)CertificateRequestState.New:
                        return CertificateRequestState.New;
                    case (int)CertificateRequestState.Rejected:
                        return CertificateRequestState.Rejected;
                    case (int)CertificateRequestState.Accepted:
                        return CertificateRequestState.Accepted;
                    case (int)CertificateRequestState.Approved:
                        break;
                    default:
                        throw new ServiceResultException(StatusCodes.BadInvalidArgument);
                }
                certificateGroupId = request.CertificateGroupId;
                certificateTypeId = request.CertificateTypeId;
                return CertificateRequestState.Approved;
            }
        }
        public void Initialize()
        {
            throw new NotImplementedException();
        }
        public CertificateRequestState ReadRequest(NodeId applicationId, NodeId requestId, out string certificateGroupId, out string certificateTypeId, out byte[] certificateRequest, out string subjectName, out string[] domainNames, out string privateKeyFormat, out string privateKeyPassword)
        {
            certificateGroupId = null;
            certificateTypeId = null;
            certificateRequest = null;
            subjectName = null;
            domainNames = null;
            privateKeyFormat = null;
            privateKeyPassword = null;
            Guid reqId = (Guid)requestId.Identifier;
            Guid appId = (Guid)applicationId.Identifier;
            lock (Lock)
            {
                CertificateRequest request = (from x in CertificateRequests where x.RequestId == reqId select x).SingleOrDefault();
                if (request == null)
                    throw new ServiceResultException(StatusCodes.BadInvalidArgument);
                switch (request.State)
                {
                    case (int)CertificateRequestState.New:
                        return CertificateRequestState.New;
                    case (int)CertificateRequestState.Rejected:
                        return CertificateRequestState.Rejected;
                    case (int)CertificateRequestState.Accepted:
                        return CertificateRequestState.Accepted;
                    case (int)CertificateRequestState.Approved:
                        break;
                    default:
                        throw new ServiceResultException(StatusCodes.BadInvalidArgument);
                }
                certificateGroupId = request.CertificateGroupId;
                certificateTypeId = request.CertificateTypeId;
                certificateRequest = request.CertificateSigningRequest;
                subjectName = request.SubjectName;
                domainNames = request.DomainNames;
                privateKeyFormat = request.PrivateKeyFormat;
                privateKeyPassword = request.PrivateKeyPassword;
                return CertificateRequestState.Approved;
            }
        }
        public NodeId StartNewKeyPairRequest(NodeId applicationId, string certificateGroupId, string certificateTypeId, string subjectName, string[] domainNames, string privateKeyFormat, string privateKeyPassword, string authorityId)
        {
            Guid id = (Guid)applicationId.Identifier;
            lock (Lock)
            {
                Application application = (from x in Applications where x.ApplicationId == id select x).SingleOrDefault();
                if (application == null)
                    throw new ServiceResultException(StatusCodes.BadNodeIdUnknown);
                CertificateRequest request = (from x in CertificateRequests where x.AuthorityId == authorityId && x.ApplicationId == id select x).SingleOrDefault();
                bool isNew = false;
                if (request == null)
                {
                    request = new CertificateRequest()
                    {
                        RequestId = Guid.NewGuid(),
                        AuthorityId = authorityId
                    };
                    isNew = true;
                }
                request.State = (int)CertificateRequestState.New;
                request.CertificateGroupId = certificateGroupId;
                request.CertificateTypeId = certificateTypeId;
                request.SubjectName = subjectName;
                request.DomainNames = domainNames;
                request.PrivateKeyFormat = privateKeyFormat;
                request.PrivateKeyPassword = privateKeyPassword;
                request.CertificateSigningRequest = null;
                request.ApplicationId = id;
                if (isNew)
                    CertificateRequests.Add(request);
                SaveChanges();
                return new NodeId(request.RequestId, NamespaceIndex);
            }
        }
        public NodeId StartSigningRequest(NodeId applicationId, string certificateGroupId, string certificateTypeId, byte[] certificateRequest, string authorityId)
        {
            Guid id = (Guid)applicationId.Identifier;
            lock (Lock)
            {
                Application application = (from x in Applications where x.ApplicationId == id select x).SingleOrDefault();
                if (application == null)
                    throw new ServiceResultException(StatusCodes.BadNodeIdUnknown);
                CertificateRequest request = (from x in CertificateRequests where x.AuthorityId == authorityId && x.ApplicationId == id select x).SingleOrDefault();
                bool isNew = false;
                if (request == null)
                {
                    request = new CertificateRequest() { RequestId = Guid.NewGuid(), AuthorityId = authorityId };
                    isNew = true;
                }
                request.State = (int)CertificateRequestState.New;
                request.CertificateGroupId = certificateGroupId;
                request.CertificateTypeId = certificateTypeId;
                request.SubjectName = null;
                request.DomainNames = null;
                request.PrivateKeyFormat = null;
                request.PrivateKeyPassword = null;
                request.CertificateSigningRequest = certificateRequest;
                request.ApplicationId = id;
                if (isNew)
                    CertificateRequests.Add(request);
                SaveChanges();
                return new NodeId(request.RequestId, NamespaceIndex);
            }
        }
        #endregion

        #region IApplicationsDatabase
        public NodeId RegisterApplication(ApplicationRecordDataType application)
        {
            NodeId appNodeId = application.ApplicationId;
            if (NodeId.IsNull(appNodeId))
            {
                appNodeId = new NodeId(Guid.NewGuid(), NamespaceIndex);
            }
            Guid applicationId = (Guid)appNodeId.Identifier;
            StringBuilder capabilities = new StringBuilder();
            application.ServerCapabilities.Sort();
            foreach (string capability in application.ServerCapabilities)
            {
                if (string.IsNullOrEmpty(capability))
                    continue;
                if (capabilities.Length > 0)
                    capabilities.Append(',');
                capabilities.Append(capability);
            }
            lock (Lock)
            {
                Application record = null;
                if (applicationId != Guid.Empty)
                {
                    IEnumerable<Application> results = from x in Applications
                                                       where x.ApplicationId == applicationId
                                                       select x;
                    record = results.SingleOrDefault();
                    if (record != null)
                    {
                        List<ServerEndpoint> endpoints = (from ii in ServerEndpoints
                                                          where ii.ApplicationId == record.ApplicationId
                                                          select ii).ToList();
                        foreach (var endpoint in endpoints)
                        {
                            ServerEndpoints.Remove(endpoint);
                        }
                        List<ApplicationName> names = (from ii in ApplicationNames
                                     where ii.ApplicationId == record.ApplicationId
                                     select ii).ToList();
                        foreach (var name in names)
                        {
                            ApplicationNames.Remove(name);
                        }
                        SaveChanges();
                    }
                }
                bool isNew = false;
                if (record == null)
                {
                    applicationId = Guid.NewGuid();
                    record = new Application()
                    {
                        ApplicationId = applicationId,
                        ID = 0
                    };
                    isNew = true;
                }
                record.ApplicationUri = application.ApplicationUri;
                record.ApplicationName = application.ApplicationNames[0].Text;
                record.ApplicationType = (int)application.ApplicationType;
                record.ProductUri = application.ProductUri;
                record.ServerCapabilities = capabilities.ToString();
                if (isNew)
                {
                    Applications.Add(record);
                }
                SaveChanges();
                if (application.DiscoveryUrls != null)
                {
                    foreach (var discoveryUrl in application.DiscoveryUrls)
                    {
                        ServerEndpoints.Add(new ServerEndpoint()
                        {
                            ApplicationId = record.ApplicationId,
                            DiscoveryUrl = discoveryUrl
                        });
                    }
                }
                if (application.ApplicationNames != null && application.ApplicationNames.Count > 0)
                {
                    foreach (var applicationName in application.ApplicationNames)
                    {
                        ApplicationNames.Add(new ApplicationName()
                        {
                            ApplicationId = record.ApplicationId,
                            Locale = applicationName.Locale,
                            Text = applicationName.Text
                        });
                    }
                }
                SaveChanges();
                return new NodeId(applicationId, NamespaceIndex);
            }
        }
        public void UnregisterApplication(NodeId applicationId)
        {
            Guid id = (Guid)applicationId.Identifier;
            List<byte[]> certificates = new List<byte[]>();
            lock (Lock)
            {
                Application application = (from ii in Applications
                                           where ii.ApplicationId == id
                                           select ii).SingleOrDefault();
                if (application == null)
                    throw new ArgumentException("A record with the specified application id does not exist.", nameof(applicationId));
                IEnumerable<CertificateRequest> certificateRequests =
                    from ii in CertificateRequests
                    where ii.ApplicationId == id
                    select ii;
                foreach (CertificateRequest entry in new List<CertificateRequest>(certificateRequests))
                {
                    CertificateRequests.Remove(entry);
                }
                IEnumerable<ApplicationName> applicationNames =
                    from ii in ApplicationNames
                    where ii.ApplicationId == id
                    select ii;
                foreach (ApplicationName entry in new List<ApplicationName>(applicationNames))
                {
                    ApplicationNames.Remove(entry);
                }
                IEnumerable<ServerEndpoint> serverEndpoints =
                    from ii in ServerEndpoints
                    where ii.ApplicationId == id
                    select ii;
                foreach (ServerEndpoint entry in new List<ServerEndpoint>(serverEndpoints))
                {
                    ServerEndpoints.Remove(entry);
                }
                Applications.Remove(application);
                SaveChanges();
            }
        }
        public ApplicationRecordDataType GetApplication(NodeId applicationId)
        {
            Guid id = (Guid)applicationId.Identifier;
            lock (Lock)
            {
                IEnumerable<Application> results = from x in Applications
                                                   where x.ApplicationId == id
                                                   select x;
                Application result = results.SingleOrDefault();
                if (result == null)
                    return null;
                IEnumerable<ApplicationName> applicationNames = from ii in ApplicationNames
                                                                where ii.ApplicationId == id
                                                                select ii;
                List<LocalizedText> names = new List<LocalizedText>();
                foreach (ApplicationName applicationName in applicationNames)
                {
                    names.Add(new LocalizedText(applicationName.Locale, applicationName.Text));
                }
                StringCollection discoveryUrls = null;
                IEnumerable<ServerEndpoint> endpoints = from ii in ServerEndpoints
                                                        where ii.ApplicationId == result.ApplicationId
                                                        select ii;
                if (endpoints != null)
                {
                    discoveryUrls = new StringCollection();
                    foreach (ServerEndpoint endpoint in endpoints)
                    {
                        discoveryUrls.Add(endpoint.DiscoveryUrl);
                    }
                }
                StringCollection capabilities = new StringCollection();
                if (!string.IsNullOrWhiteSpace(result.ServerCapabilities))
                {
                    capabilities.AddRange(result.ServerCapabilities.Split(','));
                }
                return new ApplicationRecordDataType()
                {
                    ApplicationId = new NodeId(result.ApplicationId, NamespaceIndex),
                    ApplicationUri = result.ApplicationUri,
                    ApplicationType = (ApplicationType)result.ApplicationType,
                    ApplicationNames = new LocalizedTextCollection(names),
                    ProductUri = result.ProductUri,
                    DiscoveryUrls = discoveryUrls,
                    ServerCapabilities = capabilities
                };
            }
        }
        public ApplicationRecordDataType[] FindApplications(string applicationUri)
        {
            lock (Lock)
            {
                IEnumerable<Application> results = from x in Applications
                                                   where x.ApplicationUri == applicationUri
                                                   select x;
                List<ApplicationRecordDataType> records = new List<ApplicationRecordDataType>();
                foreach (Application result in results)
                {
                    LocalizedText[] names = null;
                    if (result.ApplicationName != null)
                    {
                        names = new LocalizedText[] { result.ApplicationName };
                    }
                    StringCollection discoveryUrls = null;
                    IEnumerable<ServerEndpoint> endpoints = from ii in ServerEndpoints
                                    where ii.ApplicationId == result.ApplicationId
                                    select ii;
                    if (endpoints != null)
                    {
                        discoveryUrls = new StringCollection();
                        foreach (var endpoint in endpoints)
                        {
                            discoveryUrls.Add(endpoint.DiscoveryUrl);
                        }
                    }
                    string[] capabilities = null;
                    if (result.ServerCapabilities != null)
                    {
                        capabilities = result.ServerCapabilities.Split(',');
                    }
                    records.Add(new ApplicationRecordDataType()
                    {
                        ApplicationId = new NodeId(result.ApplicationId, NamespaceIndex),
                        ApplicationUri = result.ApplicationUri,
                        ApplicationType = (ApplicationType)result.ApplicationType,
                        ApplicationNames = new LocalizedTextCollection(names),
                        ProductUri = result.ProductUri,
                        DiscoveryUrls = discoveryUrls,
                        ServerCapabilities = capabilities
                    });
                }
                return records.ToArray();
            }
        }
        public ServerOnNetwork[] QueryServers(uint startingRecordId, uint maxRecordsToReturn, string applicationName, string applicationUri, string productUri, string[] serverCapabilities, out DateTime lastCounterResetTime)
        {
            lock (Lock)
            {
                lastCounterResetTime = queryCounterResetTime;
                var results = from x in ServerEndpoints
                              join y in Applications on x.ApplicationId equals y.ApplicationId
                              where y.ID >= startingRecordId
                              orderby y.ID
                              select new
                              {
                                  y.ID,
                                  y.ApplicationName,
                                  y.ApplicationUri,
                                  y.ProductUri,
                                  x.DiscoveryUrl,
                                  y.ServerCapabilities
                              };

                List<ServerOnNetwork> records = new List<ServerOnNetwork>();
                uint lastID = 0;
                foreach (var result in results)
                {
                    if (!string.IsNullOrEmpty(applicationName))
                    {
                        if (!Match(result.ApplicationName, applicationName))
                            continue;
                    }
                    if (!string.IsNullOrEmpty(applicationUri))
                    {
                        if (!Match(result.ApplicationUri, applicationUri))
                            continue;
                    }
                    if (!string.IsNullOrEmpty(productUri))
                    {
                        if (!Match(result.ProductUri, productUri))
                            continue;
                    }
                    string[] capabilities = null;
                    if (!string.IsNullOrEmpty(result.ServerCapabilities))
                    {
                        capabilities = result.ServerCapabilities.Split(',');
                    }
                    if (serverCapabilities != null && serverCapabilities.Length > 0)
                    {
                        bool match = true;
                        for (int ii = 0; ii < serverCapabilities.Length; ii++)
                        {
                            if (capabilities == null || !capabilities.Contains(serverCapabilities[ii]))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match)
                            continue;
                    }
                    if (lastID != 0)
                    {
                        if (maxRecordsToReturn != 0 && lastID != result.ID && records.Count >= maxRecordsToReturn)
                            break;
                    }
                    lastID = result.ID;
                    records.Add(new ServerOnNetwork()
                    {
                        RecordId = result.ID,
                        ServerName = result.ApplicationName,
                        DiscoveryUrl = result.DiscoveryUrl,
                        ServerCapabilities = capabilities
                    });
                }
                return records.ToArray();
            }
        }
        public bool SetApplicationCertificate(NodeId applicationId, string certificateTypeId, byte[] certificate)
        {
            Guid id = (Guid)applicationId.Identifier;
            lock (Lock)
            {
                IEnumerable<Application> results = from x in Applications
                                                   where x.ApplicationId == id
                                                   select x;
                Application result = results.SingleOrDefault();
                if (result == null)
                    return false;
                result.Certificate[certificateTypeId] = certificate;
                SaveChanges();
            }

            return true;
        }
        public bool GetApplicationCertificate(NodeId applicationId, string certificateTypeId, out byte[] certificate)
        {
            certificate = null;
            Guid id = (Guid)applicationId.Identifier;
            List<byte[]> certificates = new List<byte[]>();
            lock (Lock)
            {
                Application application = (from ii in Applications
                                           where ii.ApplicationId == id
                                           select ii).SingleOrDefault();
                if (application == null)
                    throw new ArgumentException("A record with the specified application id does not exist.", nameof(applicationId));
                if (!application.Certificate.TryGetValue(certificateTypeId, out certificate))
                    return false;
            }
            return true;
        }
        public bool SetApplicationTrustLists(NodeId applicationId, string certificateTypeId, string trustListId)
        {
            Guid id = (Guid)applicationId.Identifier;
            lock (Lock)
            {
                Application result = (from x in Applications where x.ApplicationId == id select x).SingleOrDefault();
                if (result == null)
                    return false;
                if (trustListId != null)
                {
                    CertificateStore result2 = (from x in CertificateStores where x.Path == trustListId select x).SingleOrDefault();
                    if (result2 != null)
                        result.TrustListId[certificateTypeId] = result2.TrustListId;
                }
                SaveChanges();
            }
            return true;
        }
        public bool GetApplicationTrustLists(NodeId applicationId, string certificateTypeId, out string trustListId)
        {
            trustListId = null;
            Guid id = (Guid)applicationId.Identifier;
            lock (Lock)
            {
                Application result = (from x in Applications where x.ApplicationId == id select x).SingleOrDefault();
                if (result == null)
                    return false;
                Guid trustListGuid;
                if (result.TrustListId.TryGetValue(certificateTypeId, out trustListGuid))
                {
                    CertificateStore result2 = (from x in CertificateStores where x.TrustListId == trustListGuid select x).SingleOrDefault();
                    if (result2 != null)
                    {
                        trustListId = result2.Path;
                        return true;
                    }
                }
            }
            return false;
        }
        public ApplicationDescription[] QueryApplications(uint startingRecordId, uint maxRecordsToReturn, string applicationName, string applicationUri, uint applicationType, string productUri, string[] serverCapabilities, out DateTime lastCounterResetTime, out uint nextRecordId)
        {
            lastCounterResetTime = DateTime.MinValue;
            nextRecordId = 0;
            List<ApplicationDescription> records = new List<ApplicationDescription>();
            lock (Lock)
            {
                IEnumerable<Application> results = from x in Applications
                                                   where ((int)startingRecordId == 0 || (int)startingRecordId <= x.ID)
                                                   select x;
                lastCounterResetTime = queryCounterResetTime;
                uint lastID = 0;
                foreach (Application result in results)
                {
                    if (!string.IsNullOrEmpty(applicationName))
                    {
                        if (!Match(result.ApplicationName, applicationName))
                            continue;
                    }
                    if (!string.IsNullOrEmpty(applicationUri))
                    {
                        if (!Match(result.ApplicationUri, applicationUri))
                            continue;
                    }
                    if (!string.IsNullOrEmpty(productUri))
                    {
                        if (!Match(result.ProductUri, productUri))
                            continue;
                    }
                    string[] capabilities = null;
                    if (!string.IsNullOrEmpty(result.ServerCapabilities))
                    {
                        capabilities = result.ServerCapabilities.Split(',');
                    }
                    if (serverCapabilities != null && serverCapabilities.Length > 0)
                    {
                        bool match = true;
                        for (int ii = 0; ii < serverCapabilities.Length; ii++)
                        {
                            if (capabilities == null || !capabilities.Contains(serverCapabilities[ii]))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match)
                            continue;
                    }

                    // type filter, 0 and 3 returns all
                    // filter for servers
                    if (applicationType == 1 &&
                        result.ApplicationType == (int)ApplicationType.Client)
                    {
                        continue;
                    }
                    else // filter for clients
                    if (applicationType == 2 &&
                        result.ApplicationType != (int)ApplicationType.Client &&
                        result.ApplicationType != (int)ApplicationType.ClientAndServer)
                    {
                        continue;
                    }

                    var endpoints = from ii in ServerEndpoints
                                    where ii.ApplicationId == result.ApplicationId
                                    select ii;

                    var discoveryUrls = new StringCollection();
                    if (endpoints != null)
                    {

                        foreach (var endpoint in endpoints)
                        {
                            discoveryUrls.Add(endpoint.DiscoveryUrl);
                        }
                    }

                    if (lastID == 0)
                    {
                        lastID = result.ID;
                    }
                    else
                    {
                        if (maxRecordsToReturn != 0 &&
                            records.Count >= maxRecordsToReturn)
                        {
                            break;
                        }

                        lastID = result.ID;
                    }

                    records.Add(new ApplicationDescription()
                    {
                        ApplicationUri = result.ApplicationUri,
                        ProductUri = result.ProductUri,
                        ApplicationName = result.ApplicationName,
                        ApplicationType = (ApplicationType)result.ApplicationType,
                        GatewayServerUri = null,
                        DiscoveryProfileUri = null,
                        DiscoveryUrls = discoveryUrls
                    });
                    nextRecordId = (uint)lastID + 1;

                }
                return records.ToArray();
            }
        }
        #endregion
    }
}
