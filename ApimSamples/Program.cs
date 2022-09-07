using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ApimSamples
{    
    internal class Program
    {
        // DO NOT SHARE THIS KEY IN CODE TO ANY TYPE OF PRODUCTION or BUSINESS related 
        // API as it will allow complete access.  This key is for testing only to a specific
        // generic API.  Please use the API you're testing and Tracing must be enabled for it to work.
        //https://krisfrost.net/2022/09/07/azure-apim-inspector-trace-c-postman/
        internal const string APIMSUBSCRIPTIONKEY = "6089327acfe542fda59898bc0972dc92";

        // Use a URL to an APIM API operation.
        const string _webApiUrl = "https://apiutils.azure-api.net/Conference/topics?dayno=1";

        static void Main(string[] args)
        {
            /*
            ServicePointManager.ServerCertificateValidationCallback =
            delegate (object sender, X509Certificate certificate, X509Chain
            chain, SslPolicyErrors sslPolicyErrors)
            {
               return true;
            };
            */

            GenerateInspectorTrace().Wait();

            Console.WriteLine("Hello, World!");
        }

        static async Task GenerateInspectorTrace()
        {
            var _handler = new HttpClientHandler();

            // add a custom certificate validation callback to the handler
            _handler.ServerCertificateCustomValidationCallback = ((sender, cert, chain, errors) => ValidateCert(sender, cert, chain, errors));

            var _httpClient = new HttpClient(_handler);

            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", APIMSUBSCRIPTIONKEY);
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Trace", "true");

            HttpResponseMessage _response = await _httpClient.GetAsync(_webApiUrl);

            Console.WriteLine($@"Response Code: {_response.StatusCode}");

            var _traceHeader = _response.Headers.SingleOrDefault(w => w.Key.ToLower().Contains("ocp-apim-trace-location"));

            if(_traceHeader.Key != null)
            {
                OpenUrl(_traceHeader.Value.First());
            }

        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                  //  url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        protected static bool ValidateCert(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {

            //return true;
            // set a list of servers for which cert validation errors will be ignored
            // These are the certs we allow to be overriden
            var overrideCerts = new string[]
            {
                "localhost"
            };

            // if the server is in the override list, then ignore any validation errors
            var serverName = cert.Subject.ToLower();
            if (overrideCerts.Any(overrideName => serverName.Contains(overrideName))) return true;

            // otherwise use the standard validation results
            return errors == SslPolicyErrors.None;
        }
    }
}