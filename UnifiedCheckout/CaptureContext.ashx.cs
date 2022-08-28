using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Net;
namespace UnifiedCheckout {
    /// <summary>
    /// Summary description for CaptureContext
    /// </summary>
    public class CaptureContext : IHttpHandler {
        private readonly string requestHost = "apitest.cybersource.com";
        private readonly string merchantId = "testrest";
        private readonly string merchantKeyId = "08c94330-f618-42a3-b09d-e1e43be5efda";
        private readonly string merchantSecretKey = "yBJxy6LjM2TmcPGu+GaJrHtkke25fPpUX+UY6/L/1tE=";
        private readonly string resource = "/up/v1/capture-contexts";
        private readonly DateTime requestDate = DateTime.UtcNow;
        private string GenerateDigest(string payload) {
            var buffer = Encoding.UTF8.GetBytes(payload);
            using (SHA256 sha256 = SHA256.Create()) {
                var hash = sha256.ComputeHash(buffer);
                string digest = Convert.ToBase64String(hash, 0, hash.Length);
                return digest;
            }
        }
        private string GenerateSignature(string payload) {
            StringBuilder signatureBuilder = new StringBuilder();
            
            signatureBuilder.Append("keyid=\"" + merchantKeyId + "\",");
            signatureBuilder.Append("algorithm=\"HmacSHA256\",");
            string headersForPostMethod = "host date (request-target) digest v-c-merchant-id";
            signatureBuilder.Append("headers=\"" + headersForPostMethod + "\"");

            StringBuilder signatureString = new StringBuilder();
            signatureString.Append("host: "+requestHost);
            signatureString.Append("\n date:" + requestDate);
            string targetUrlForPost = "post " + resource;
            signatureString.Append("\n (request-target): " + targetUrlForPost);
            signatureString.Append("\n digest: SHA-256=" + GenerateDigest(payload));
            signatureString.Append("\n v-c-merchant-id: " + merchantId);

            var data = Encoding.UTF8.GetBytes(signatureString.ToString());
            var key = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(merchantSecretKey)));

            using (HMACSHA256 hmacsha256 = new HMACSHA256(key)) {
                var sighash = hmacsha256.ComputeHash(data);
                string signatureValue = Convert.ToBase64String(sighash);
                signatureBuilder.Append(",signature=\"" + signatureValue + "\"");
            }
                return signatureBuilder.ToString();
        }
        private string GetRequestPayload() {
            string jsonStr = "";
            using (StreamReader reader = new StreamReader(HttpContext.Current.Request.InputStream)) {
                jsonStr = reader.ReadToEnd();
            }
           return jsonStr;
        }
        public void ProcessRequest(HttpContext context) {
            string payload = GetRequestPayload();

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://" + requestHost + resource);
            
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("v-c-merchant-id", merchantId);
            headers.Add("signature", GenerateSignature(payload));
            headers.Add("digest", "SHA-256="+GenerateDigest(payload));
            httpWebRequest.Headers = headers;

            httpWebRequest.Host = requestHost;
            httpWebRequest.Date = requestDate;
            httpWebRequest.UserAgent = "Mozilla/5.0";
            httpWebRequest.Method = "POST";

            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Accept = "application/jwt";


            using (StreamWriter writer = new StreamWriter(httpWebRequest.GetRequestStream())) {
                writer.Write(payload);
                writer.Flush();
                writer.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            context.Response.ContentType = "application/jwt";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                context.Response.Write(streamReader.ReadToEnd());
            }
          
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}