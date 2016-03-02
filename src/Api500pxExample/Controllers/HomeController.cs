using System;
using System.Threading.Tasks;
using Api500pxExample.Api;
using Api500pxExample.Api.Models;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;

namespace Api500pxExample.Controllers
{
    public class HomeController : Controller
    {
        #region private methods
        private void SaveToken(string key, OauthToken token)
        {
            HttpContext.Response.Cookies.Append(key + ".Token", token.Token ?? string.Empty);
            HttpContext.Response.Cookies.Append(key + ".Secret", token.Secret ?? string.Empty);
            HttpContext.Response.Cookies.Append(key + ".Verifier", token.Verifier ?? string.Empty);
        }

        private OauthToken LoadToken(string key)
        {
            return new OauthToken()
            {
                Token = HttpContext.Request.Cookies[key + ".Token"],
                Secret = HttpContext.Request.Cookies[key + ".Secret"],
                Verifier = HttpContext.Request.Cookies[key + ".Verifier"]
            };
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            var accessToken = LoadToken("AccessToken");
            ViewBag.IsAuthenticated = !string.IsNullOrEmpty(accessToken.Token);
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
            var service = new Api500px(LoadToken("AccessToken"));
            ViewData.Model = await service.Photos("feature=popular&image_size=4&sort=rating");
            return View("Photos");
        }

        public async Task<ActionResult> Search()
        {
            var service = new Api500px(LoadToken("AccessToken"));
            ViewData.Model = await service.Search("term=inspire&rpp=30");
            return View("Photos");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
