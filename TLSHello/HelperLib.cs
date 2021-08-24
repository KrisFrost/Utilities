using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace TLSHello
{
    public class ServerValidationData
    {
        public ServerValidationData(X509Chain chain, SslPolicyErrors policyErrors)
        {
            ChainPolicy = chain.ChainPolicy;
            ChainElements = chain.ChainElements;
            ChainStatuses = chain.ChainStatus;
            PolicyErrors = policyErrors;
        }

        public X509ChainPolicy ChainPolicy { get; set; }
        public X509ChainElementCollection ChainElements { get; set; }
        public X509ChainStatus[] ChainStatuses { get; set; }

        public SslPolicyErrors PolicyErrors { get; set; }

    }

    public class HelperLib
    {

        public static void TcpPingHost(string hostName, int port)
        {
            // First get all the IP addresses for a host name.

            try
            {
                //var _addressList = Dns.GetHostByName(hostName).AddressList; GetHostAddresses
                var _addressList = Dns.GetHostAddresses(hostName);

                if (_addressList.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{_addressList.Length} IP's found for host name {hostName}. \r\nTesting TCP connectivity....\r\n");
                    Console.ForegroundColor = ConsoleColor.Cyan;

                    Parallel.ForEach(_addressList, i =>
                    {
                        SoxCon(i, port);
                    });

                }
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

        /// <summary>
        /// Per Maxim here, test http connectivity.  Note though we need to test to all IP's associated with a URL, not just one.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async static Task HttpPing(string url)
        {
            try
            {
                using (HttpClient _client = new HttpClient())
                {
                    //url = @"http://ocsp.pki.goog/gts1c3";

                    var _response = await _client.GetAsync(url);

                    if (_response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Http 200 on Http Get to {url}");
                    }
                    else
                    {
                        Console.WriteLine($"Http status code {_response.StatusCode} returned for url {url}.  \r\nGetting a response code does mean there was connectivity.");
                    }

                    // We don't necessarily need to look for a 200 as some of the urls, don't have websites.
                    // Any resspone means the traffic got there.  With Microsoft, 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Http connection to {url} failed with the following error: {ex.GetBaseException().Message}.");
            }
        }

        public static string[] SplitStringByArrays(string value)
        {
            Regex _aiaSplit = new Regex(@"\[[^\]]\]");

            return _aiaSplit.Split(value);
        }

        /// <summary>
        /// Test Http connectivity
        /// </summary>
        /// <returns></returns>
        private static bool HttpCon()
        {
            bool _retVal = false;

            return _retVal;

        }

        private static bool SoxCon(IPAddress ipAddress, int port)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint EPhost = new IPEndPoint(ipAddress, port);

            bool _connSuccess = false;

            try
            {
                //   sock.Connect(EPhost);

                Stopwatch _sw = new Stopwatch();

                //https://stackoverflow.com/questions/1062035/how-to-configure-socket-connect-timeout

                _sw.Start();
                // Console.WriteLine($"TCP connect to {EPhost} ");

                IAsyncResult _result = sock.BeginConnect(EPhost, null, null);
                _connSuccess = _result.AsyncWaitHandle.WaitOne(5000, true);

                _sw.Stop();

                if (sock.Connected)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" TCP Connect to {EPhost} - From {sock.LocalEndPoint}: Success - {_sw.ElapsedMilliseconds} ms");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"From {sock.LocalEndPoint}: Timed out after - {_sw.ElapsedMilliseconds} ms");
                    // This would show the socket error for connection time out
                    throw new SocketException(10060);
                }
            }
            catch (ArgumentNullException argumentNullException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Concat("ArgumentNullException : ", argumentNullException.GetBaseException().Message));
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (SocketException socketException1)
            {

                SocketException socketException = socketException1;
                object[] ePhost = new object[] { "SocketException occurred in connection to ", EPhost, ".  Error: ", socketException.GetBaseException().Message };
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Concat(ePhost));
                Console.ForegroundColor = ConsoleColor.Gray;

            }
            catch (Exception exception)
            {
                Console.WriteLine(string.Concat("Unexpected exception : ", exception.GetBaseException()));
            }
            finally
            {
                sock.Close();
            }

            return _connSuccess;
        }
    }
}

