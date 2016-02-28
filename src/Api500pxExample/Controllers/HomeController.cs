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

        private void SaveToken(OauthToken token)
        {
            HttpContext.Session.SetString("Token", token.Token ?? string.Empty);
            HttpContext.Session.SetString("Secret", token.Secret ?? string.Empty);
            HttpContext.Session.SetString("Verifier", token.Verifier ?? string.Empty);
        }

        private OauthToken LoadToken()
        {
            return new OauthToken()
            {
                Token = HttpContext.Session.GetString("Token"),
                Secret = HttpContext.Session.GetString("Secret"),
                Verifier = HttpContext.Session.GetString("Verifier")
            };
        }

        public async Task<ActionResult> Authenticate()
        {
            var service = new Api500px();
            var token = await service.GetRequestToken();
            SaveToken(token);

            var uri = service.GetAuthorizationUrl(token);
            
            return new RedirectResult(uri);
        }

        public async Task<ActionResult> Callback(string oauth_token, string oauth_verifier)
        {
            var service = new Api500px();

            var requestToken = LoadToken();
            var accessToken = await service.GetAccessToken(new OauthToken() {Token = oauth_token, Secret = requestToken.Secret, Verifier = oauth_verifier});

            await service.Popular(accessToken);
            return View();
        }


        public async Task<RedirectResult> Popular()
        {
            var service = new Api500px();

            //ViewData["Message"] = "Popular Photos";
            //ViewData.Model = await service.Popular();
            //return Redirect("https://api.500px.com/v1/oauth/authorize");

            return null;
            //return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
