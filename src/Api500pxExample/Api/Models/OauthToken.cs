namespace Api500pxExample.Api.Models
{
    public class OauthToken
    {
        public string Token { get; set; }
        public string SecretCode { get; set; }
        public string Verifier { get; set; }
    }
}
