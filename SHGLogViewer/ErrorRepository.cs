using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHGLogViewer
{
    public static class ErrorRepository
    {
        static Dictionary<string, string> _recommendations = new Dictionary<string, string>();

        static ErrorRepository()
        {

        }

        /// <summary>
        /// // This is a place holder to start storing recommendations.  If it grows, should be moved to a data respository. Purely
        /// to save time starting out.
        /// Right now, we're just looking at status code but designed in case we need to parse other fields as well such as exception or messages.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static string CheckForRecommendations(string statusCode)
        {
            statusCode = statusCode.ToLower();

            StringBuilder _sb = new StringBuilder();

            if(!_recommendations.ContainsKey(statusCode))
            {
                _recommendations.Add(statusCode, StatusCodeRecommendations(statusCode));
            }

            _sb.AppendLine(_recommendations[statusCode]);

            return _sb.ToString();
        }


        /// <summary>
        /// This will be for Status Codes
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        static string StatusCodeRecommendations(string statusCode)
        {
            StringBuilder _sb = new StringBuilder();

            switch (statusCode.ToLower())
            {
                //case string un when un.Contains("unauthorized"):
                case ("unauthorized"):
                    _sb.AppendLine($@"Recommendation:  Unauthorized typically is caused by stale Token.  Tokens must be refreshed every 30 days \r\n 
https://docs.microsoft.com/en-us/azure/api-management/how-to-self-hosted-gateway-on-kubernetes-in-production#access-token");

                    break;


            }

            return _sb.ToString();
        }

       
    }


}
