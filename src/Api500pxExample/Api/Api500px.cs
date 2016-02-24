using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Linq;
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
        private readonly Uri _baseAddress = new Uri("https://api.500px.com/v1");
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
                await RequestToken();
                await AuthorizeToken();
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

            AuthorizationParameters = new Dictionary<string, string>()
                                                                     {
                                                                         {OauthParameter.OauthCallback, CallbackUrl},
                                                                         {OauthParameter.OauthConsumerKey, ConsumerKey},
                                                                         {OauthParameter.OauthNonce, GetNonce()},
                                                                         {OauthParameter.OauthSignatureMethod,OAuthSignatureMethod},
                                                                         {OauthParameter.OauthTimestamp, GetTimeStamp()},
                                                                         {OauthParameter.OauthVersion, OAuthVersion}
                                                                     };

            var signature = GetSignature(sigBaseString);

            var resourceUri = RequestTokenUrl + "?" + sigBaseStringParams + "&oauth_signature=" + Uri.EscapeDataString(signature);
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(new Uri(resourceUri));

            string requestToken = null;
            var keyValPairs = response.Split('&');

            foreach (var t in keyValPairs)
            {
                var splits = t.Split('=');
                switch (splits[0])
                {
                    case "oauth_token":
                        requestToken = splits[1];
                        break;
                    case "oauth_token_secret":
                        break;
                }
            }

            return requestToken;
        }

        private async Task<OauthToken> AuthorizeToken()
        {

        }

        private async Task<OauthToken> AccessToken()
        {

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

        private string GetSignature(string Url, string tokenSecret, string requestType)
        {
            var SigBaseStringParams = string.Join("&", AuthorizationParameters.Select(key => key.Key + "=" + Uri.EscapeDataString(key.Value)));
            var SigBaseString = requestType + "&";
            SigBaseString += Uri.EscapeDataString(Url) + "&" + Uri.EscapeDataString(SigBaseStringParams);

            IBuffer KeyMaterial = CryptographicBuffer.ConvertStringToBinary(Constants.ConsumerSecret + "&" + tokenSecret, BinaryStringEncoding.Utf8);
            MacAlgorithmProvider HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm(OAuthSignatureMethodName);
            CryptographicKey MacKey = HmacSha1Provider.CreateKey(KeyMaterial);
            IBuffer DataToBeSigned = CryptographicBuffer.ConvertStringToBinary(SigBaseString, BinaryStringEncoding.Utf8);
            IBuffer SignatureBuffer = CryptographicEngine.Sign(MacKey, DataToBeSigned);
            String Signature = CryptographicBuffer.EncodeToBase64String(SignatureBuffer);
            AuthorizationParameters.Add(OauthParameter.OauthSignature, Signature);
            return this;
        }
    }
}
