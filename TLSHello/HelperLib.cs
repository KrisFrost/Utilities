using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TLSHello
{
    public class HelperLib
    {
        public static void TcpPingHost(string hostName, int port)
        {
            // First get all the IP addresses for a host name.

            try
            {
                var _addressList = Dns.GetHostByName(hostName).AddressList;

                if (_addressList.Any())
                {
                    Console.WriteLine($"{_addressList.Length} IP's found for host name {hostName}.  Testing TCP connectivity....\r\n");

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

        public static string[] SplitStringByArrays(string value)
        {
            Regex _aiaSplit = new Regex(@"\[[^\]]\]");
            
            return _aiaSplit.Split(value);
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
                    Console.WriteLine($" TCP Connect to {EPhost} - From {sock.LocalEndPoint}: Success - {_sw.ElapsedMilliseconds} ms");
                }
                else
                {                    
                    Console.Write($"From {sock.LocalEndPoint}: Timed out after - {_sw.ElapsedMilliseconds} ms");
                    // This would show the socket error for connection time out
                    throw new SocketException(10060);
                }
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
