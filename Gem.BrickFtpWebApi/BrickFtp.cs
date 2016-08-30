using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Gem.BrickFtpWebApi.Model;
using log4net;
using Newtonsoft.Json;

namespace Gem.BrickFtpWebApi
{
    public static class BrickFtp
    {
        private static ILog log = LogManager.GetLogger(typeof (BrickFtp));
        public const string Protocol = "https://";
        public const string SessionCookieName = "BrickAPI";

        public static Session Login(string domain, string username, string password)
        {
            string basePath = Protocol + domain;
            var operationPath = "/api/rest/v1/sessions.json";
            var headerParams = new Dictionary<string, string>();
            var queryParams = new Dictionary<string, object>();
            var postParams = new Dictionary<string, object>();

            // verify required params are set
            if ((object)username == null || (object)password == null)
            {
                throw new BrickFtpException(400, "missing required params");
            }
            postParams.Add("username", username);
            postParams.Add("password", password);
            try
            {
                string response = Invoke(basePath, operationPath, "POST", queryParams, postParams, headerParams);
                if (response != null)
                {
                    var sessionId = (SessionId)Deserialize(response, typeof(SessionId));
                    return new Session(domain, username, password, sessionId);
                }
                return null;
            }
            catch (Exception ex)
            {
                string msg = "Failed to login user: " + username + " to domain: " + domain;
                log.Warn(msg, ex);
                // Log and swallow exception, give general error exception back for failing authentication.
                throw new BrickFtpException(403, "Failed to login user: " + username + " to domain: " + domain+". Make sure that username and password is correct.");
            }
        }

        //GET /folders/
        public static List<FtpItem> ListFolder(Session session, string path)
        {
            if(session == null) throw  new ArgumentException("Session cannot be null, please login to get a valid session object.");

            path = path.TrimStart('/');
            string basePath = Protocol + session.Domain;
            var operationPath = "/api/rest/v1/folders/"+session.Username+"/"+path;
            operationPath = operationPath.TrimEnd('/');
            var headerParams = new Dictionary<String, String>();
            var queryParams = new Dictionary<String, object>();
            var postParams = new Dictionary<String, object>();

            try
            {
                string response = Invoke(basePath, operationPath, "GET", queryParams, postParams, headerParams, sessionCookie:session.Cookie);
                if (response != null)
                {
                    var folders = (List<FtpItem>)Deserialize(response, typeof(List<FtpItem>));
                    return folders;
                }
            }
            catch (BrickFtpException ex)
            {
                if (ex.ErrorCode == 404)
                {
                    return new List<FtpItem>();
                }
                throw;
            }

            return new List<FtpItem>();
        }

        //GET /folders/
        public static List<FtpItem> ListFolderWithConstraints(Session session, string path, int entriesPerPage = 1000, int pageNumber = 1, FileSortType sortType = FileSortType.None, string searchText = "")
        {
            if (session == null) throw new ArgumentException("Session cannot be null, please login to get a valid session object.");

            path = path.TrimStart('/');
            string basePath = Protocol + session.Domain;
            var operationPath = "/api/rest/v1/folders/" + session.Username + "/" + path;
            operationPath = operationPath.TrimEnd('/');
            var headerParams = new Dictionary<String, String>();
            var queryParams = new Dictionary<String, object>();
            var postParams = new Dictionary<String, object>();

            //var operationPath = "/api/rest/v1/folders/testaq/Testtt?&per_page=1";
            //var operationPath = "/api/rest/v1/folders/testaq/Testtt?&sort_by[modified_at_datetime]=desc";
            //var operationPath = "/api/rest/v1/folders/testaq/Testtt?&search=201606030";

            operationPath += "?&per_page=" + entriesPerPage+"&page="+pageNumber;
            if (sortType == FileSortType.SortByPathAsc) operationPath += "&sort_by[path] = asc";
            else if (sortType == FileSortType.SortByPathDesc) operationPath += "&sort_by[path] = desc";
            else if (sortType == FileSortType.SortBySizeAsc) operationPath += "&sort_by[size] = asc";
            else if (sortType == FileSortType.SortBySizeDesc) operationPath += "&sort_by[size] = desc";
            else if (sortType == FileSortType.SortByTimeModifiedAsc) operationPath += "&sort_by[modified_at_datetime] = asc";
            else if (sortType == FileSortType.SortByTimeModifiedDesc) operationPath += "&sort_by[modified_at_datetime] = desc";

            if (!string.IsNullOrWhiteSpace(searchText)) operationPath += "&search=" + searchText;

            try
            {
                string response = Invoke(basePath, operationPath, "GET", queryParams, postParams, headerParams, sessionCookie: session.Cookie);
                if (response != null)
                {
                    var items = (List<FtpItem>)Deserialize(response, typeof(List<FtpItem>));
                    return items;
                }
            }
            catch (BrickFtpException ex)
            {
                if (ex.ErrorCode == 404)
                {
                    return new List<FtpItem>();
                }
                throw;
            }
            return new List<FtpItem>();
        }

