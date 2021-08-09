using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sox2
{
    class Program
    {
        public static bool _eventlog;
        public const string EVENTSOURCE = "Sox Util(3rd Party)";
        private static bool _enableSmtpNotifications = false;
        private static bool _enableSmsNotifications = false;
        static string _smtpServer;
        static string _smsServer;
        static string _smtpTos;
        static string _smtpSubject;
        static string _smtpUserName;
        static string _smtpPassword;
        static string _smsTos;

        /*
        <add key = "EnableSmtpFailureNotifications" value="true"/>
		<add key = "EnableSmsFailureNotifications" value="true"/>
		<add key = "SMTPServer" value="yourserverhere"/>
		<add key = "SMTPUserName" value="username(notrequired)"/>
		<add key = "SMTPPassword" value="NotRequired"/>
		<add key = "SMTPTos" value="commadelimitedemailaddresses"/>
		<add key = "SMTPSubject" value="SubjectHere"/>		
		<add key = "SMSServer" value="pathtoServer"/>
		<add key = "SMSTos" value="CommaDelimtedPhone#'s"/>
        */

        static void Main(string[] args)
        {
            //_enableSmtpNotifications = bool.Parse(ConfigurationManager.AppSettings["EnableSmtpFailureNotifications"]);
            //_enableSmsNotifications = bool.Parse(ConfigurationManager.AppSettings["EnableSmsFailureNotifications"]);

            _eventlog = false;
            bool _continuousTCP = false;

            int _argLength = args.Length;

            if(_argLength == 2)
            {
                TestAzureServices(args[0], args[1]);

            }
            else if (_argLength == 3 || _argLength == 4)
            {
            
                if (args[2][1] == 'y' || args[2][1] == 'Y')
                {
                    _eventlog = true;
                }

                if(_argLength == 4)
                {
                    _continuousTCP = true;
                }

                // If continuous, need to continue until ended.
                // If Not continuous, need to run once

                do
                {
                    // kkf, may have to change these settings
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    try
                    {
                        try
                        {
                            IPAddress addressList = Dns.GetHostByName(args[0]).AddressList[0];

                            IPEndPoint pEndPoint = new IPEndPoint(addressList, Convert.ToInt32(args[1]));
                            SoxCon(pEndPoint, socket).Wait();
                        }
                        catch (ArgumentNullException argumentNullException)
                        {
                            Console.WriteLine(string.Concat("ArgumentNullException : ", argumentNullException.GetBaseException().Message));
                        }
                        catch (SocketException socketException)
                        {
                            Console.WriteLine(string.Concat("SocketException : ", socketException.GetBaseException().Message));
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(string.Concat("Unexpected exception : ", exception.GetBaseException().Message));
                        }
                    }
                    finally
                    {
                        socket.Close();
                    }

                } while (!Console.KeyAvailable &&  _continuousTCP);

                
            }
            else
            { 
                DispHelp();
            }
            

            Console.WriteLine("Press space bar to end.");
            Console.ReadKey();
        }

        private static void TestAzureServices(string arg1, string arg2)
        {
            switch(arg1)
            {
                case string apim when apim.ToLower().Contains("apim"):
                    TestApimConnectivity(arg2);
                    break;


                default:
                    Console.WriteLine("Invalid command");
                    break;
            }
        }

        private async static void TestApimConnectivity(string arg2)
        {
            Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                try
                {
                    int _port = 3443;

                    IPAddress addressList = Dns.GetHostEntry(arg2).AddressList[0];
                    // First test Portal & Power Shell
                    IPEndPoint pEndPoint = new IPEndPoint(addressList,  _port);

                    try
                    {
                     //   System.Diagnostics.Debugger.Launch();

                        await SoxCon(pEndPoint, _socket);
                       // Console.WriteLine(string.Format("Connection to Port {0}  passed. ", _port));
                    }
                    catch (SocketException socketException)
                    {
                        Console.WriteLine(string.Format("SocketException : ", socketException.GetBaseException().Message));
                    }

                    try
                    {
                        _port = 80;
                        //IPEndPoint pEndPoint2 = new IPEndPoint(addressList, _port);
                        pEndPoint = new IPEndPoint(addressList, _port);
                        await SoxCon (pEndPoint, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                    }
                    catch (SocketException socketException)
                    {
                        Console.WriteLine(string.Format("SocketException : ", socketException.GetBaseException().Message));
                    }

                    try
                    {
                        _port = 443;
                        pEndPoint = new IPEndPoint(addressList, _port);
                        await SoxCon (pEndPoint, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                    }
                    catch (SocketException socketException)
                    {
                        Console.WriteLine(string.Format("SocketException : ", socketException.GetBaseException().Message));
                    }

                }
                catch (ArgumentNullException argumentNullException)
                {
                    Console.WriteLine(string.Concat("ArgumentNullException : ", argumentNullException.GetBaseException().Message));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(string.Concat("Unexpected exception : ", exception.GetBaseException().Message));
                }
            }
            finally
            {
                _socket.Close();
            }
        }

        private static void DispHelp()
        {
            Console.WriteLine("Test Azure Services: -<Command> <FQDN or IPAddress");

            Console.WriteLine("\n-apim This will test ports needed for APIM to work.  \r\nhttps://docs.microsoft.com/en-us/azure/api-management/api-management-using-with-vnet#-common-network-configuration-issues");

            Console.WriteLine("\r\n\n--- Test IP/FQDN & port with logging");
            Console.WriteLine("\nSyntax: sox 192.168.0.1 80 -y or sox server.acme.com 80 -n \r\n\nFirst parameter:\tSpecify IP address or host name to connect to. \n\nSecond parameter:\tPort to connect to.  I.e. 80 for http.\n\nThird parameter:\tEnable event logging. -Y or -N\n\t\t\tIf enabled an warning message will be logged to the\n\t\t\tApplication Log.  Event ID 65535\n"); 

            Console.WriteLine("\r\n\n--- Continuous Test IP/FQDN & port");
            Console.WriteLine("\nSyntax: sox 192.168.0.1 80 -y -c or sox server.acme.com 80 -n -c \r\n\nFirst parameter:\tSpecify IP address or host name to connect to. \n\nSecond parameter:\tPort to connect to.  I.e. 80 for http.\n\nThird parameter:\tEnable event logging. -Y or -N\n\t\t\tIf enabled an warning message will be logged to the\n\t\t\tApplication Log.  Event ID 65535\n\nFourth Parameter: \tContinuous Ping -c ");
            
   //         Console.WriteLine("\r\n\nUse App.config to enable notifications");
        }

        private async static Task SoxCon(IPEndPoint EPhost, Socket sock)
        {
            try
            {
                try
                {
                 //   sock.Connect(EPhost);

                    Stopwatch _sw = new Stopwatch();

                    bool _connSuccess = false;

                    //https://stackoverflow.com/questions/1062035/how-to-configure-socket-connect-timeout

                    _sw.Start();                    
                   // Console.WriteLine($"TCP connect to {EPhost} ");

                    IAsyncResult _result = sock.BeginConnect(EPhost, null, null);
                    _connSuccess = _result.AsyncWaitHandle.WaitOne(5000, true);
                                        
                    _sw.Stop();

                    if(sock.Connected)
                    {
                        Console.WriteLine($"TCP Connect to {EPhost} - From {sock.LocalEndPoint}: Success - {_sw.ElapsedMilliseconds} ms - {DateTime.Now}");
                    }
                    else
                    {
                        Console.Write($"From {sock.LocalEndPoint}: Timed out after - {_sw.ElapsedMilliseconds} ms");
                        // This would show the socket error for connection time out
                        throw new SocketException(10060);
                    }
                   

                    /*
                     * TCP connect to 13.77.161.179:443:
Infinite iterations (warmup 1) ping test:
Connecting to 13.77.161.179:443 (warmup): from 192.168.2.201:57342: 86.43ms
Connecting to 13.77.161.179:443: from 192.168.2.201:57343: 82.15ms
Connecting to 13.77.161.179:443: from 192.168.2.201:57344: 87.61ms
                    */
                }
                catch (ArgumentNullException argumentNullException)
                {
                    Console.WriteLine(string.Concat("ArgumentNullException : ", argumentNullException.GetBaseException().Message));
                }
                catch (SocketException socketException1)
                {
                    SocketException socketException = socketException1;
                    object[] ePhost = new object[] { "SocketException occurred in connection to ", EPhost, ".  Error: ", socketException.GetBaseException().Message };
                    Console.WriteLine(string.Concat(ePhost));

                    if (_eventlog)
                    {

                        await LogEvent(EVENTSOURCE, $"SocketException occurred in connection to {EPhost} Error: {socketException.GetBaseException().Message} ");
                         
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(string.Concat("Unexpected exception : ", exception.GetBaseException()));
                }
            }
            finally
            {
                sock.Close();                
            }
        }

        private async static Task LogEvent(string source, string message)
        {
            if (_eventlog)
            {
                Console.WriteLine("\nEvent written.");

                EventLog eventLog = new EventLog();
                eventLog.Source = source;

                //object[] objArray = new object[] { "SocketException occurred in connection to ", EPhost, ".  Error: ", socketException.GetBaseException().Message };
                //object[] objArray = new object[] { message };

                //eventLog.WriteEntry(string.Concat(objArray), 2, 65535);
                eventLog.WriteEntry(message, EventLogEntryType.Warning, 65535);
                if (EventLog.SourceExists(source))
                {
                    EventLog.DeleteEventSource(source);
                }
                //_eventlog = false;
            }
        }
    }
}
