// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

int _argLength = args.Length;

// Debug Args
// Issue here port could be something other than 80 or 443 so we need 
// a parameter here and also allow the port to be defined.
bool _runContinuous = false;
bool _isDebugMode = false;

string _ipUrl = "";
string _port = "";
bool _useSecure = true;


#if DEBUG
_isDebugMode = true;
//_ipUrl = "https://microsoft.com";
//_ipUrl = "https://fkapimanagementprod.management.azure-api.net/servicestatus";
_ipUrl = "https://zebpay-stg.management.azure-api.net:3443/servicestatus?api-version=2018-01-01";
//_ipUrl = "https://api-eco-prod.azure-api.net/ciam/ja_jp/loyalty-progress";
//_ipUrl = "https://apiutils.azure-api.net/status-0123456789abcdef";
//_ipUrl = "https://apiutils.developer.azure-api.net/internal-status-0123456789abcdef";
//_ipUrl = "https://apiutils.management.azure-api.net/servicestatus";
//_ipUrl = "https://104.215.148.63";
_runContinuous = true;

//_port = "443";
#else

_ipUrl = args[0].ToString().ToLower();


#endif

//Console.WriteLine(args.Length);
if (_argLength == 2)
{ 
    if(args[2].ToLower() == "c")
    {
        _runContinuous = true;
        await TestHttpConnection(_ipUrl, _runContinuous);
    }
    else
    {
        DisplayHelp();
        return;
    }
}
else if (_argLength == 1 || _isDebugMode)
{
    await TestHttpConnection(_ipUrl, _runContinuous);
}
else
{
    DisplayHelp();
}

Console.ReadKey();



static void DisplayMessage(string message, ConsoleColor color = ConsoleColor.White)
{
/*
    switch(mt)
    {
        case MessageType.Default:
            Console.ForegroundColor = ConsoleColor.White;
            break;

            case MessageType.Warning:
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            break;
            case MessageType.Error:
            Console.ForegroundColor = ConsoleColor.Red;

            break;

        case MessageType.Success:
            Console.ForegroundColor = ConsoleColor.Green;

            break;
    }
*/
    Console.ForegroundColor = color;
    Console.WriteLine(message);

    Console.ForegroundColor = ConsoleColor.White;
}

