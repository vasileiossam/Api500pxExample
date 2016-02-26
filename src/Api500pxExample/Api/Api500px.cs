using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Api500pxExample.Api.Models;

// ReSharper disable InconsistentNaming

namespace Api500pxExample.Api
{
    /// <summary>
    /// https://apigee.com/vova/embed/console/api500px
    /// OAuth 1.0a
    /// </summary>
    public class Api500px //: IApi500px
    {
        private const string AccessUrl = "https://api.500px.com/v1/oauth/access_token";
        private const string AuthorizeUrl = "https://api.500px.com/v1/oauth/authorize";
        private const string RequestTokenUrl = "https://api.500px.com/v1/oauth/request_token";
        private const string OAuthSignatureMethod = "HMAC-SHA1";
        private const string OAuthVersion = "1.0";
        private const string CallbackUrl = "http://www.google.com";
        private Dictionary<string, string> AuthorizationParameters;

        //public Task<List<Photo>> Photos(Features? feature, string user_id, string username, string category_include, string category_exclude,
        //    string sort, int? page, int? rpp, string image_size, IncludeStores include_store, IncludeStates include_states,
        //    string tags)
        //{
        //    using (var client = new HttpClient {BaseAddress = _baseAddress})
        //    {
        //        var resourceUri = GetUriWithAccessToken("community_shares");
        //        var response = await client.GetAsync(resourceUri);

        //    }
        //}

        public async Task<List<Photo>> Popular()
        {
            var list = new List<Photo>();
            if (!await Authenticate()) return list;

            list.Add(new Photo() { Name = "a" });
            return list;
        }

        private async Task<bool> Authenticate()
        {
            try
            {
                var token = await RequestToken();
                await AuthorizeToken(token);
                await AccessToken();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<OauthToken> RequestToken()
        {
            var token = new OauthToken();

            AuthorizationParameters = new Dictionary<string, string>()
                                                                     {
                                                                         {OauthParameter.OauthCallback, CallbackUrl},
                                                                         {OauthParameter.OauthConsumerKey, Constants.ConsumerKey},
                                                                         {OauthParameter.OauthNonce, GetNonce()},
                                                                         {OauthParameter.OauthSignatureMethod, OAuthSignatureMethod},
                                                                         {OauthParameter.OauthTimestamp, GetTimeStamp()},
                                                                         {OauthParameter.OauthVersion, OAuthVersion}
                                                                     };

            var response = await Sign(RequestTokenUrl, string.Empty, "POST").PostRequest(RequestTokenUrl);

            if (!string.IsNullOrEmpty(response))
            {
                //  "oauth_token=qWnfRDBG47T57ud3peQeYtWHjoGSlbYtAwboQZuD&oauth_token_secret=iZJG3SKMA8BJexk4iEtQTwZmbstfR1TqoY1yKBo6&oauth_callback_confirmed=true"    string
                var keyValPairs = response.Split('&');
                foreach (var splits in keyValPairs.Select(t => t.Split('=')))
                {
                    switch (splits[0])
                    {
                        case "oauth_token":
                            token.Token = splits[1];
                            break;
                        case "oauth_token_secret":
                            token.SecretCode = splits[1];
                            break;
                    }
                }
            }

            return token;
        }

        private async Task<OauthToken> AuthorizeToken(OauthToken token)
        {
            var tempAuthorizeUrl = AuthorizeUrl + "?oauth_token=" + token.Token;

            var StartUri = new Uri(tempAuthorizeUrl);
            var EndUri = new Uri(CallbackUrl);

            var auth =
                await
                WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, StartUri, EndUri);

            //var responseData = auth.ResponseData;
            //responseData = responseData.Replace("http://www.bing.com/?", "");
            //var split = responseData.Split('&');
            //var keyValue = split[1].Split('=');
            //Token.Verifier = keyValue[1];
            //return Token;

            return null;
        }

        private async Task<OauthToken> AccessToken()
        {
            return null;
        }

        private static string GetNonce()
        {
            var rand = new Random();
            var nonce = rand.Next(1000000000);
            return nonce.ToString();
        }

        private static string GetTimeStamp()
        {
            var sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Math.Round(sinceEpoch.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private Api500px Sign(string uri, string tokenSecret, string requestType)
        {
            var signatureParams = string.Join("&", AuthorizationParameters.Select(key => key.Key + "=" + Uri.EscapeDataString(key.Value)));
            var signatureBase = requestType + "&";
            signatureBase += Uri.EscapeDataString(uri) + "&" + Uri.EscapeDataString(signatureParams);

            var hash = GetHash(tokenSecret);
            var dataBuffer = System.Text.Encoding.ASCII.GetBytes(signatureBase);
            var hashBytes = hash.ComputeHash(dataBuffer);

            AuthorizationParameters.Add(OauthParameter.OauthSignature, Convert.ToBase64String(hashBytes));
            return this;
        }

        private HashAlgorithm GetHash(string tokenSecret)
        {
            if (OAuthSignatureMethod != "HMAC-SHA1") throw new NotImplementedException();

            var keystring = string.Format("{0}&{1}", Constants.ConsumerSecret, tokenSecret);

            var hmacsha1 = new HMACSHA1
            {
                Key = System.Text.Encoding.ASCII.GetBytes(keystring)
            };
            return hmacsha1;
        }

        private async Task<string> PostRequest(string url)
        {
            var oauthString = string.Empty;
            if (AuthorizationParameters != null)
            {
                oauthString = string.Join(", ",
                                               AuthorizationParameters.Select(
                                                   key =>
                                                   key.Key +
                                                   (string.IsNullOrEmpty(key.Value)
                                                        ? string.Empty
                                                        : "=\"" + Uri.EscapeDataString(key.Value) + "\"")));
                AuthorizationParameters.Clear();
            }


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", oauthString);
                var response = await client.PostAsync(new Uri(url, UriKind.Absolute), null);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }

            return string.Empty;
        }
    }
}