        //POST /folders/:path_and_folder_name
        public static void CreateFolder(Session session, string pathToNewFolder)
        {
            if (session == null) throw new ArgumentException("Session cannot be null, please login to get a valid session object.");

            pathToNewFolder = pathToNewFolder.TrimStart('/');
            string basePath = Protocol + session.Domain;
            var operationPath = "/api/rest/v1/folders/" + session.Username + "/" + pathToNewFolder;
            var headerParams = new Dictionary<String, String>();
            var queryParams = new Dictionary<String, object>();
            var postParams = new Dictionary<String, object>();
            
            try
            {
                string response = Invoke(basePath, operationPath, "POST", queryParams, postParams, headerParams, sessionCookie: session.Cookie);
                if (log.IsDebugEnabled) log.Debug("CREATED (POST) " + operationPath + " Expected []: " + response);
            }
            catch (Exception ex)
            {
                log.Warn(string.Format("User: {0}, failed to create folder: {1} at server: {2}. See stack trace.",session.Username, pathToNewFolder, session.Domain), ex);
                throw;
            }
        }

        public static void DeleteFolder(Session session, string pathToFolder, bool deleteRecursively=true)
        {
            if (session == null) throw new ArgumentException("Session cannot be null, please login to get a valid session object.");

            pathToFolder = pathToFolder.TrimStart('/');
            string basePath = Protocol + session.Domain;
            var operationPath = "/api/rest/v1/files/" + session.Username + "/" + pathToFolder;
            var headerParams = new Dictionary<String, String>();
            if(deleteRecursively) headerParams.Add("Depth", "infinity");
            var queryParams = new Dictionary<String, object>();
            var postParams = new Dictionary<String, object>();

            try
            {
                string response = Invoke(basePath, operationPath, "DELETE", queryParams, postParams, headerParams, sessionCookie: session.Cookie);
                if (log.IsDebugEnabled) log.Debug("DELETE " + operationPath + " Expected []: " + response);
            }
            catch (BrickFtpException ex)
            {
                if (ex.ErrorCode == 404)
                {
                    return;
                }
                throw;
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ProtocolError && !deleteRecursively)
                {
                    throw new BrickFtpException(412, "Cannot delete a folder which is not empty unless the recursively flag is set to true.");
                }
                throw;
            }
        }

