using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Api500pxExample.Api.Models;
using Newtonsoft.Json;
using Api500pxExample.Api.Contracts;

// ReSharper disable InconsistentNaming

namespace Api500pxExample.Api
{
    /// <summary>
    /// https://apigee.com/vova/embed/console/api500px
    /// OAuth 1.0a
    ///
    /// Good ideas here: http://www.rahulpnath.com/blog/exploring-oauth-c-and-500px/
    /// To check the signature: http://oauth.googlecode.com/svn/code/javascript/example/signature.html
    /// </summary>
    public class Api500px 
    {
        #region Private Constants
        private const string AccessUrl = "https://api.500px.com/v1/oauth/access_token";
        private const string AuthorizeUrl = "https://api.500px.com/v1/oauth/authorize";
        private const string RequestTokenUrl = "https://api.500px.com/v1/oauth/request_token";
        private const string OAuthSignatureMethod = "HMAC-SHA1";
        private const string OAuthVersion = "1.0";
        #endregion

        #region Private Fields
        private Dictionary<string, string> AuthorizationParameters;
        #endregion

        #region Private Methods        
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

        private Api500px Sign(string url, string tokenSecret1, string tokenSecret2, string requestType, string parameters)
        {
            var signatureParams = string.Join("&", AuthorizationParameters.Select(key => key.Key + "=" + Uri.EscapeDataString(key.Value)));
            var signatureBase = requestType + "&";

            if (string.IsNullOrEmpty(parameters))
            {
                signatureBase += Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(signatureParams);
            }
            else
            {
                signatureBase += Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(parameters + "&" + signatureParams); 
            }
        
            var hash = GetHash(tokenSecret1, tokenSecret2);
            var dataBuffer = Encoding.ASCII.GetBytes(signatureBase);
            var hashBytes = hash.ComputeHash(dataBuffer);

            AuthorizationParameters.Add(OauthParameter.OauthSignature, Convert.ToBase64String(hashBytes));
            return this;
        }

        private HashAlgorithm GetHash(string tokenSecret1, string tokenSecret2)
        {
            if (OAuthSignatureMethod != "HMAC-SHA1") throw new NotImplementedException();

            var keystring = $"{Uri.EscapeDataString(tokenSecret1)}&{Uri.EscapeDataString(tokenSecret2)}";

            var hmacsha1 = new HMACSHA1
            {
                Key = Encoding.ASCII.GetBytes(keystring)
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

        private async Task<HttpResponseMessage> GetRequest(string url, string parameters)
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
                return await client.GetAsync(new Uri(url + "?" + parameters, UriKind.Absolute));
            }
        }

        private OauthToken ParseReponse(string response)
        {
            var token = new OauthToken();
            if (string.IsNullOrEmpty(response)) return token;

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
                        token.Secret = splits[1];
                        break;
                }
            }

            return token;
        }

        private static async Task<T> DeserializeResponse<T>(HttpResponseMessage httpResponse) where T : Response, new()
        {
            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<T>(content) ?? new T();

            response.Content = content;
            response.IsSuccessStatusCode = httpResponse.IsSuccessStatusCode;
            response.StatusCode = httpResponse.StatusCode;

            if (!httpResponse.IsSuccessStatusCode)
            {
             //   Debug.WriteLine("HttpResponseMessage failed: " + httpResponse + "\r\n -- Content: " + content + ((!string.IsNullOrWhiteSpace(response.Error)) ? "\r\n -- Error: " + response.Error : string.Empty));
            }

            return response;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// All subsequent request to any protected resource needs the AccessToken and should be 
        /// signed using ConsumerKey and the access token's secret code.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<GetPhotosResponse> Popular(OauthToken token)
        {
            AuthorizationParameters = new Dictionary<string, string>()
            {
                {OauthParameter.OauthConsumerKey, Constants.ConsumerKey},
                {OauthParameter.OauthNonce, GetNonce()},
                {OauthParameter.OauthSignatureMethod, OAuthSignatureMethod},
                {OauthParameter.OauthTimestamp, GetTimeStamp()},
                {OauthParameter.OauthToken, token.Token},
                {OauthParameter.OauthVersion, OAuthVersion}
            };

            var url = "https://api.500px.com/v1/photos";
            var parameters = "feature=popular&image_size=4";
            var response = await Sign(url, Constants.ConsumerSecret, token.Secret, "GET", parameters).GetRequest(url, parameters);
            return await DeserializeResponse<GetPhotosResponse>(response);
        }

        public async Task<OauthToken> GetRequestToken()
        {
            AuthorizationParameters = new Dictionary<string, string>()
            {
                {OauthParameter.OauthCallback, Constants.CallbackUrl},
                {OauthParameter.OauthConsumerKey, Constants.ConsumerKey},
                {OauthParameter.OauthNonce, GetNonce()},
                {OauthParameter.OauthSignatureMethod, OAuthSignatureMethod},
                {OauthParameter.OauthTimestamp, GetTimeStamp()},
                {OauthParameter.OauthVersion, OAuthVersion}
            };

            var response = await Sign(RequestTokenUrl, Constants.ConsumerSecret, string.Empty, "POST", "").PostRequest(RequestTokenUrl);
            return ParseReponse(response);
        }

        /// <summary>
        /// A callback is needed to get back oauth_token and oauth_verifier
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string GetAuthorizationUrl(OauthToken token)
        {
            return AuthorizeUrl + "?oauth_token=" + token.Token;
        }

        public async Task<OauthToken> GetAccessToken(OauthToken token)
        {
            AuthorizationParameters = new Dictionary<string, string>()
            {
                {OauthParameter.OauthConsumerKey, Constants.ConsumerKey},
                {OauthParameter.OauthNonce, GetNonce()},
                {OauthParameter.OauthSignatureMethod, OAuthSignatureMethod},
                {OauthParameter.OauthTimestamp, GetTimeStamp()},
                {OauthParameter.OauthToken, token.Token},
                {OauthParameter.OauthVerifier, token.Verifier},
                {OauthParameter.OauthVersion, OAuthVersion}
             };

            var response = await Sign(AccessUrl, Constants.ConsumerSecret, token.Secret, "POST", "").PostRequest(AccessUrl);
            return ParseReponse(response);
        }
        #endregion  
    }
}
