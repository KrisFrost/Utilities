
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
//using System.Security.Cryptography.Asn1;
//using System.Security.Cryptography.X509Certificates.Asn1;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using System.Diagnostics;

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace TLSHello
{
    class Program
    {
        // Code from https://docs.microsoft.com/en-us/dotnet/api/system.net.security.sslstream?view=net-5.0

        ////const string OID_AIA_NAME = "Authority Information Access";
        ////const string OID_CRLDP_NAME = "CRL Distribution Points";
        ////const string OID_SAN_NAME = "Subject Alternate Names";
        // OID Value 1.3.6.1.5.5.7.1.1 AIA
        // OID CRL Dist 2.5.29.31 
        // OID SAN "2.5.29.17"
        const string OID_AIA_VALUE = "1.3.6.1.5.5.7.1.1";
        const string OID_CRLDP_VALUE = "2.5.29.31";
        const string OID_SAN_VALUE = "2.5.29.17";

        // in .Net version, RemoteCertificateValidationCallback, SslStream isn't properly instantiated with properties we won't to show.
        // Creating global vars to set values for SslPolicyErrors & Chain.
        // 
        //static SslPolicyErrors _sslPolicyErrors { get; set; }

        //static X509ChainElementCollection _chainElements = null;

        static ServerValidationData _serverValidationData = null;


        static void Main(string[] args)
        {

            int _argLength = args.Length;

            if (_argLength == 2)
            {
                try
                {
                    switch (args[0][1].ToString().ToLower())
                    {
                        case "i":
                            var _hostPort = args[1].ToString().Split(':');

                            // IPAddress addressList = Dns.GetHostEntry(_hostPort[0]).AddressList[0];

                            // GetServerSecurityInfo(addressList.ToString(), Convert.ToInt32(_hostPort[1])).Wait();
                            GetServerSecurityInfo(_hostPort[0], Convert.ToInt32(_hostPort[1])).Wait();

                            //IPEndPoint pEndPoint = new IPEndPoint(addressList, Convert.ToInt32(args[1]));
                            break;

                        default:
                            throw new InvalidDataException();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Invalid Syntax.  Please try again.");
                    DispHelp();
                }

            }
            else
            {
                DispHelp();
            }

            Console.ReadKey();
        }

        private async static Task GetServerSecurityInfo(string remoteHost, int portNum)
        {
            try
            {
                // take in args url/ip & port
                TcpClient _client = new TcpClient(remoteHost, portNum);
                SslStream _sslStream = new SslStream(_client.GetStream(),
                                            false,
                                            new RemoteCertificateValidationCallback(ValidateServerCertificate)
                                            //new LocalCertificateSelectionCallback(ValidateLocalCertificate));
                                            );

                // use this overload to ensure SslStream has the same scope of enabled protocol as HttpWebRequest
                // Try/Catch is used to catch exception form call
                Console.WriteLine($"Local Endpoint:  {_client.Client.LocalEndPoint}");
                Console.WriteLine($"Remote Endpoint:  {_client.Client.RemoteEndPoint}");
                Console.WriteLine($"_____________________________________________________");

                _sslStream.AuthenticateAsClient(remoteHost, null,
                     (SslProtocols)ServicePointManager.SecurityProtocol, true);

                // In .Net why isn't sslstream info available in callback.
                // You may need to get policy errors back somehow or email
                // I guess you can post what you saw to System.Net Discussion<ncltalk@microsoft.com>
                // the dev alias is System.Net Triage < ncltriage@microsoft.com >

                  
                 X509Certificate2 _cert2 = new X509Certificate2(_sslStream.RemoteCertificate);

                ShowCertValidation(_serverValidationData.PolicyErrors, _serverValidationData.ChainElements, _cert2, _sslStream);


                // Encode a test message into a byte array.
                // Signal the end of the message using the "<EOF>".
                //byte[] messsage = Encoding.UTF8.GetBytes("Hello from the client.<EOF>");
                //// Send hello message to the server.
                //sslStream.Write(messsage);
                //sslStream.Flush();
                //// Read message from the server.
                //string serverMessage = ReadMessage(sslStream);
                //Console.WriteLine("Server says: {0}", serverMessage);

                _client.Close();
                _sslStream.Close();
            }
            catch (Exception ex)
            {
                var _baseEx = ex.GetBaseException();

                // Don't show a callback exception again
                if (_baseEx.Message != "The remote certificate was rejected by the provided RemoteCertificateValidationCallback.")
                {
                    Console.WriteLine($"Exception running GetServerSecurityInfo() - {ex.GetBaseException().ToString()}");
                }
            }


            /*

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                       SecurityProtocolType.Tls11 |
                                       SecurityProtocolType.Tls12 |
                                       SecurityProtocolType.Tls13 
                                       ;

            // Handle the Server certificate exchange, to inspect the certificates received
            ServicePointManager.ServerCertificateValidationCallback += TlsValidationCallback;

            Uri requestUri = new Uri("https://apiutils.azure-api.net");
            var request = WebRequest.CreateHttp(requestUri);

            // request.Method = WebRequestMethods.Http.Post;
            request.Method = WebRequestMethods.Http.Get;
            request.ServicePoint.Expect100Continue = false;
            request.AllowAutoRedirect = true;
            request.CookieContainer = new CookieContainer();

            //request.ContentType = "application/x-www-form-urlencoded";
            //var postdata = Encoding.UTF8.GetBytes("Some postdata here");
            //request.ContentLength = postdata.Length;

            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident / 7.0; rv: 11.0) like Gecko";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate;q=0.8");
            request.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");

            using (var requestStream = request.GetRequestStream())
            {
                //Here the request stream is already validated
                SslProtocols sslProtocol = ExtractSslProtocol(requestStream);
                if (sslProtocol < SslProtocols.Tls12)
                {
                    // Refuse/close the connection
                }
            }
            */
        }

        private static X509Certificate ValidateLocalCertificate(object sender,
            string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            // Need to figure out exactly what we'd use this for.
            // Console.WriteLine("Client is selecting a local certificate.");
            if (acceptableIssuers != null &&
                acceptableIssuers.Length > 0 &&
                localCertificates != null &&
                localCertificates.Count > 0)
            {
                // Use the first certificate that is from an acceptable issuer.
                foreach (X509Certificate certificate in localCertificates)
                {
                    string issuer = certificate.Issuer;
                    if (Array.IndexOf(acceptableIssuers, issuer) != -1)
                        return certificate;
                }
            }
            if (localCertificates != null &&
                localCertificates.Count > 0)
                return localCertificates[0];

            return null;
        }

        static string ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF.
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }

        /*
         * RemoteCertificateValidationCallback callback = delegate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslError) {
    X509Certificate2 certv2 = new X509Certificate2(cert);
    // more code here that sends soap request
    return false;
};
         */
        public static bool ValidateServerCertificate(
          object sender,
          X509Certificate cert,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
        {
            // sender SslStream properties we need are null here in .Net, not in core.  Not sure why
            // so we're assigning the values to globals so we can display what we need to in
            // order.

            // Must use our own class and assign values as chain will null out after this call
            // If I create a global chain property and assign it here, by ref and losing data

            _serverValidationData = new ServerValidationData(chain, sslPolicyErrors);

            // return sslPolicyErrors == SslPolicyErrors.None;
            // We need details, not the generic exception on false
            return true;
        }


        /// <summary>
        /// Because .Net Framework doesn't populate Sslstream in the callback, moving all the logic to check cert and errors
        /// to this function to all from the main function where the data we need in SslStream is available.
        /// </summary>
        /// <param name="sslPolicyErrors"></param>
        /// <param name="chainElements"></param>
        /// <param name="cert2"></param>
        /// <param name="sslStream"></param>
        private static void ShowCertValidation(SslPolicyErrors sslPolicyErrors, X509ChainElementCollection chainElements, X509Certificate2 cert2, SslStream sslStream)
        {            

            // Need to have logic that will look in Extensions and use Jeremy's info 
            /* yeah, that's where you'd need to loop over cert.Extensions, find the one with the right data type identifier / 
             * OID and decode it(extn.RawData) using AsnReader.
             * [6:10 PM] Jeremy Barton
Neither of the CRL Distribution Point extension nor the Authority Information Access extension have good first-class support in .NET (no public X509Extension-derived type that exposes their data).  For each of them you could loop over cert.Extensions, find the item with the right OID, and then decode it using System.Formats.Asn1.AsnReader and the structure from https://datatracker.ietf.org/doc/html/rfc5280#section-4.2.1.13 / section 4.2.2.1.  (Or find that same code in our codebase and copy it out)

[6:12 PM] Jeremy Barton
As for #3) On Windows, no, there's no good way to do it.  On Linux you can use the CipherSuitePolicy and every time you connect, write down what ciphersuite got selected, remove that from the list, and try again.  When the connection finally fails you know that the server doesn't support anything left in the list (or the client doesn't support it, I suppose).

[6:13 PM] Jeremy Barton
That's what the SslLabs scanner, et al, do.  The server never shares its list, it just picks one from the client list.

             */
            
            // Check sslStream.SslProtocol here 
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\r\nProtocol/Cipher Info:");
            Console.ForegroundColor = ConsoleColor.Gray;

            string _tlsVer = sslStream.SslProtocol.ToString().ToLower();

            Console.WriteLine($"\r\nProtocol:  {sslStream.SslProtocol}");

            if (_tlsVer != "tls12")
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("Waring: TLS versions should be 1.2 for APIM and no lower for security");
                Console.ForegroundColor = ConsoleColor.Gray;
            }


            // Not available in .Net Framework, only core.
            //Console.WriteLine($"Negotiated Cipher: {sslStream.NegotiatedCipherSuite}");
            Console.WriteLine($"Cipher Algorithm: {sslStream.CipherAlgorithm}");
            Console.WriteLine($"Cipher Strength: {sslStream.CipherStrength}");
            Console.WriteLine($"Hash Alg:  {sslStream.HashAlgorithm}");
            Console.WriteLine($"Hash Strength:  {sslStream.HashStrength}");

            Console.WriteLine("\r\n____________________________________________________");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\r\nValidating Certificate...");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                //  Console.WriteLine($"Cert Name:  {sslStream.RemoteCertificate.GetName()}");
                Console.WriteLine($"\r\nCert Subject:  {cert2.Subject}");
                Console.WriteLine($"Cert Issuer:  {cert2.Issuer}");

                Console.WriteLine($"Cert Start:  {cert2.GetEffectiveDateString()}");
                Console.WriteLine($"Cert Exp:  {cert2.GetExpirationDateString()}");
                Console.WriteLine($"Cert Serial #:  {cert2.GetSerialNumberString()}");
                Console.WriteLine($"Cert Alg.:  {cert2.GetKeyAlgorithm()}");
                Console.WriteLine($"Sig. Alg:  {cert2.SignatureAlgorithm.FriendlyName}");
                Console.WriteLine($"ThumbPrint:  {cert2.Thumbprint}");
                Console.WriteLine($"Version:  {cert2.Version}");

                //  Console.WriteLine($"SANS:  {_cert2.GetNameInfo(X509NameType.UpnName, false)}");
                //  Console.WriteLine($"Cert Alg. Value:  {sslStream.RemoteCertificate.}");
                //Console.WriteLine($"Cert Public Key:  {certificate.GetPublicKeyString()}");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\r\nNo SSL Policy Errors.");
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.WriteLine("\r\n____________________________________________________\r\n");

                // Check for AIA and if any urls, try to make the connection.  Should this be a tcp ping or get the file?
                //certificate.Extensions
                // Think we need to use values because names may not match because of language
                // OID Value 1.3.6.1.5.5.7.1.1 AIA
                // OID CRL Dist 2.5.29.31 AIA
                // OID SAN "2.5.29.17"
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Checking for AIA Extensions ....");
                Console.ForegroundColor = ConsoleColor.Gray;

                var _aiaExt = cert2.Extensions.OfType<X509Extension>().FirstOrDefault(f => f.Oid.Value == OID_AIA_VALUE);

                if (_aiaExt != null)
                {
                    //  var _contentType = X509Certificate2.GetCertContentType(_aiaOID.RawData);
                    //   var _aiaData = new AsnReader(_aiaOID.RawData, AsnEncodingRules.DER);                   // _aiaOID.RawData
                    //   var _strd = _aiaData.ReadCharacterString(UniversalTagNumber.GeneralString);

                    /*
                    public static IEnumerable<string> ParseSujectAlternativeNames(X509Certificate2 cert)
                    {
                        Regex sanRex = new Regex(@"^DNS Name=(.*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

                        var sanList = from X509Extension ext in cert.Extensions
                                      where ext.Oid.FriendlyName.Equals("Subject Alternative Name", StringComparison.Ordinal)
                                      let data = new AsnEncodedData(ext.Oid, ext.RawData)
                                      let text = data.Format(true)
                                      from line in text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      let match = sanRex.Match(line)
                                      where match.Success && match.Groups.Count > 0 && !string.IsNullOrEmpty(match.Groups[1].Value)
                                      select match.Groups[1].Value;

                        return sanList;
                    }
                    */
                    //var _data = _aiaData.Rea
                    AsnEncodedData _asnData = new AsnEncodedData(_aiaExt.Oid, _aiaExt.RawData);
                    var _aiaRaw = _asnData.Format(true);

                    // Split string by Array []'s
                    var _aiaCollection = HelperLib.SplitStringByArrays(_aiaRaw);

                    if (_aiaCollection.Any())
                    {
                        for (int i = 0; i < _aiaCollection.Length; i++)
                        {
                            // Look through get the name and then also 
                            // get the Uri.  Maybe validate it with the Uri
                            string _element = _aiaCollection[i];

                            if (!string.IsNullOrEmpty(_element))
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"\r\nAIA {i}:");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine(_element);

                                // Here we need to get the URL, test the URI validity
                                // Then test the network connectivity to the host name in the URL

                                var _aiaUrls = _aiaCollection[i].ToLower().Split(new string[] { "url=" }, StringSplitOptions.RemoveEmptyEntries);

                                /*
                                 * \( : match an opening parentheses
                                    ( : begin capturing group
                                    [^)]+: match one or more non ) characters
                                    ) : end capturing group
                                    \) : match closing parentheses
                                 */
                                //  Regex _parenUrl = new Regex(@"/\(([^)]+)\)/");

                                foreach (var url in _aiaUrls)
                                {
                                    // First get the host name from the URL.
                                    // THen we want to get all the IP addresses for the host name. (DNS is round robin).
                                    //
                                    if (url.ToLower().StartsWith("http"))
                                    {
                                        try
                                        {
                                            Uri _uri = new Uri(url);
                                            HelperLib.TcpPingHost(_uri.Host, _uri.Port);

                                            Console.WriteLine("\r\nTesting http connectivity.");
                                            // Test Http Connectivity as well
                                            HelperLib.HttpPing(_uri.AbsoluteUri).Wait();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Url for AIA {url} is not valid.  Message: {ex.GetBaseException().Message}");
                                        }
                                    }

                                    Console.WriteLine("\r\n");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Certificate doesn't contain any AIA Extensions ....");
                    }

                    #region AsnReader
                    /*
                                        try
                                        {
                                            AsnReader reader = new AsnReader(_aiaExt.RawData, AsnEncodingRules.DER);
                                            AsnReader sequenceReader = reader.ReadSequence();
                                            reader.ThrowIfNotEmpty();

                                            while (sequenceReader.HasData)
                                            {

                                                AccessDescriptionAsn.Decode(ref sequenceReader, authorityInformationAccess, out AccessDescriptionAsn description);
                                                if (StringComparer.Ordinal.Equals(description.AccessMethod, recordTypeOid))
                                                {
                                                    GeneralNameAsn name = description.AccessLocation;
                                                    if (name.Uri != null &&
                                                        Uri.TryCreate(name.Uri, UriKind.Absolute, out Uri? uri) &&
                                                        uri.Scheme == "http")
                                                    {
                                                        return name.Uri;
                                                    }
                                                }
                                            }
                                        }
                                        catch (CryptographicException)
                                        {
                                            // Treat any ASN errors as if the extension was missing.
                                        }
                                        catch (AsnContentException)
                                        {
                                            // Treat any ASN errors as if the extension was missing.
                                        }
                    */
                    #endregion

                }

                Console.WriteLine("____________________________________________________\r\n");

                Console.WriteLine("Checking for CRL Distribution Point Extensions ....");
                var _crldpExt = cert2.Extensions.OfType<X509Extension>().FirstOrDefault(f => f.Oid.Value == OID_CRLDP_VALUE);

                if (_crldpExt != null)
                {
                    //var _data = _aiaData.Rea
                    AsnEncodedData _asnData = new AsnEncodedData(_crldpExt.Oid, _crldpExt.RawData);
                    var _crlRaw = _asnData.Format(true);

                    // Split by array as above
                    // Then display and then split again by URL= same as above.
                    // Create URI, the get the host and port.
                    // Test and show the results.

                    // Split string by Array []'s
                    var _crlCollection = HelperLib.SplitStringByArrays(_crlRaw);

                    if (_crlCollection.Any())
                    {
                        for (int i = 0; i < _crlCollection.Length; i++)
                        {
                            // Look through get the name and then also 
                            // get the Uri.  Maybe validate it with the Uri
                            string _element = _crlCollection[i];

                            if (!string.IsNullOrEmpty(_element))
                            {
                                Console.WriteLine($"\r\nCRL DP's URL {i}:");
                                Console.WriteLine(_element);

                                // Here we need to get the URL, test the URI validity
                                // Then test the network connectivity to the host name in the URL

                                var _crlUrls = _crlCollection[i].ToLower().Split(new string[] { "url=" }, StringSplitOptions.RemoveEmptyEntries);

                                /*
                                 * \( : match an opening parentheses
                                    ( : begin capturing group
                                    [^)]+: match one or more non ) characters
                                    ) : end capturing group
                                    \) : match closing parentheses
                                 */
                                //  Regex _parenUrl = new Regex(@"/\(([^)]+)\)/");

                                foreach (var url in _crlUrls)
                                {
                                    // First get the host name from the URL.
                                    // THen we want to get all the IP addresses for the host name. (DNS is round robin).
                                    //
                                    if (url.ToLower().StartsWith("http"))
                                    {
                                        try
                                        {
                                            Uri _uri = new Uri(url);
                                            HelperLib.TcpPingHost(_uri.Host, _uri.Port);
                                            Console.WriteLine("\r\nTesting http connectivity.");
                                            // Test Http Connectivity as well
                                            HelperLib.HttpPing(_uri.AbsoluteUri).Wait();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Url for CRL DP {url} is not valid.  Message: {ex.GetBaseException().Message}");
                                        }
                                    }

                                    Console.WriteLine("\r\n");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Certificate doesn't contain any CRL Extensions ....");
                    }
                }

                /*
                 * We use a variant of the type (AsnValueReader) in conjunction with some generated code, 
                 * but https://github.com/dotnet/runtime/blob/a85d36fed49b8c56d3365417e047fc4306cd74fc/src/libraries/System.Security.Cryptography.X509Certificates/src/Internal/Cryptography/Pal.Unix/OpenSslX509ChainProcessor.cs#L1163-L1196
                 CDP (CRL Distribution Point) is the same, but different: https://github.com/dotnet/runtime/blob/d62117f7f383a56b781fbf663e69859ebf11
                 */
                // Show SAN
                //var _sanExt = _cert2.Extensions.OfType<X509Extension>().FirstOrDefault(f => f.Oid.Value == OID_SAN_VALUE);

                //if (_sanExt != null)
                //{
                //    //var _data = _aiaData.Rea
                //    AsnEncodedData _asnData = new AsnEncodedData(_sanExt.Oid, _sanExt.RawData);
                //    var _sanRaw = _asnData.Format(true);

                //}

                // Friendly name Authority Information Access
                // Check for CRL CRL Distribution Points

                // Get SAN Friendly OID Name  Subject Alternate Names


                // Here show Chain Elements
                Console.WriteLine("\r\n____________________________________________________");
                Console.WriteLine("\r\nCerticate Chain Info:");
                for (int i = 0; i < chainElements.Count; i++)
                {

                    Console.WriteLine($"Level {i + 1}:  \r\n{chainElements[i].Certificate}");
                }

                Console.WriteLine("\r\n____________________________________________________");

            }
            else
            {
                // Console.WriteLine("\r\nCertificate error: {0}", sslPolicyErrors);

                // If there are errors, go through every element, show the cert info as well as the status info
                var _chainElementsWithErrors = chainElements.OfType<X509ChainElement>()
                    .Where(b => b.ChainElementStatus != null && b.ChainElementStatus.Length >= 1).ToArray();

                if (_chainElementsWithErrors.Any())
                {
                    Console.WriteLine($"Chain Element Errors Found:  {_chainElementsWithErrors.Length}\r\n");

                    for (int i = 0; i < _chainElementsWithErrors.Count(); i++)
                    {
                        Console.WriteLine($"Chain Element:  {i + 1}\r\n");
                        var _element = _chainElementsWithErrors[i];

                        Console.WriteLine($"Cert Subject:  {_element.Certificate.Subject}");
                        Console.WriteLine($"Cert Issuer:  {_element.Certificate.Issuer}");

                        Console.WriteLine($"Cert Start:  {_element.Certificate.GetEffectiveDateString()}");
                        Console.WriteLine($"Cert Exp:  {_element.Certificate.GetExpirationDateString()}");
                        Console.WriteLine($"Cert Serial #:  {_element.Certificate.GetSerialNumberString()}");
                        Console.WriteLine($"Cert Alg.:  {_element.Certificate.GetKeyAlgorithm()}");


                        Console.WriteLine("\r\nErrors");
                        Console.ForegroundColor = ConsoleColor.Red;
                        for (int ii = 0; ii < _element.ChainElementStatus.Length; ii++)
                        {

                            Console.WriteLine($"{ii + 1}. - Status: {_element.ChainElementStatus[ii].Status} - {_element.ChainElementStatus[ii].StatusInformation}");
                        }

                        Console.ForegroundColor = ConsoleColor.Gray;

                        Console.WriteLine("\r\n");
                    }
                }

                //if(chain != null && chain.ChainStatus != null && chain.ChainStatus.Length > 0)
                //{
                //    Console.WriteLine($"\r\nChain errors: {chain.ChainStatus.Length}");

                //    for (int i = 0; i < chain.ChainStatus.Length; i++)
                //    {
                //        Console.WriteLine($"{i+1}. - Status: {chain.ChainStatus[0].Status} - {chain.ChainStatus[i].StatusInformation}");
                //    }               

                //}

                // Do not allow this client to communicate with unauthenticated servers.
            }
        }

        private static SslProtocols ExtractSslProtocol(Stream stream)
        {
            if (stream is null) return SslProtocols.None;

            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            Stream metaStream = stream;

            if (stream.GetType().BaseType == typeof(GZipStream))
            {
                metaStream = (stream as GZipStream).BaseStream;
            }
            else if (stream.GetType().BaseType == typeof(DeflateStream))
            {
                metaStream = (stream as DeflateStream).BaseStream;
            }

            var connection = metaStream.GetType().GetProperty("Connection", bindingFlags).GetValue(metaStream);
            if (!(bool)connection.GetType().GetProperty("UsingSecureStream", bindingFlags).GetValue(connection))
            {
                // Not a Https connection
                return SslProtocols.None;
            }
            var tlsStream = connection.GetType().GetProperty("NetworkStream", bindingFlags).GetValue(connection);
            var tlsState = tlsStream.GetType().GetField("m_Worker", bindingFlags).GetValue(tlsStream);
            return (SslProtocols)tlsState.GetType().GetProperty("SslProtocol", bindingFlags).GetValue(tlsState);
        }

        private static bool TlsValidationCallback(object sender, X509Certificate CACert, X509Chain CAChain, SslPolicyErrors sslPolicyErrors)
        {
            List<Oid> oidExtractor = CAChain
                                     .ChainElements
                                     .Cast<X509ChainElement>()
                                     .Select(x509 => new Oid(x509.Certificate.SignatureAlgorithm.Value))
                                     .ToList();
            // Inspect the oidExtractor list

            var certificate = new X509Certificate2(CACert);

            //If you needed/have to pass a certificate, add it here.
            //X509Certificate2 cert = new X509Certificate2(@"[localstorage]/[ca.cert]");
            //CAChain.ChainPolicy.ExtraStore.Add(cert);
            CAChain.Build(certificate);
            foreach (X509ChainStatus CACStatus in CAChain.ChainStatus)
            {
                if ((CACStatus.Status != X509ChainStatusFlags.NoError) &
                    (CACStatus.Status != X509ChainStatusFlags.UntrustedRoot))
                    return false;
            }
            return true;
        }

        private static void DispHelp()
        {
            Console.WriteLine("TLSHello makes a https connection and gives server info about that connection.");

            Console.WriteLine("\nOptions: -i for remote conn. Syntax: tlshello.exe -i microsoft.com:443");

            //Console.WriteLine("\r\n\n--- Test IP/FQDN & port with logging");
            //Console.WriteLine("\nSyntax: sox 192.168.0.1 80 -y or sox server.acme.com 80 -n \r\n\nFirst parameter:\tSpecify IP address or host name to connect to. \n\nSecond parameter:\tPort to connect to.  I.e. 80 for http.\n\nThird parameter:\tEnable event logging. -Y or -N\n\t\t\tIf enabled an warning message will be logged to the\n\t\t\tApplication Log.  Event ID 65535\n");

            //Console.WriteLine("\r\n\n--- Continuous Test IP/FQDN & port");
            //Console.WriteLine("\nSyntax: sox 192.168.0.1 80 -y -c or sox server.acme.com 80 -n -c \r\n\nFirst parameter:\tSpecify IP address or host name to connect to. \n\nSecond parameter:\tPort to connect to.  I.e. 80 for http.\n\nThird parameter:\tEnable event logging. -Y or -N\n\t\t\tIf enabled an warning message will be logged to the\n\t\t\tApplication Log.  Event ID 65535\n\nFourth Parameter: \tContinuous Ping -c ");

            //         Console.WriteLine("\r\n\nUse App.config to enable notifications");
        }
    }
}

