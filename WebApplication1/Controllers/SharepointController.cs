using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Metadata;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using WebApplication1.Models;
using System.Data;
using System.Net;
using System.Xml;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    [Route("[controller]")]
    public class SharepointController : Controller
    {
        private readonly IConfiguration _configuration;
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IWebHostEnvironment _hostingEnvironment;
        string tenantId = string.Empty;
        string redirectURL = string.Empty;
        string clientId = string.Empty;
        //string siteScope = string.Empty;
        string SharepointApp_ClientId = string.Empty;
        string targetHost = string.Empty;
        string vaultURL = string.Empty;
        string ClientSecretName = string.Empty;
        string SharepointSecret = string.Empty;
        string endPoint = string.Empty;
        string graphEndpoint = string.Empty;
        string oAuthVersion = string.Empty;
        public SharepointController(IConfiguration configuration)
        {
            _configuration = configuration;
            try
            {
                string ReadFromAppSetting = _configuration["WebApp_readFromAppSetting"];
                if (string.Compare(ReadFromAppSetting, "false").Equals(0))
                {
                    tenantId = _configuration["AzureAD_App_tenantId"];
                    redirectURL = _configuration["AzureAD_App_redirectURL"];
                    clientId = _configuration["AzureAD_App_clientId"];
                    SharepointApp_ClientId = _configuration["Sharepoint_App_clientId"];
                    vaultURL = _configuration["AzureAD_App_vaultURL"];
                    ClientSecretName = _configuration["AzureAD_App_clientSecretName"];
                    SharepointSecret = _configuration["Sharepoint_App_clientSecretName"];
                    targetHost = _configuration["Sharepoint_App_targetHost"];
                    oAuthVersion = _configuration["OAuthVersion"];
                    Def_Values._createLog = Convert.ToBoolean(_configuration["LogCreation"]);
                }
                else
                {
                    tenantId = _configuration["AzureAD_App_tenant_Id"];
                    redirectURL = _configuration["AzureAD_App_redirect_URL"];
                    clientId = _configuration["AzureAD_App_client_Id"];
                    SharepointApp_ClientId = _configuration["Sharepoint_App_client_Id"];
                    vaultURL = _configuration["AzureAD_App_vault_URL"];
                    ClientSecretName = _configuration["AzureAD_App_clientSecret_Name"];
                    SharepointSecret = _configuration["Sharepoint_App_clientSecret_Name"];
                    targetHost = _configuration["Sharepoint_App_target_Host"];
                    oAuthVersion = _configuration["OAuth_Version"];
                    Def_Values._createLog = Convert.ToBoolean(_configuration["LogCreation"]);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
            }
        }
        public void getEndpoints(string custType)
        {
            try
            {
                if (string.Compare(custType, "COMM", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    endPoint = _configuration["CommEndpoint"];
                    graphEndpoint = _configuration["CommGraphEndpoint"];
                }
                else if (string.Compare(custType, "GOV", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    endPoint = _configuration["GovEndpoint"];
                    graphEndpoint = _configuration["GovGraphEndpoint"];
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
            }
        }
        public RestRequest getRequest(Method method, string Content_Type, string grant_type, string client_id, string scope, string redirect_uri, string client_secret)
        {
            var request = new RestRequest(method);
            try
            {
                if (!string.IsNullOrEmpty(Content_Type))
                {
                    request.AddHeader("Content-Type", Content_Type);
                }
                request.AddHeader("Access-Control-Allow-Origin", "*");
                if (!string.IsNullOrEmpty(grant_type))
                {
                    request.AddParameter("grant_type", grant_type);
                }
                if (!string.IsNullOrEmpty(client_id))
                {
                    request.AddParameter("client_id", client_id);
                }
                if (!string.IsNullOrEmpty(scope))
                {
                    request.AddParameter("scope", scope);
                }
                if (!string.IsNullOrEmpty(redirect_uri))
                {
                    request.AddParameter("redirect_uri", redirect_uri);
                }
                if (!string.IsNullOrEmpty(client_secret))
                {
                    request.AddParameter("client_secret", client_secret);
                }
            }
            catch (Exception ex)
            {
                request = new RestRequest(ex.ToString());
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
            }
            return request;
        }
        /// <summary>Gets the authentication URL.</summary>
        /// <param name="custType">Type of the customer.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        ///   authentication URL
        /// </returns>
        [HttpGet(Name = "get_authURL")]
        //[Route("[action]")]
        //[Route("api/Sharepoint/get_authURL")]

        public IActionResult get_authURL(string custType, string userId)
        {
            try
            {
                LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().Name, "Info : Started...");
                //log.Info(System.Reflection.MethodBase.GetCurrentMethod().Name + "Info : Started...");
                getEndpoints(custType);
                string scope = "offline_access " + graphEndpoint + ".default";
                if (!string.IsNullOrEmpty(endPoint) && !string.IsNullOrEmpty(tenantId))
                {
                    string res = endPoint + tenantId + "/oauth2/" + oAuthVersion + "/authorize?client_id=" + clientId + "&response_type=code&redirect_uri=" + redirectURL + "&response_mode=query&scope=" + scope + "&state=" + userId + "";
                    return Ok(res);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                return NotFound(ex);
            }
        }
        /// <summary>
        /// Get the token using authentication code
        /// </summary>
        /// <param name="custType">Type of the customer</param>
        /// <param name="userId">The user identifier</param>
        /// <returns>Token</returns>
        //[HttpGet]
        //[Route("[action]")]
        //[Route("api/Sharepoint/getToken")]
        [HttpGet(Name = "getToken")]
        public ContentResult getToken(string custType, string userId)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().Name, "Info : Started...");
            string res = string.Empty;
            //log.Info("getToken Type = " + custType + " called at " + DateTime.Now);
            getEndpoints(custType);
            var tokenData = Def_Values.tokenList.FirstOrDefault(x => x.state == userId);
            if (tokenData != null)
            {
                if (tokenData.token_Expiry > DateTime.Now)
                {
                    return getResponse(HttpStatusCode.OK, tokenData.response);
                }
                else
                {
                    return getOBOtoken(userId);
                }
            }
            else
            {
                return getOBOtoken(userId);
            }
        }
        public ContentResult getOBOtoken(string userId)
        {
            var data = Def_Values.authList.FirstOrDefault(x => x.state == userId);
            if (!string.IsNullOrEmpty(data.code))
            {
                try
                {
                    var client = new RestClient("" + endPoint + "" + tenantId + "/oauth2/" + oAuthVersion + "/token");
                    client.Timeout = -1;
                    var request = getRequest(Method.POST, "application/x-www-form-urlencoded", "authorization_code", clientId, graphEndpoint + ".default", redirectURL, Def_Values.client_Secret);
                    request.AddParameter("code", data.code);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.Accepted | response.StatusCode == HttpStatusCode.OK)
                    {
                        DataSet ds = JsonToDataSet(response.Content);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            dynamic PdJson = null;
                            PdJson = JsonConvert.DeserializeObject(response.Content);
                            if (ds.Tables[0].Columns.Contains("expires_in") && ds.Tables[0].Columns.Contains("access_token"))
                            {
                                int expiry = Convert.ToInt32(ds.Tables[0].Rows[0]["expires_in"]);
                                string res = Convert.ToString(ds.Tables[0].Rows[0]["access_token"]);
                                if (expiry > 0)
                                {
                                    DateTime dtTokenExpiry = DateTime.Now.AddSeconds(expiry);
                                    Def_Values.tokenList.Add(new Token_List(userId, res, dtTokenExpiry, Convert.ToString(PdJson)));
                                }
                            }
                            data.code = string.Empty;
                            return getResponse(response.StatusCode, JsonConvert.SerializeObject(PdJson, Newtonsoft.Json.Formatting.Indented));
                        }
                        else
                        {
                            return getResponse(HttpStatusCode.BadRequest, response.Content);
                        }
                    }
                    else
                    {
                        return getResponse(HttpStatusCode.BadRequest, response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                    return getResponse(HttpStatusCode.BadRequest, ex.ToString());
                }
            }
            else
            {
                return getResponse(HttpStatusCode.NotFound, "Authentication code not found."); ;
            }
        }
        /// <summary>
        /// Get token as a system based
        /// </summary>
        /// <param name="custType"></param>
        /// <returns>Token</returns>
        //[HttpGet]
        //[Route("[action]")]
        //[Route("api/Sharepoint/getToken_System")]
        [HttpGet(Name = "getToken_System")]
        public async Task<ContentResult> getToken_System(string custType)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : Started...");
            ContentResult responseMessage;
            try
            {
                responseMessage = await System_GetToken(custType);
                return responseMessage;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
        /// <summary>
        /// Get Client secret from key vault
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public async Task<string> getSecret(string secretName)
        {
            string actualSecret;
            try
            {
                var credential = new DefaultAzureCredential();
                var client = new SecretClient(new Uri(vaultURL), credential);
                var secret = await client.GetSecretAsync(secretName);
                actualSecret = secret.Value.Value;
            }
            catch (Exception ex)
            {
                actualSecret = ex.ToString();
            }
            return actualSecret;
        }
        /// <summary>
        /// Get Token as system based
        /// </summary>
        /// <param name="custType"></param>
        /// <returns>Token</returns>
        public async Task<ContentResult> System_GetToken(string custType)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : Started...");
            if (string.IsNullOrEmpty(Def_Values.client_Secret))
            {
                Def_Values.client_Secret = await getSecret(ClientSecretName);
            }
            try
            {
                getEndpoints(custType);
                var client = new RestClient("" + endPoint + "" + tenantId + "/oauth2/" + oAuthVersion + "/token");
                client.Timeout = -1;
                var request = getRequest(Method.POST, "application/x-www-form-urlencoded", "client_credentials", clientId, graphEndpoint + ".default", string.Empty, Def_Values.client_Secret);
                IRestResponse response = client.Execute(request);
                //log.Info("getToken_System End at " + DateTime.Now);
                return getResponse(response.StatusCode, response.Content);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
        public string get_Token(ContentResult responseMessage)
        {
            DataSet dsJson = JsonToDataSet(responseMessage.Content.ToString());
            if (dsJson != null && dsJson.Tables.Count > 0)
            {
                return Convert.ToString(dsJson.Tables[0].Rows[0]["access_token"]);
            }
            else
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Get Token using refresh token
        /// </summary>
        /// <param name="refresh_token"></param>
        /// <param name="custType"></param>
        /// <returns>Token</returns>
        //[HttpGet]
        //[Route("[action]")]
        //[Route("api/Sharepoint/getToken_by_RefreshToken")]
        [HttpGet(Name = "getToken_by_RefreshToken")]
        public ContentResult getToken_by_RefreshToken(string refresh_token, string custType)
        {
            try
            {
                LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().Name, "Info : Started...");
                getEndpoints(custType);
                var client = new RestClient(endPoint + tenantId + "/oauth2/v2.0/token");
                client.Timeout = -1;
                var request = getRequest(Method.POST, "application/x-www-form-urlencoded", "refresh_token", clientId, graphEndpoint + ".default", redirectURL, Def_Values.client_Secret);
                request.AddParameter("refresh_token", refresh_token);
                IRestResponse response = client.Execute(request);
                //log.Info("getToken_by_RefreshToken End at " + DateTime.Now);
                return getResponse(response.StatusCode, response.Content);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        /// <summary>
        /// Convert json to dataset
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns>dataset</returns>
        public DataSet JsonToDataSet(string jsonString)
        {
            DataSet ds = new DataSet();
            try
            {
                if (!string.IsNullOrEmpty(jsonString))
                {
                    jsonString = "{ \"rootNode\": {" + jsonString.Trim().TrimStart('{').TrimEnd('}') + "} }";
                    XmlDocument xd = JsonConvert.DeserializeXmlNode(jsonString);
                    ds.ReadXml(new XmlNodeReader(xd));
                }
                else
                {
                    ds = null;
                }
            }
            catch (Exception ex)
            {
                ds = null;
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
            }
            return ds;
        }
        /// <summary>
        /// Create folder structure using json
        /// </summary>
        /// <param name="fs"></param>
        /// <returns>status</returns>
        //[HttpPost]
        //[Route("[action]")]
        //[Route("api/Sharepoint/CreateFolderStructure")]
        [HttpPost(Name = "CreateFolderStructure")]
        public IActionResult CreateFolderStructure([FromBody] FolderStructure fs)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().Name, "Info : Started...");
            IRestResponse response = null;
            try
            {
                DataSet ds = JsonToDataSet(fs.json);
                if (ds.Tables.Count > 0)
                {
                    foreach (DataTable dt in ds.Tables)
                    {
                        if (dt.TableName == "ROOTFOLDER")
                        {
                            string[] columnNames = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
                            if (columnNames.Length > 0)
                            {
                                foreach (var item in columnNames)
                                {
                                    try
                                    {
                                        if (item == "ROOTFOLDER_Id")
                                        {
                                            continue;
                                        }
                                        else
                                        {

                                            string folderName = Convert.ToString(item);
                                            response = Create_Folder(fs.token, fs.rootPath + "/" + folderName);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        response = (IRestResponse)ex;
                                        LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                                    }
                                }
                                continue;
                            }
                        }
                        //Searching for the table whose name contains "ROOTFOLDER"
                        if (dt.Columns.Contains("ROOTFOLDER_Id"))
                        {
                            string folderName = Convert.ToString(dt.TableName);
                            if (!string.IsNullOrEmpty(folderName))
                            {
                                try
                                {
                                    response = Create_Folder(fs.token, fs.rootPath + "/" + folderName);
                                }
                                catch (Exception ex)
                                {
                                    response = (IRestResponse)ex;
                                    LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                                }
                            }
                            string[] columnNames = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
                            if (columnNames.Length > 0)
                            {
                                foreach (var item in columnNames)
                                {
                                    try
                                    {
                                        if (item == "ROOTFOLDER_Id")
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            string val = Convert.ToString(dt.Rows[0][item]);
                                            if (string.IsNullOrEmpty(val))
                                            {
                                                response = Create_Folder(fs.token, fs.rootPath + "/" + folderName + "/" + item);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        response = (IRestResponse)ex;
                                        LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                                    }
                                }
                            }
                            if (dt.Columns.Contains(folderName + "_Id"))
                            {
                                //int Id = Convert.ToInt32(dt.Rows[0][folderName + "_Id"]);
                                foreach (DataTable dt1 in ds.Tables)
                                {
                                    try
                                    {
                                        if (dt1.TableName == "ROOTFOLDER" | dt1.TableName == folderName)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (dt1.Columns.Contains(folderName + "_Id"))
                                            {
                                                string subFolder = Convert.ToString(dt1.TableName);
                                                response = Create_Folder(fs.token, fs.rootPath + "/" + folderName + "/" + subFolder);
                                                string[] colNames = dt1.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
                                                if (colNames.Length > 0)
                                                {
                                                    foreach (var item in colNames)
                                                    {
                                                        try
                                                        {
                                                            if (item == folderName + "_Id")
                                                            {
                                                                continue;
                                                            }
                                                            else
                                                            {
                                                                string val = Convert.ToString(dt1.Rows[0][item]);
                                                                if (string.IsNullOrEmpty(val))
                                                                {
                                                                    response = Create_Folder(fs.token, fs.rootPath + "/" + folderName + "/" + subFolder + "/" + item);
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            response = (IRestResponse)ex;
                                                            LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        response = (IRestResponse)ex;
                                        LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                                    }
                                }
                            }
                        }

                    }
                    if (response.StatusCode == HttpStatusCode.Accepted | response.StatusCode == HttpStatusCode.OK | response.StatusCode == HttpStatusCode.Created)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return NotFound(response);
                    }
                }
                else
                {
                    return NotFound("Please check request body.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                return NotFound(ex.ToString());
            }
        }
        /// <summary>
        /// Create Folder
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="rootPath"></param>
        public IRestResponse Create_Folder(string Token, string rootPath)
        {
            IRestResponse response;
            try
            {
                var client = new RestClient(rootPath);
                client.Timeout = -1;
                var request = new RestRequest(Method.PATCH);
                request.AddHeader("Authorization", "Bearer " + Token + "");
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
                " + "\n" +
                @"    ""folder"": {},
                " + "\n" +
                @"    ""@microsoft.graph.conflictBehavior"": ""fail""
                " + "\n" +
                @"}";
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                response = client.Execute(request);
            }
            catch (Exception ex)
            {
                response = (IRestResponse)ex;
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
            }
            return response;
        }
        public ContentResult getResponse(HttpStatusCode httpStatusCode, string message)
        {
            //HttpResponseMessage responseMessage;
            try
            {
                return new ContentResult
                {
                    Content = message,
                    ContentType = "text/json",
                    StatusCode = Convert.ToInt32(httpStatusCode)
                };
            }
            catch (Exception ex)
            {
                return new ContentResult
                {
                    Content = ex.ToString(),
                    ContentType = "text/json",
                    StatusCode = Convert.ToInt32(HttpStatusCode.BadRequest)
                };
            }
        }
        /// <summary>
        /// Get file versions
        /// </summary>
        /// <param name="fs"></param>
        /// <returns>versions</returns>
        //[HttpPost]
        //[Route("[action]")]
        //[Route("api/Sharepoint/getVersion")]
        [HttpPost(Name = "getVersion")]
        public async Task<ContentResult> getVersion([FromBody] Files fs)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : Started...");
            string res;
            try
            {
                //log.Info("getVersion called at " + DateTime.Now);
                if (string.IsNullOrEmpty(fs.access_token))
                {
                    ContentResult responseMessage = await System_GetToken(fs.custType);
                    if (responseMessage.StatusCode.Equals(200))
                    {
                        fs.access_token = get_Token(responseMessage);
                    }
                    else
                    {
                        return responseMessage;
                    }
                }
                var client = new RestClient(fs.path);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + fs.access_token + "");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.Accepted | response.StatusCode == HttpStatusCode.OK)
                {
                    string version = Convert.ToString(client.BaseUrl.Segments[1]);
                    string siteId = Convert.ToString(client.BaseUrl.Segments[3]);
                    string driveId = Convert.ToString(client.BaseUrl.Segments[5]);
                    DataSet ds = JsonToDataSet(response.Content);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        string id = Convert.ToString(ds.Tables[0].Rows[0]["id"]);
                        string versionURL = "https://graph.microsoft.com/" + version + "sites/" + siteId + "drives/" + driveId + "items/" + id + "/versions";
                        IRestResponse versionResponse = getVersionById(versionURL, fs.access_token);
                        if (versionResponse.StatusCode == HttpStatusCode.Accepted | versionResponse.StatusCode == HttpStatusCode.OK)
                        {

                            DataSet dsVersion = JsonToDataSet(versionResponse.Content);
                            dynamic PdJson = null;
                            if (dsVersion != null && dsVersion.Tables.Count > 0 && dsVersion.Tables.Contains("value"))
                            {
                                List<VersionURL> ls = new List<VersionURL>();
                                foreach (DataRow item in dsVersion.Tables["value"].Rows)
                                {
                                    string versionId = Convert.ToString(item["id"]);
                                    ls.Add(new VersionURL(versionId, versionURL + "/" + versionId));
                                }
                                var json = JsonConvert.SerializeObject(ls);
                                PdJson = JsonConvert.DeserializeObject(json);
                            }
                            res = JsonConvert.SerializeObject(PdJson, Newtonsoft.Json.Formatting.Indented);
                            return getResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            //log.Info("getVersion End at " + DateTime.Now);
                            return getResponse(versionResponse.StatusCode, versionResponse.Content);
                        }
                    }
                    else
                    {
                        //log.Info("getVersion End at " + DateTime.Now);
                        return getResponse(HttpStatusCode.BadRequest, response.Content);
                    }
                }
                else
                {
                    return getResponse(response.StatusCode, response.Content);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
        /// <summary>
        /// Get sharepoint file version using file id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="driveId"></param>
        /// <param name="access_Token"></param>
        /// <param name="Id"></param>
        /// <returns>id and version</returns>
        public IRestResponse getVersionById(string versionURL, string access_Token)
        {
            IRestResponse response;
            try
            {
                //log.Info("getVersionById called at " + DateTime.Now);
                var client = new RestClient(versionURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + access_Token + "");
                response = client.Execute(request);
                //log.Info("getVersionById End at " + DateTime.Now);
            }
            catch (Exception ex)
            {
                response = (IRestResponse)ex;
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
            }
            return response;
        }
        /// <summary>
        /// Get files from sharepoint
        /// </summary>
        /// <param name="fs"></param>
        /// <returns>files</returns>
        //[HttpPost]
        //[Route("[action]")]
        //[Route("api/Sharepoint/getFiles")]
        [HttpPost(Name = "getFiles")]
        public async Task<ContentResult> getFiles([FromBody] Files fs)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : Started...");
            try
            {
                //log.Info("getFiles called at " + DateTime.Now);
                if (string.IsNullOrEmpty(fs.access_token))
                {
                    ContentResult responseMessage = await System_GetToken(fs.custType);
                    if (responseMessage.StatusCode.Equals(200))
                    {
                        fs.access_token = get_Token(responseMessage);
                    }
                    else
                    {
                        return responseMessage;
                    }
                }
                var client = new RestClient(fs.path + ":/children?$expand=listitem");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + fs.access_token + "");
                IRestResponse response = client.Execute(request);
                return getResponse(response.StatusCode, response.Content);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
        /// <summary>
        /// Get sharepoint access token as system based
        /// </summary>
        /// <returns>sharepoint access token</returns>
        //[HttpGet]
        //[Route("[action]")]
        //[Route("api/Sharepoint/System_GetSiteToken")]

        [HttpGet(Name = "System_GetSiteToken")]
        public async Task<IActionResult> System_GetSiteToken()
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : Started...");
            if (string.IsNullOrEmpty(Def_Values.sharepoint_Secret))
            {
                Def_Values.sharepoint_Secret = await getSecret(SharepointSecret);
            }
            try
            {
                var client = new RestClient("https://accounts.accesscontrol.windows.net/" + tenantId + "/tokens/OAuth/2");
                client.Timeout = -1;
                var request = getRequest(Method.POST, "application/x-www-form-urlencoded", "client_credentials", SharepointApp_ClientId + "@" + tenantId, string.Empty, string.Empty, Def_Values.sharepoint_Secret);
                request.AddParameter("resource", "00000003-0000-0ff1-ce00-000000000000/" + targetHost + "@" + tenantId);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.Accepted | response.StatusCode == HttpStatusCode.OK)
                {
                    DataSet ds = JsonToDataSet(response.Content);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        dynamic PdJson = null;
                        PdJson = JsonConvert.DeserializeObject(response.Content);
                        string res = JsonConvert.SerializeObject(PdJson, Newtonsoft.Json.Formatting.Indented);
                        //log.Info("System_GetSiteToken by System End at " + DateTime.Now);
                        return Ok(res);
                    }
                    else
                    {
                        //log.Info("System_GetSiteToken by System End at " + DateTime.Now);
                        return NotFound(response.Content);
                    }
                }
                else
                {
                    //log.Info("System_GetSiteToken by System End at " + DateTime.Now);
                    return NotFound(response.Content);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return NotFound(ex.ToString());
            }
        }
        /// <summary>
        /// Create folder
        /// </summary>
        /// <param name="fs"></param>
        /// <returns>status</returns>
        //[HttpPost]
        //[Route("[action]")]
        //[Route("api/Sharepoint/System_CreateFolder")]
        [HttpPost(Name = "System_CreateFolder")]
        public async Task<ContentResult> System_CreateFolder([FromBody] Files fs)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : Started...");
            try
            {
                //log.Info("System_CreateFolder by System called at " + DateTime.Now);
                if (string.IsNullOrEmpty(fs.access_token))
                {
                    ContentResult responseMessage = await System_GetToken(fs.custType);
                    if (responseMessage.StatusCode.Equals(200))
                    {
                        fs.access_token = get_Token(responseMessage);
                    }
                    else
                    {
                        return responseMessage;
                    }
                }
                IRestResponse response = Create_Folder(fs.access_token, fs.path);
                return getResponse(response.StatusCode, response.Content);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
        /// <summary>
        /// Compare sharepoint file using file version
        /// </summary>
        /// <param name="fs"></param>
        /// <returns>version url</returns>
        //[HttpPost]
        //[Route("[action]")]
        //[Route("api/Sharepoint/System_FileContent_Byte")]

        [HttpPost(Name = "System_FileContent_Byte")]
        public async Task<ContentResult> System_FileContent_Byte([FromBody] ByteToFile fs)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : Started...");
            try
            {
                if (string.IsNullOrEmpty(fs.access_token))
                {
                    ContentResult responseMessage = await System_GetToken(fs.custType);
                    if (responseMessage.StatusCode.Equals(200))
                    {
                        fs.access_token = get_Token(responseMessage);
                    }
                    else
                    {
                        return responseMessage;
                    }
                }
                byte[] fileContentArray1 = getFileArray(fs.fileUrl1, fs.access_token);
                byte[] fileContentArray2 = getFileArray(fs.fileUrl2, fs.access_token);
                if (fileContentArray1 != null && fileContentArray2 != null)
                {
                    CompareDocuments cd = new CompareDocuments(_configuration);
                    ContentResult responseMessage = cd.getCompareURL_Byte(fileContentArray1, fileContentArray2, fs.fileName1, fs.fileName2);
                    return responseMessage;
                }
                else
                {
                    return getResponse(HttpStatusCode.BadRequest, "File version not found");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        /// <summary>
        /// Get file bytes
        /// </summary>
        /// <param name="fileURL"></param>
        /// <param name="access_token"></param>
        /// <returns>bytes</returns>
        public byte[] getFileArray(string fileURL, string access_token)
        {
            byte[] bytes = null;
            try
            {
                var client = new RestClient(fileURL + "/content");
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + access_token + "");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.Accepted | response.StatusCode == HttpStatusCode.OK | response.StatusCode == HttpStatusCode.Created)
                {
                    bytes = response.RawBytes;
                }
                else
                {
                    DataSet dsVersion = JsonToDataSet(response.Content);
                    if (dsVersion != null && dsVersion.Tables.Count > 0)
                    {
                        if (dsVersion.Tables.Contains("error"))
                        {
                            string message = Convert.ToString(dsVersion.Tables[0].Rows[0]["message"]);
                            if (message.Equals("You cannot get the content of the current version."))
                            {
                                string siteId = Convert.ToString(client.BaseUrl.Segments[3]);
                                string driveId = Convert.ToString(client.BaseUrl.Segments[5]);
                                string Id = Convert.ToString(client.BaseUrl.Segments[7]);
                                client = new RestClient("https://graph.microsoft.com/v1.0/sites/" + siteId + "drives/" + driveId + "items/" + Id + "/content");
                                request = new RestRequest(Method.GET);
                                request.AddHeader("Authorization", "Bearer " + access_token + "");
                                response = client.Execute(request);
                                if (response.StatusCode == HttpStatusCode.Accepted | response.StatusCode == HttpStatusCode.OK | response.StatusCode == HttpStatusCode.Created)
                                {
                                    bytes = response.RawBytes;
                                }
                            }
                            else
                            {
                                bytes = response.RawBytes;
                            }
                        }
                        else
                        {
                            bytes = response.RawBytes;
                        }
                    }
                    else
                    {
                        bytes = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
            }
            return bytes;
        }

        /// <summary>
        /// Upload files to sharepoint
        /// </summary>
        /// <param name="uf"></param>
        /// <returns>status</returns>
        //[HttpPost]
        //[Route("[action]")]
        //[Route("api/Sharepoint/UploadFiles")]
        [HttpPost(Name = "UploadFiles")]
        public async Task<ContentResult> UploadFiles([FromBody] Upload_Files uf)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : Started...");
            try
            {
                //log.Info("UploadFiles called at " + DateTime.Now);
                if (string.IsNullOrEmpty(uf.access_token))
                {
                    ContentResult responseMessage = await System_GetToken(uf.custType);
                    if (responseMessage.StatusCode.Equals(200))
                    {
                        uf.access_token = get_Token(responseMessage);
                    }
                    else
                    {
                        return responseMessage;
                    }
                }
                if (!string.IsNullOrEmpty(uf.Base64string) && !string.IsNullOrEmpty(uf.filePath))
                {
                    long length = uf.Base64string.Length / 1024;
                    if (length > 3999)
                    {
                        //log.Info("UploadFiles End at " + DateTime.Now);
                        return uploadLargeFiles(uf);
                    }
                    else
                    {
                        //log.Info("UploadFiles End at " + DateTime.Now);
                        return uploadSmallFiles(uf);
                    }
                }
                else
                {
                    //log.Info("UploadFiles End at " + DateTime.Now);
                    return getResponse(HttpStatusCode.BadRequest, "Please check binary data and filepath.");
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
        /// <summary>
        /// Upload big files to sharepoint
        /// </summary>
        /// <param name="uf"></param>
        /// <returns></returns>
        public ContentResult uploadLargeFiles(Upload_Files uf)
        {
            //ContentResult responseMessage = null;
            try
            {
                //log.Info("uploadLargeFiles called at " + DateTime.Now);
                var client = new RestClient(uf.filePath + ":/createUploadSession");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + uf.access_token);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK | response.StatusCode == HttpStatusCode.Created)
                {
                    DataSet ds = JsonToDataSet(response.Content);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Columns.Contains("uploadUrl"))
                        {
                            string session = Convert.ToString(ds.Tables[0].Rows[0]["uploadUrl"]);
                            return UploadFileBySession(session, Convert.FromBase64String(uf.Base64string));
                        }
                        else
                        {
                            return getResponse(response.StatusCode, response.Content);
                        }
                    }
                    else
                    {
                        return getResponse(response.StatusCode, response.Content);
                    }
                }
                else
                {
                    return getResponse(response.StatusCode, response.Content);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
            //return responseMessage;
        }
        /// <summary>
        /// Create upload session for big files upload
        /// </summary>
        /// <param name="url"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private ContentResult UploadFileBySession(string url, byte[] file)
        {
            HttpResponseMessage responseMessage = null;
            try
            {
                //log.Info("UploadFileBySession by System called at " + DateTime.Now);
                int fragSize = 1024 * 1024 * 4;
                var arrayBatches = ByteArrayIntoBatches(file, fragSize);
                int start = 0;
                string JsonContent = "";

                foreach (var byteArray in arrayBatches)
                {
                    int byteArrayLength = byteArray.Length;
                    var contentRange = " bytes " + start + "-" + (start + (byteArrayLength - 1)) + "/" + file.Length;

                    using (var client = new HttpClient())
                    {
                        var content = new ByteArrayContent(byteArray);
                        content.Headers.Add("Content-Length", byteArrayLength.ToString());
                        content.Headers.Add("Content-Range", contentRange);
                        responseMessage = client.PutAsync(url, content).Result;
                        JsonContent = responseMessage.Content.ReadAsStringAsync().Result;
                    }

                    start = start + byteArrayLength;
                }
                //log.Info("UploadFileBySession called at " + DateTime.Now);
                return getResponse(responseMessage.StatusCode, responseMessage.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
        /// <summary>
        /// Chunking big files into small batches
        /// </summary>
        /// <param name="bArray"></param>
        /// <param name="intBufforLengt"></param>
        /// <returns></returns>
        internal IEnumerable<byte[]> ByteArrayIntoBatches(byte[] bArray, int intBufforLengt)
        {
            int bArrayLenght = bArray.Length;
            int i = 0;
            byte[] bReturn;
            for (; bArrayLenght > (i + 1) * intBufforLengt; i++)
            {
                bReturn = new byte[intBufforLengt];
                Array.Copy(bArray, i * intBufforLengt, bReturn, 0, intBufforLengt);
                yield return bReturn;
            }

            int intBufforLeft = bArrayLenght - i * intBufforLengt;
            if (intBufforLeft > 0)
            {
                bReturn = new byte[intBufforLeft];
                Array.Copy(bArray, i * intBufforLengt, bReturn, 0, intBufforLeft);
                yield return bReturn;
            }
        }
        /// <summary>
        /// Upload small files to sharepoint
        /// </summary>
        /// <param name="uf"></param>
        /// <returns>status</returns>
        public ContentResult uploadSmallFiles(Upload_Files uf)
        {
            HttpResponseMessage response = null;
            try
            {
                //log.Info("uploadSmallFiles called at " + DateTime.Now);
                string ext = "application/" + Path.GetExtension(uf.filePath).Replace(".", "");
                using (var client = new HttpClient())
                {
                    string requestUrl = uf.filePath + ":/content";
                    var content = new ByteArrayContent(Convert.FromBase64String(uf.Base64string));
                    content.Headers.Add("Content-Type", ext);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + uf.access_token);
                    response = client.PutAsync(requestUrl, content).Result;
                    //log.Info("uploadSmallFiles called at " + DateTime.Now);
                    return getResponse(response.StatusCode, response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
        //[HttpPost]
        //[Route("[action]")]
        //[Route("api/Sharepoint/updateSPMetdata")]
        [HttpPost(Name = "updateSPMetdata")]
        public ContentResult updateSPMetdata([FromBody] MetadataValues fs)
        {
            LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().Name, "Info : Started...");
            try
            {
                UpdateMetadata obj = new UpdateMetadata();
                _ = obj.updateSPMetdata(fs);
                return getResponse(HttpStatusCode.OK, "Metadata updating.");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
    }
}