async static Task TestHttpConnection(string ipUrl, bool runContinuous)
{
    const int CONNECTIONTIMEOUT = 6;

    try
    {
        // validate a valid url, else return error and show help

        Uri _uriResult;// = new Uri(ipUrl);

        Uri.TryCreate(ipUrl, UriKind.RelativeOrAbsolute, out _uriResult);

        if(_uriResult == null || (_uriResult.Scheme != Uri.UriSchemeHttp && _uriResult.Scheme != Uri.UriSchemeHttps))
        {
            DisplayMessage($@"{ipUrl} is not a value Uri.", ConsoleColor.Red);
            DisplayHelp();

            return;
        }
      
        Stopwatch _sw = new Stopwatch();

        Console.WriteLine($@"HttpsPing has started for URL {ipUrl} - Time (Utc): {DateTime.UtcNow}");

        do
        {
            try
            { 
                _sw.Start();
                // what if there are multiple IP's for the hostname, does that matter to us?
                // handle all scenarios.
                HttpClient _client = new HttpClient();
                _client.Timeout = TimeSpan.FromSeconds(CONNECTIONTIMEOUT);
                var _request = new HttpRequestMessage(HttpMethod.Get, ipUrl);

                var _response = await _client.SendAsync(_request);

                _sw.Stop();

                if (_response.IsSuccessStatusCode)
                {
                    DisplayMessage($@"Connection to {ipUrl} successful. Time: {(_sw.ElapsedMilliseconds * 0.001).ToString("0.###")} secs", ConsoleColor.Green);
                }
                else
                {
                    if(_response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        DisplayMessage($@"Service is not available. If proxy service or Gateway, check it's request logs and http.sys logs(if Windows Service) {ipUrl} status code  {_response.StatusCode} returned. Time: {(_sw.ElapsedMilliseconds * 0.001).ToString("0.###")} secs", ConsoleColor.Red);
                    }
                    else
                    { 
                        DisplayMessage($@"Connection was made to {ipUrl} status code  {_response.StatusCode} returned. Time: {(_sw.ElapsedMilliseconds * 0.001).ToString("0.###")} secs", ConsoleColor.Yellow);
                    }
                }
            }
            catch(TaskCanceledException cex)
            {
                //The request was canceled due to the configured HttpClient.Timeout of 5 seconds elapsing.
                if (cex.Message.Contains("Timeout"))
                {
                    DisplayMessage($@"Connection to {ipUrl} timed out.  Default is set to {CONNECTIONTIMEOUT.ToString()} seconds.", ConsoleColor.Red);
                }
                else
                {
                    throw cex;
                }
            }
            catch(HttpRequestException reqex)
            {
                var _message = reqex.GetBaseException().Message;

                switch(_message)
                {
                    case string a when a.Contains("RemoteCertificateNameMismatch"):
                        DisplayMessage($"There is certificate issue with the URL: {ipUrl} \nPlease fix the cert issue or use correct URL.  Message: {_message}", ConsoleColor.Red);
                        
                        return;


                    default:

                        break;
                }

                //The remote certificate is invalid according to the validation procedure: RemoteCertificateNameMismatch
                Console.WriteLine($@"The following error occurred: {reqex.GetBaseException().Message}");
            }
            catch (Exception ex)
            {
                DisplayMessage($@"The following error occurred: {ex.GetBaseException().Message}", ConsoleColor.Red);
            }
            finally
            {
                _sw.Reset();
            }

            // here we could get back a 404, 401 or something which is ok
            // means there was a connection.

        } while (!Console.KeyAvailable && runContinuous);

    }
    catch (Exception ex)
    {
        DisplayMessage($@"The following error occurred: {ex.GetBaseException().Message}", ConsoleColor.Red);
        Console.WriteLine();
        DisplayHelp();
    }
}

    static void DisplayHelp()
    {
    // HttpsPing url/ip port <80 or 443>
    // Add the ability to specify get, post put etc.

    Console.WriteLine("HttpsPing Help (By Kris Frost)\n");
    Console.WriteLine("Usage:");
    Console.WriteLine("HttpsPing will make a http(s) connetion to let you know if traffic can reach the destination and the destination is servicing the requests. This provides a better test than TCP connection when testing Http services.\n");

    Console.WriteLine("Syntax:");
    Console.WriteLine("httpsping.exe <url or IP>  c (Optional and will do a continuous test.)");

        Console.WriteLine("Example:  httpsping.exe https://microsoft.com c\n");
    Console.WriteLine("Options:");
    Console.WriteLine("Option: <url or IP> IP Or url to test http(s) connectivity.  Use http or https with url or IP.  If you are using a different port, use :<port#>.");
        Console.WriteLine("Option: Different port exampel https://acme.com:4443");        
        Console.WriteLine("Option: c this is option and if c is present, will continually repeat the connection. Press a key to stop.");


    //Console.WriteLine("\r\n\n--- Test IP/FQDN & port with logging");
    //Console.WriteLine("\nSyntax: sox 192.168.0.1 80 -y or sox server.acme.com 80 -n \r\n\nFirst parameter:\tSpecify IP address or host name to connect to. \n\nSecond parameter:\tPort to connect to.  I.e. 80 for http.\n\nThird parameter:\tEnable event logging. -Y or -N\n\t\t\tIf enabled an warning message will be logged to the\n\t\t\tApplication Log.  Event ID 65535\n");

    //Console.WriteLine("\r\n\n--- Continuous Test IP/FQDN & port");
    //Console.WriteLine("\nSyntax: sox 192.168.0.1 80 -y -c or sox server.acme.com 80 -n -c \r\n\nFirst parameter:\tSpecify IP address or host name to connect to. \n\nSecond parameter:\tPort to connect to.  I.e. 80 for http.\n\nThird parameter:\tEnable event logging. -Y or -N\n\t\t\tIf enabled an warning message will be logged to the\n\t\t\tApplication Log.  Event ID 65535\n\nFourth Parameter: \tContinuous Ping -c ");

    //         Console.WriteLine("\r\n\nUse App.config to enable notifications");
}


/*
enum MessageType
{
    Default,
    Info,
    Success,
    Warning,
    Error
}
*/