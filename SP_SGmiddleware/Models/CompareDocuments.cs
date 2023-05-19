using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;


namespace SP_SGmiddleware.Models
{
    public class CompareDocuments
    {
        public static string accountId = string.Empty;
        public static string authToken = string.Empty;
        public static string baseUrl = string.Empty;
        public CompareDocuments(IConfiguration configuration)
        {
            accountId = configuration["DocCompare_accountId"];
            authToken = configuration["DocCompare_authToken"];
            baseUrl = configuration["DocCompare_baseUrl"];
        }
        public ContentResult getCompareURL_Byte(byte[] fileContentArray1, byte[] fileContentArray2, string fileName1, string fileName2)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                string fileType1 = Path.GetExtension(fileName1).Replace(".", "");
                string fileType2 = Path.GetExtension(fileName2).Replace(".", "");
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Token " + authToken);
                    client.DefaultRequestHeaders.Add("ContentType", "multipart/form-data");
                    MultipartFormDataContent multipartContent = new MultipartFormDataContent();
                    multipartContent.Add(new ByteArrayContent(fileContentArray1), "left.file", "doc1");
                    multipartContent.Add(new ByteArrayContent(fileContentArray2), "right.file", "doc2");
                    multipartContent.Add(new StringContent(fileType1), "left.file_type");
                    multipartContent.Add(new StringContent(fileType2), "right.file_type");
                    var response = client.PostAsync(baseUrl + "comparisons", multipartContent).Result;
                    var JsonContent = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == HttpStatusCode.Accepted | response.StatusCode == HttpStatusCode.OK | response.StatusCode == HttpStatusCode.Created)
                    {
                        var result = JsonContent.ToString();
                        string[] subs = result.Split(',');
                        string identifierRaw = subs[0];
                        string[] subs1 = identifierRaw.Split(':');
                        string identifier = subs1[1].Replace('"', ' ').Trim();
                        DateTime currentTime = DateTime.Now;
                        DateTime x30MinsLater = currentTime.AddMinutes(30);
                        string res = SignedViewerURL(identifier, x30MinsLater);
                        return new ContentResult
                        {
                            Content = res,
                            ContentType = "text/plain",
                            StatusCode = 200
                        };
                    }
                    else
                    {
                        return new ContentResult
                        {
                            Content = JsonContent,
                            ContentType = "text/json",
                            StatusCode = Convert.ToInt32(response.StatusCode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ContentResult
                {
                    Content = ex.ToString(),
                    ContentType = "text/plain",
                    StatusCode = 400
                };
            }
        }
        internal IEnumerable<byte[]> ByteArrayIntoBatches(byte[] bArray, int intBufforLengt)
        {
            int bArrayLenght = bArray.Length;
            byte[] bReturn = null;

            int i = 0;
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
        public static string SignedViewerURL(string identifier, DateTime validUntil, bool wait = false)
        {

            var baseURL = baseUrl + "comparisons/viewer/" + accountId + "/" + identifier;
            var validUntilTimestamp = ValidUntilTimestamp(validUntil);
            var signature = ViewerSignatureFor(accountId: accountId, authToken: authToken, identifier: identifier, validUntilTimestamp: validUntilTimestamp);

            return $"{baseURL}?valid_until={validUntilTimestamp}&signature={signature}{(wait ? "&wait" : "")}";
        }
        private static readonly DateTime UnixEpoch = new DateTime(year: 1970, month: 1, day: 1, hour: 0, minute: 0, second: 0, millisecond: 0, kind: DateTimeKind.Utc);

        public static int ValidUntilTimestamp(DateTime validUntil) => (int)(validUntil - UnixEpoch).TotalSeconds;
        public static string ViewerSignatureFor(string accountId, string authToken, string identifier, int validUntilTimestamp)
        {
            var jsonPolicy = JsonConvert.SerializeObject(new object[] {
                accountId,
                identifier,
                validUntilTimestamp,
            });

            return HMACHexDigest(authToken, jsonPolicy);
        }


        #region HMACHexDigest

        private static string HMACHexDigest(string key, string content) => Hexify(HMACDigest(key, content));


        private static byte[] HMACDigest(string key, string content)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(content));
            }
        }

        [CollectionAccess(CollectionAccessType.Read)]
        private static string Hexify([NotNull, InstantHandle] byte[] bytes)
        {
            const string hexDigits = "0123456789abcdef";
            var sb = new StringBuilder(2 * bytes.Length);
            foreach (var b in bytes)
            {
                sb.Append(hexDigits[b >> 4]);
                sb.Append(hexDigits[b & 0xF]);
            }
            return sb.ToString();
        }

        #endregion HMACHexDigest


    }
}