        //GET /files/:path_and_filename
        public static List<FtpItem> GetFolderStats(Session session, string path)
        {   
            if (session == null) throw new ArgumentException("Session cannot be null, please login to get a valid session object.");

            path = path.TrimStart('/');
            string basePath = Protocol + session.Domain;
            var operationPath = "/api/rest/v1/folders/" + session.Username + "/" + path;
            var headerParams = new Dictionary<String, String>();
            var queryParams = new Dictionary<String, object>();
            var postParams = new Dictionary<String, object>();
            operationPath = operationPath.TrimEnd('/');
            operationPath +="?action=stat";
            
            try
            {
                string response = Invoke(basePath, operationPath, "GET", queryParams, postParams, headerParams, sessionCookie: session.Cookie);
                return (List<FtpItem>)Deserialize(response, typeof(List<FtpItem>));
            }
            catch (BrickFtpException ex)
            {
                if (ex.ErrorCode == 404)
                {
                    return new List<FtpItem>();
                }
                throw;
            }
        }

        //GET /files/:path_and_filename
        public static FtpFileContent GetFile(Session session, string pathToFile)
        {   
            if (session == null) throw new ArgumentException("Session cannot be null, please login to get a valid session object.");

            pathToFile = pathToFile.TrimStart('/');
            string basePath = Protocol + session.Domain;
            var operationPath = "/api/rest/v1/files/" + session.Username + "/" + pathToFile;
            operationPath = operationPath.TrimEnd('/');
            var headerParams = new Dictionary<String, String>();
            var queryParams = new Dictionary<String, object>();
            var postParams = new Dictionary<String, object>();

            try
            {
                string response = Invoke(basePath, operationPath, "GET", queryParams, postParams, headerParams, sessionCookie:session.Cookie);
                if (response != null)
                {

                    var ftpItem = (FtpDownloadItem)BrickFtp.Deserialize(response, typeof(FtpDownloadItem));
                    using (var client = new HttpClient())
                    {
                        // HTTP GET
                        Task<byte[]> r = client.GetByteArrayAsync(ftpItem.download_uri);
                        r.Wait();
                        if (r.Status == TaskStatus.RanToCompletion)
                        {
                            byte[] array = r.Result;
                            //var str = System.Text.Encoding.Default.GetString(array);
                            var fileContent = new FtpFileContent(ftpItem, array);
                            return fileContent;
                        }
                    }
                }
            }
            catch (BrickFtpException ex)
            {
                if (ex.ErrorCode == 404)
                {
                    return null;
                }
                throw;
            }

            return null;
        }

