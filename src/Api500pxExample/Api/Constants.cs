using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api500pxExample.Api
{
    public class Constants
    {
        public const string ConsumerKey = " 63x9cuRYAJyxE5prc0VbFjjbse51EsyJizgixal0";
        public const string ConsumerSecret = " iF7uMD1mu8DINttVybyRPuzgpc8GzUtGbsp0lLPD";
        
        private const string AccessUrl = "https://api.500px.com/v1/oauth/access_token";
        private const string AuthorizeUrl = "https://api.500px.com/v1/oauth/authorize";
        private const string RequestTokenUrl = "https://api.500px.com/v1/oauth/request_token";
        private const string OAuthSignatureMethod = "HMAC-SHA1";
        private const string OAuthVersion = "1.0";
    }
}
