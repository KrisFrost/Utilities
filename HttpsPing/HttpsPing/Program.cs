    // See https://aka.ms/new-console-template for more information
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
_ipUrl = "microsoft.com";
//_port = "443";
#else
        _ipUrl = args[0].ToString().ToLower();

        if(_argLength > 3)
        
        // No use for checking for hs, we're going to do one or the other
        if(_args[2].ToLower() == "h")
        {
            _useSecure = false;
        }
        else if (_args[2].ToLower() != hs


#endif

if (_argLength == 3)
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
else if (_argLength == 2 || _isDebugMode)
{
    await TestHttpConnection(_ipUrl, _runContinuous);
}
else
{
    DisplayHelp();
}

Console.ReadKey();

async static Task TestHttpConnection(string ipUrl, bool runContinuous)
{
    try
    {   
        // validate a valid url, else return error and show help
      
        Uri _uriResult = new Uri(ipUrl);

        Uri.TryCreate(ipUrl, UriKind.RelativeOrAbsolute, out _uriResult);

        if(_uriResult == null || (_uriResult.Scheme != Uri.UriSchemeHttp || _uriResult.Scheme != Uri.UriSchemeHttps))
        {
            Console.WriteLine($@"{ipUrl} is not a value Uri.");
            DisplayHelp();
        }
      

        do
        {
            HttpClient _client = new HttpClient();
            var _request = new HttpRequestMessage(HttpMethod.Get, ipUrl);

            await _client.SendAsync(_request);

            // here we could get back a 404, 401 or something which is ok
            // means there was a connection.

        } while (!Console.KeyAvailable && runContinuous);

    }
    catch (Exception ex)
    {
        Console.WriteLine($@"The following error occurred: {ex.GetBaseException().Message}");
        Console.WriteLine();
        DisplayHelp();
    }
}

    static void DisplayHelp()
    {
    // HttpsPing url/ip port <80 or 443>

    Console.WriteLine("HttpsPing Help\n");
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