        public static void DeleteFile(Session session, string pathToFile)
        {
            if (session == null) throw new ArgumentException("Session cannot be null, please login to get a valid session object.");

            pathToFile = pathToFile.TrimStart('/');
            string basePath = Protocol + session.Domain;
            var operationPath = "/api/rest/v1/files/" + session.Username + "/" + pathToFile;
            var headerParams = new Dictionary<String, String>();
            var queryParams = new Dictionary<String, object>();
            var postParams = new Dictionary<String, object>();

            try
            {
                string response = Invoke(basePath, operationPath, "DELETE", queryParams, postParams, headerParams, sessionCookie: session.Cookie);
                if(log.IsDebugEnabled) log.Debug("DELETE "+operationPath+" Expected []: "+ response);
            }
            catch (BrickFtpException ex)
            {
                if (ex.ErrorCode == 404)
                {
                    return;
                }
                throw;
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ProtocolError/* && !deleteRecursively*/)
                {
                    throw new BrickFtpException(412, "Cannot delete a folder which is not empty unless the recursively flag is set to true.");
                }
                throw;
            }
        }

        //POST /files/:path_and_filename
        public static void UploadFile(Session session, FileInfo localFile, string remotePathToFile)
        {
            if (session == null) throw new ArgumentException("Session cannot be null, please login to get a valid session object.");
            if (localFile == null) throw new ArgumentException("LocalFile cannot be null.");
            if (!localFile.Exists) throw new ArgumentException("LocalFile must be a valid file on disk.");
            if (string.IsNullOrWhiteSpace(remotePathToFile)) throw new ArgumentException("The remote path to file cannot be empty or not specified.");

            remotePathToFile = remotePathToFile.TrimStart('/');
            string basePath = Protocol + session.Domain;
            var operationPath = "/api/rest/v1/files/" + session.Username + "/" + remotePathToFile;
            operationPath = operationPath.TrimEnd('/');
            var headerParams = new Dictionary<String, String>();
            var queryParams = new Dictionary<String, object>();
            var postParams = new Dictionary<String, object>();

            postParams.Add("action", "put");
            try
            {
                string response = Invoke(basePath, operationPath, "POST", queryParams, postParams, headerParams, sessionCookie: session.Cookie);
                if (response != null)
                {
                    var ui = (UploadItem)Deserialize(response, typeof(UploadItem));
                    using (var stream = localFile.OpenRead())
                    {
                        var responseMessage = Upload(stream, ui);
                        responseMessage.Wait(TimeSpan.FromSeconds(30));
                        if (responseMessage.Status == TaskStatus.RanToCompletion && responseMessage.Result.StatusCode == HttpStatusCode.OK)
                        {
                            postParams["action"] = "end";
                            postParams.Add("ref", ui.@ref);
                            response = Invoke(basePath, operationPath, "POST", queryParams, postParams, headerParams, sessionCookie: session.Cookie);
                            if (response != null)
                            {
                                Console.Out.WriteLine("End Resp: " + response);
                            }
                        }
                        else
                        {
                            if (responseMessage.Result != null) throw new BrickFtpException((int)responseMessage.Result.StatusCode, "Failed to upload file to: ["+operationPath+"] Task.Result: "+responseMessage.Result.ReasonPhrase);
                            if (responseMessage.Exception != null) throw new Exception("Failed to upload file to: [" + operationPath + "] Task.Message: " + responseMessage.Exception.Message + ". Task.Stack: " + responseMessage.Exception.StackTrace);
                            throw new Exception("Failed to upload file to: [" + operationPath + "].");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn(string.Format("User: {0}, failed to upload file to: {1} at server: {2}. See stack trace.", session.Username, remotePathToFile, session.Domain), ex);
                throw;
            }
        }

        private static async Task<HttpResponseMessage> Upload(Stream stream, UploadItem ui)
        {
            using (var client = new HttpClient())
            {
                string host = "https://s3.amazonaws.com";
                client.BaseAddress = new Uri(host);
                string uri = ui.upload_uri.Substring(host.Length);
                HttpResponseMessage response = await client.PutAsync(uri, new StreamContent(stream));
                return response;
            }
        }

        // POST /files/:source_path_and_filename
        public static void CopyFileOrFolder(Session session, string pathToFileOrFolder, string newPathToFileOrFolder, bool copy = true)
        {
            if (session == null) throw new ArgumentException("Session cannot be null, please login to get a valid session object.");

            pathToFileOrFolder = pathToFileOrFolder.TrimStart('/');
            var operationPath = "/api/rest/v1/files/" + session.Username + "/" + pathToFileOrFolder;
            operationPath = operationPath.TrimEnd('/');

            newPathToFileOrFolder = newPathToFileOrFolder.TrimStart('/');
            var newOperationPath = "/" + session.Username + "/" + newPathToFileOrFolder;
            newOperationPath = newOperationPath.TrimEnd('/');

            var headerParams = new Dictionary<String, String>();
            var queryParams = new Dictionary<String, object>();
            var postParams = new Dictionary<String, object>();
            if (copy) postParams.Add("copy-destination", newOperationPath);
            else postParams.Add("move-destination", newOperationPath);

            try
            {
                string basePath = Protocol + session.Domain;
                string response = Invoke(basePath, operationPath, "POST", queryParams, postParams, headerParams, sessionCookie: session.Cookie);
                if (log.IsDebugEnabled) log.Debug("MOVED " + operationPath + " Expected []: " + response);
            }
            catch (BrickFtpException ex)
            {
                if (ex.ErrorCode == 404)
                {
                    log.Warn("Trying to move/rename a file or folder which does not exist: " + pathToFileOrFolder);
                }
                throw;
            }
        }

        private static object Deserialize(string json, Type type)
        {
            try
            {
                var jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                return JsonConvert.DeserializeObject(json, type, jsonSerializerSettings);
            }
            catch (IOException e)
            {
                throw new BrickFtpException(500, e.Message);
            }

        }

        private static string Serialize(Dictionary<String, object> dict)
        {
            var builder = new StringBuilder();
            foreach (KeyValuePair<String, object> entry in dict)
            {
                if (entry.Value is List<String>)
                {
                    var list = entry.Value as List<String>;
                    foreach (String str in list)
                    {
                        builder.Append(string.Format("{0}[]={1}&", entry.Key, str));
                    }
                }
                else
                {
                    if (entry.Value != null)
                    {
                        builder.Append(string.Format("{0}={1}&", entry.Key, entry.Value));
                    }
                }
            }

            string postData = builder.ToString();
            return postData.TrimEnd('&');
        }

        private static string Invoke(string host, string path, string method, Dictionary<String, object> queryParams, Dictionary<String, object> body, Dictionary<String, String> headerParams, string acceptHeader = "application/json", Cookie sessionCookie = null)
        {
            string querystring = Serialize(queryParams);
            if (querystring != string.Empty) querystring = "?" + querystring;
            
            host = host.EndsWith("/") ? host.Substring(0, host.Length - 1) : host;

            var client = WebRequest.Create(host + path + querystring);
            client.ContentType = "application/x-www-form-urlencoded";  
            client.Method = method;
            if (sessionCookie != null) client.TryAddCookie(sessionCookie);
            if (!string.IsNullOrWhiteSpace(acceptHeader)) client.TryAddAcceptHeader(acceptHeader);
            foreach (var headerParamsItem in headerParams)
            {
                client.Headers.Add(headerParamsItem.Key, headerParamsItem.Value);
            }
            
            switch (method)
            {
                case "GET":
                    break;
                case "POST":
                case "PUT":
                case "DELETE":
                    var swRequestWriter = new StreamWriter(client.GetRequestStream());
                    swRequestWriter.Write(Serialize(body));
                    swRequestWriter.Close();
                    break;
                default:
                    throw new BrickFtpException(500, "unknown method type " + method);
            }

            var webResponse = (HttpWebResponse)client.GetResponse();
            if (webResponse.StatusCode == HttpStatusCode.Created)
            {
                if(log.IsDebugEnabled) log.Debug("Created a resource: "+webResponse.StatusDescription);
            }
            else if (webResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new BrickFtpException((int) webResponse.StatusCode, webResponse.StatusDescription);
            }

            var responseReader = new StreamReader(webResponse.GetResponseStream());
            var responseData = responseReader.ReadToEnd();
            responseReader.Close();

            return responseData;
        }
    }

    // BrickFtpWebModel

    public static class WebRequestExt
    {
        public static bool TryAddCookie(this WebRequest webRequest, Cookie cookie)
        {
            HttpWebRequest httpRequest = webRequest as HttpWebRequest;
            if (httpRequest == null)
            {
                return false;
            }

            if (httpRequest.CookieContainer == null)
            {
                httpRequest.CookieContainer = new CookieContainer();
            }

            httpRequest.CookieContainer.Add(cookie);
            return true;
        }

        public static bool TryAddAcceptHeader(this WebRequest webRequest, string acceptHeader)
        {
            var httpRequest = webRequest as HttpWebRequest;
            if (httpRequest == null)
            {
                return false;
            }

            httpRequest.Accept = acceptHeader;
            return true;
        }
    }

    public class BrickFtpException : Exception
    {
        private int errorCode = 0;

        public BrickFtpException() { }
        public BrickFtpException(int errorCode, string message)
            : base(message)
        {
            this.errorCode = errorCode;
        }

        public int ErrorCode { get { return errorCode; } }
    }
}