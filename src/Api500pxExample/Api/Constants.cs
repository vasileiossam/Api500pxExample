using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api500pxExample.Api
{
    public class Constants
    {
        public const string ConsumerKey = "<ENTER YOUR CONSUMER KEY HERE>";
        public const string ConsumerSecret = "<ENTER YOUR CONSUMER SECRET HERE>";
        
        private const string AccessUrl = "https://api.500px.com/v1/oauth/access_token";
        private const string AuthorizeUrl = "https://api.500px.com/v1/oauth/authorize";
        private const string RequestTokenUrl = "https://api.500px.com/v1/oauth/request_token";
        private const string OAuthSignatureMethod = "HMAC-SHA1";
        private const string OAuthVersion = "1.0";
    }
}
 