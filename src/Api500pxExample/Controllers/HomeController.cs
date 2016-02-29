using System;
using System.Threading.Tasks;
using Api500pxExample.Api;
using Api500pxExample.Api.Models;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;

namespace Api500pxExample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private void SaveToken(string key, OauthToken token)
        {
            HttpContext.Session.SetString(key + ".Token", token.Token ?? string.Empty);
            HttpContext.Session.SetString(key + ".Secret", token.Secret ?? string.Empty);
            HttpContext.Session.SetString(key + ".Verifier", token.Verifier ?? string.Empty);
        }

        private OauthToken LoadToken(string key)
        {
            return new OauthToken()
            {
                Token = HttpContext.Session.GetString(key + ".Token"),
                Secret = HttpContext.Session.GetString(key + ".Secret"),
                Verifier = HttpContext.Session.GetString(key + ".Verifier")
            };
        }

        public async Task<ActionResult> Authenticate()
        {
            var service = new Api500px();
            var token = await service.GetRequestToken();
            SaveToken("RequestToken", token);

            var uri = service.GetAuthorizationUrl(token);
            
            return new RedirectResult(uri);
        }

        public async Task<ActionResult> Callback(string oauth_token, string oauth_verifier)
        {
            var service = new Api500px();
            var requestToken = LoadToken("RequestToken");
            var accessToken = await service.GetAccessToken(new OauthToken() {Token = oauth_token, Secret = requestToken.Secret, Verifier = oauth_verifier});

            if ((accessToken != null) && (!string.IsNullOrEmpty(accessToken.Token)))
            {
                SaveToken("AccessToken", accessToken);
                ViewBag.IsAuthenticated = 1;
            }
            else
            {
                ViewBag.IsAuthenticated = 0;
            }

            return View("Index");
        }


        public async Task<ActionResult> Popular()
        {
            var service = new Api500px();
            var accessToken = LoadToken("AccessToken");
            ViewData.Model = await service.Popular(accessToken);
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
