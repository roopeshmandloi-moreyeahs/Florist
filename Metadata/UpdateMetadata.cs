using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Data;
using System.Net;
using System.Xml;

namespace Metadata
{
    public class UpdateMetadata
    {
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
        public async Task<ContentResult> updateSPMetdata(MetadataValues fs)
        {
            try
            {
                LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : Metadata update Started");
                string folderContentTypeId = getFolderContentType(fs.rootfolderapi, fs.accessToken);
                if (!string.IsNullOrEmpty(folderContentTypeId))
                {
                    //log.Info("getFiles called at " + DateTime.Now);
                    var client = new RestClient(fs.rootfolderapi + ":/children?$expand=listItem");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Authorization", "Bearer " + fs.accessToken + "");
                    IRestResponse response = client.Execute(request);
                    ContentResult res = null;
                    if (response.StatusCode == HttpStatusCode.OK | response.StatusCode == HttpStatusCode.Accepted)
                    {
                        DataSet ds = JsonToDataSet(response.Content);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables.Contains("file") | ds.Tables.Contains("folder"))
                            {
                                res = await updateFolderMetadata(ds, fs.accessToken, fs.metadatajson, fs.rootfolderapi, folderContentTypeId, fs.contentTypeIds);
                                return res;
                            }
                            else
                            {
                                LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().Name, "Info : " + "No any files or subfolders inside this " + fs.rootfolderapi);
                                return getResponse(response.StatusCode, response.Content);
                            }
                        }
                        else
                        {
                            LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + response.Content);
                            return getResponse(response.StatusCode, response.Content);
                        }
                    }
                    else
                    {
                        LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + response.Content);
                        return getResponse(response.StatusCode, response.Content);
                    }
                }
                else
                {
                    LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + "Error while getting folder content type id.");
                    return getResponse(HttpStatusCode.BadRequest, "Error while getting folder content type id.");
                }                
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
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
        public string getFolderContentType(string folderPath, string token)
        {
            string res = string.Empty;
            try
            {
                var client = new RestClient(folderPath + "?$expand=listItem");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + token + "");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK | response.StatusCode == HttpStatusCode.Accepted)
                {
                    DataSet ds = JsonToDataSet(response.Content);
                    res = Convert.ToString(ds.Tables["contentType"].Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().Name, "Err : " + ex.ToString());
            }
            return res;
        }
        public async Task<ContentResult> updateFolderMetadata(DataSet ds, string access_token, string json, string path, string folderContentTypeId, string contentTypeIds)
        {
            ContentResult res = null;
            try
            {
                JToken[] exclude = null;
                JObject obj = JObject.Parse(contentTypeIds);
                if (obj.ContainsKey(folderContentTypeId))
                {
                    exclude = obj[folderContentTypeId].ToArray();
                }
                var dtFile = ds.Tables["file"];
                if (ds.Tables.Contains("file"))
                {                    
                    foreach (DataRow item in ds.Tables["file"].Rows)
                    {
                        try
                        {
                            int value_id = Convert.ToInt32(item[2]);
                            DataRow[] dr = ds.Tables["value"].Select("value_id = " + value_id + "");
                            if (dr.Length > 0)
                            {
                                string fileName = Convert.ToString(dr[0][4]);
                                res = await updateFileMetadata(path + "/" + fileName, access_token, json);
                            }
                        }
                        catch (Exception ex)
                        {
                            res = getResponse(HttpStatusCode.BadRequest, ex.ToString());
                            LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                        }
                    }
                }
                if (ds.Tables.Contains("folder"))
                {
                    foreach (DataRow item in ds.Tables["folder"].Rows)
                    {
                        try
                        {
                            int value_id = Convert.ToInt32(item[1]);
                            DataRow[] dr = ds.Tables["value"].Select("value_id = " + value_id + "");
                            if (!ds.Tables.Contains("contentType"))
                            {
                                continue;
                            }
                            DataRow[] drCtype = ds.Tables["contentType"].Select("listItem_Id = " + value_id + "");
                            string contentTypeId = Convert.ToString(drCtype[0][0]);
                            bool flag = false;
                            if (exclude != null)
                            {
                                foreach (var excludeitem in exclude)
                                {
                                    string exValue = excludeitem.ToString();
                                    if (contentTypeId.Contains(exValue))
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                            if (flag)
                            {
                                continue;
                            }
                            else
                            {
                                if (dr.Length > 0)
                                {
                                    string folderName = Convert.ToString(dr[0][4]);
                                    res = await updateFileMetadata(path + "/" + folderName, access_token, json);
                                    var client = new RestClient(path + "/" + folderName + ":/children?$expand=listItem");
                                    client.Timeout = -1;
                                    var request = new RestRequest(Method.GET);
                                    request.AddHeader("Authorization", "Bearer " + access_token + "");
                                    IRestResponse response = client.Execute(request);
                                    if (response.StatusCode == HttpStatusCode.OK | response.StatusCode == HttpStatusCode.Accepted)
                                    {
                                        DataSet dsFolder = JsonToDataSet(response.Content);
                                        if (dsFolder != null && dsFolder.Tables.Count > 0)
                                        {
                                            if (dsFolder.Tables.Contains("file") | ds.Tables.Contains("folder"))
                                            {
                                                res = await updateFolderMetadata(dsFolder, access_token, json, path + "/" + folderName, folderContentTypeId, contentTypeIds);
                                            }
                                            else
                                            {
                                                LogHelper.WriteLog("INFO", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Info : " + "No any files or subfolders inside this " + folderName);
                                            }
                                        }
                                        else
                                        {
                                            LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + response.Content);
                                        }
                                    }
                                    else
                                    {
                                        LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + response.Content);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            res = getResponse(HttpStatusCode.BadRequest, ex.ToString());
                            LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                        }
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
        /// <summary>
        /// update sharepoint single file metadata
        /// </summary>
        /// <param name="path"></param>
        /// <param name="access_token"></param>
        /// <param name="json"></param>
        /// <returns>ContentResult with status code</returns>
        public async Task<ContentResult> updateFileMetadata(string path, string access_token, string json)
        {
            try
            {
                var client = new RestClient(path + ":/listItem/fields");
                client.Timeout = -1;
                var request = new RestRequest(Method.PATCH);
                request.AddHeader("Authorization", "Bearer " + access_token);
                request.AddHeader("Content-Type", "application/json");
                var body = json;
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                var cancellationTokenSource = new CancellationTokenSource();
                var response = await client.ExecuteAsync(request, cancellationTokenSource.Token);
                return getResponse(response.StatusCode, response.Content);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ERROR", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Substring(1).Split('>')[0], "Err : " + ex.ToString());
                return getResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
        }
    }
}
