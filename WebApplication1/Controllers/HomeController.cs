using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Metadata;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        string vaultURL = string.Empty;
        string ClientSecretName = string.Empty;
        string SharepointSecret = string.Empty;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            string ReadFromAppSetting = _configuration["WebApp_readFromAppSetting"];
            if (ReadFromAppSetting == "false")
            {
                vaultURL = _configuration["AzureAD_App_vaultURL"];
                ClientSecretName = _configuration["AzureAD_App_clientSecretName"];
                SharepointSecret = _configuration["Sharepoint_App_clientSecretName"];
            }
            else if (ReadFromAppSetting == "true")
            {
                vaultURL = _configuration["AzureAD_App_vault_URL"];
                ClientSecretName = _configuration["AzureAD_App_clientSecret_Name"];
                SharepointSecret = _configuration["Sharepoint_App_clientSecret_Name"];
            }
        }
        //List<Auth_List> authList = new List<Auth_List> { };

        public async Task<IActionResult> Index()
        {
            string authCode = HttpContext.Request.Query["code"];
            string userstate = HttpContext.Request.Query["state"];
            if (string.IsNullOrEmpty(Def_Values.client_Secret))
            {
                Def_Values.client_Secret = await getSecret(ClientSecretName);
            }
            if (string.IsNullOrEmpty(Def_Values.sharepoint_Secret))
            {
                Def_Values.sharepoint_Secret = await getSecret(SharepointSecret);
            }
            if (!string.IsNullOrEmpty(authCode))
            {
                if (Def_Values.authList.Count > 0)
                {
                    var data = Def_Values.authList.FirstOrDefault(x => x.state == userstate);
                    if (data != null)
                    {
                        data.code = authCode;
                    }
                    else
                    {
                        Def_Values.authList.Add(new Auth_List(userstate, authCode, DateTime.Now));
                    }
                }
                else
                {
                    Def_Values.authList.Add(new Auth_List(userstate, authCode, DateTime.Now));
                }
            }
            return View();
        }
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
        public IActionResult Privacy()
        {
            return View();
        }


        public string GetClientSecret()
        {
            string _testvalue = Def_Values.client_Secret;
            return _testvalue;
        }
        public string GetSharepointtSecret()
        {
            string _testvalue = Def_Values.sharepoint_Secret;
            return _testvalue;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}