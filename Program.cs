#region Using
using System.Net.NetworkInformation;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
#endregion

namespace ipsaroundme
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region Variables
            IPAddress ipAddress = await GetMyIpAsync();
            string ipAddressString = ipAddress.ToString();

            List<IPAddress> ipAddresses = new List<IPAddress>();
            List<IPAddress> aliveIpAddresses = new List<IPAddress>();

            object locker = new object();

            Stopwatch stopwatch = new Stopwatch();

            Ping pingSender = new Ping ();

            PingOptions options = new PingOptions ();
            options.DontFragment = true;
            options.Ttl = 128;

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;

            IPAddress gatewayIP = GetDefaultGateway();
            double avgGatewayDelay = GateWayRTT(gatewayIP);
            #endregion

            if(args.Length == 0)
            {
                Console.WriteLine("\nIdentifies public live IP addresses around you and shows the distance from you in kilometres and hops.\nAdditional information regarding IP blocks allocation can be found here: https://www.iana.org/numbers\n");

                Console.WriteLine("Usage:\n");
                Console.WriteLine("ipsnearme -s \n\tFor your public IP address neighbours check. CIDR /24 or total 256 IP addresses.\n");
                Console.WriteLine("ipsnearme -r <start ip> <end ip>  \n\tFor range of public IP addresses check.\n\tExample: ipsnearme -r 1.1.1.1 2.2.2.2\n");
                Console.WriteLine("ipsnearme -c <ip>/<CIDR> \n\tFor CIDR public IP addresses check.\n\tExample: ipsnearme -c 1.1.1.1/24\n");

                Console.WriteLine("\nYour public IP address is: " + ipAddressString);
                Console.WriteLine("Your default gateway is: " + gatewayIP + "\n");
                return;
            }
            else
            {
                if(args[0].ToString().Trim() == "-s")
                {
                    for(int i = 0; i <= 255; i++)
                    {
                        ipAddresses.Add(IPAddress.Parse(ipAddressString.Substring(0, ipAddressString.LastIndexOf('.') + 1) + i));
                    }

                    int aliveIPs = 0;
                    int deadIPs = 0;

                    int processed = 0;
                    int total = ipAddresses.Count;

                    List<Tuple<IPAddress, IPAddress, double, int>> ipAddressesWithDistance = new List<Tuple<IPAddress, IPAddress, double, int>>();

                    stopwatch.Start();

                    Parallel.ForEach(ipAddresses, (ip) =>
                    {
                        lock(locker)
                        {
                            ClearCurrentConsoleLine();
                            processed++;
                            Console.WriteLine("Processing: " + processed + " of " + total);                          
                            Console.SetCursorPosition(0, Console.CursorTop - 1);

                            List<double> rttList = new List<double>();

                            // Ping 5 times the IP address.
                            for(int i = 0; i <= 5; i++){
                                PingReply reply = pingSender.Send(ip, timeout, buffer, options);

                                if(reply.Status == IPStatus.Success)
                                {
                                    rttList.Add(reply.RoundtripTime);
                                }
                            }

                            if(rttList.Count > 0)
                            {
                                double avg = rttList.Average();
                                double averageOneWayTripTimeSec = ((avg - avgGatewayDelay) / 1000) / 2;

                                if(averageOneWayTripTimeSec < 0)
                                {
                                    averageOneWayTripTimeSec = 0;
                                }

                                // Measure hops / traceroute.
                                List<Tuple<int>> hops = new List<Tuple<int>>();
            
                                int timeoutTraceRoute = 10000; // 10 seconds.
                                int maxHops = 30;

                                for (int hopIndex = 0; hopIndex < maxHops; hopIndex++)
                                {
                                    int ttl = hopIndex + 1;

                                    PingReply reply = default(PingReply);
                                    PingOptions pingOptions = new PingOptions(ttl, true);

                                    reply = pingSender.Send(ip, timeoutTraceRoute, buffer, pingOptions);

                                    Tuple<int> traceRouteHopInfo = new Tuple<int>(ttl);
                                    hops.Add(traceRouteHopInfo);

                                    if (reply.Status == IPStatus.Success)
                                    {
                                        break;
                                    }
                                }
                                
                                // 299792458 meters/second the speed of light

                                double distance = (299792458.0 * averageOneWayTripTimeSec) / 1000; // km
                                ipAddressesWithDistance.Add(new Tuple<IPAddress, IPAddress, double, int>(ip, ipAddress, Math.Round(distance, 3), hops.Max(x => x.Item1)));

                                hops.Clear();
                                aliveIPs++;
                            }
                            else
                            {
                                deadIPs++;
                            }
                        }
                    });
                    
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;

                    ipAddresses.Clear();

                    Console.WriteLine("\n\nRemote IP, Local IP, Distance (km), Hops");
                    Console.WriteLine("----------------------------------------\n");

                    ipAddressesWithDistance = ipAddressesWithDistance.OrderBy(x => x.Item3).OrderBy(y => y.Item4).ToList();

                    foreach(Tuple<IPAddress, IPAddress, double, int> ipAddressWithDistance in ipAddressesWithDistance)
                    {
                        Console.WriteLine(ipAddressWithDistance.Item1 + "," + ipAddressWithDistance.Item2 + "," + ipAddressWithDistance.Item3 + "," + ipAddressWithDistance.Item4);
                    }

                    Console.WriteLine("\nYour public IP address is: " + ipAddressString);
                    Console.WriteLine("Your default gateway is: " + gatewayIP + "\n");

                    Console.WriteLine("Total alive IP addresses: " + aliveIPs);
                    Console.WriteLine("Total dead IP addresses: " + deadIPs);

                    Console.WriteLine("\nFinished in: {0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                    Console.WriteLine("Started on: {0}\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
                else if(args[0].ToString().Trim().Substring(0, 2) == "-r")
                {
                    string startIP = args[1].ToString();
                    string endIP = args[2].ToString();

                    for (uint i = IpToInt(startIP); i <= IpToInt(endIP); i++)
                    {
                        byte[] bytes = BitConverter.GetBytes(i);
                        ipAddresses.Add(new IPAddress(new[] { bytes[3], bytes[2], bytes[1], bytes[0] }));
                    }

                    int aliveIPs = 0;
                    int deadIPs = 0;

                    int processed = 0;
                    int total = ipAddresses.Count;

                    stopwatch.Start();

                    List<Tuple<IPAddress, IPAddress, double, int>> ipAddressesWithDistance = new List<Tuple<IPAddress, IPAddress, double, int>>();

                    Parallel.ForEach(ipAddresses, (ip) =>
                    {
                        lock(locker)
                        {
                            ClearCurrentConsoleLine();
                            processed++;
                            Console.WriteLine("Processing: " + processed + " of " + total);                          
                            Console.SetCursorPosition(0, Console.CursorTop - 1);

                            List<double> rttList = new List<double>();

                            // Ping 5 times the IP address.
                            for(int i = 0; i <= 5; i++){
                                PingReply reply = pingSender.Send(ip, timeout, buffer, options);

                                if(reply.Status == IPStatus.Success)
                                {
                                    rttList.Add(reply.RoundtripTime);
                                }
                            }

                            if(rttList.Count > 0)
                            {
                                double avg = rttList.Average();
                                double averageOneWayTripTimeSec = ((avg - avgGatewayDelay) / 1000) / 2;

                                if(averageOneWayTripTimeSec < 0)
                                {
                                    averageOneWayTripTimeSec = 0;
                                }

                                // Measure hops / traceroute.
                                List<Tuple<int>> hops = new List<Tuple<int>>();
            
                                int timeoutTraceRoute = 10000; // 10 seconds.
                                int maxHops = 30;

                                for (int hopIndex = 0; hopIndex < maxHops; hopIndex++)
                                {
                                    int ttl = hopIndex + 1;

                                    PingReply reply = default(PingReply);
                                    PingOptions pingOptions = new PingOptions(ttl, true);

                                    reply = pingSender.Send(ip, timeoutTraceRoute, buffer, pingOptions);

                                    Tuple<int> traceRouteHopInfo = new Tuple<int>(ttl);
                                    hops.Add(traceRouteHopInfo);

                                    if (reply.Status == IPStatus.Success)
                                    {
                                        break;
                                    }
                                }
                                
                                // 299792458 meters/second the speed of light

                                double distance = (299792458.0 * averageOneWayTripTimeSec) / 1000; // km
                                ipAddressesWithDistance.Add(new Tuple<IPAddress, IPAddress, double, int>(ip, ipAddress, Math.Round(distance, 3), hops.Max(x => x.Item1)));

                                hops.Clear();
                                aliveIPs++;
                            }
                            else
                            {
                                deadIPs++;
                            }
                        }
                    });
                    
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;

                    ipAddresses.Clear();

                    Console.WriteLine("\n\nRemote IP, Local IP, Distance (km), Hops");
                    Console.WriteLine("----------------------------------------\n");

                    ipAddressesWithDistance = ipAddressesWithDistance.OrderBy(x => x.Item3).OrderBy(y => y.Item4).ToList();

                    foreach(Tuple<IPAddress, IPAddress, double, int> ipAddressWithDistance in ipAddressesWithDistance)
                    {
                        Console.WriteLine(ipAddressWithDistance.Item1 + "," + ipAddressWithDistance.Item2 + "," + ipAddressWithDistance.Item3 + "," + ipAddressWithDistance.Item4);
                    }

                    Console.WriteLine("\nYour public IP address is: " + ipAddressString);
                    Console.WriteLine("Your default gateway is: " + gatewayIP + "\n");

                    Console.WriteLine("Total alive IP addresses: " + aliveIPs);
                    Console.WriteLine("Total dead IP addresses: " + deadIPs);

                    Console.WriteLine("\nFinished in: {0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                    Console.WriteLine("Started on: {0}\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
                else if(args[0].ToString().Trim().Substring(0, 2) == "-c")
                {                    
                    string IP = args[1].ToString().Trim();
                    string[] parts = IP.Split('.', '/');

                    uint ipNum = (Convert.ToUInt32(parts[0]) << 24) | (Convert.ToUInt32(parts[1]) << 16) | (Convert.ToUInt32(parts[2]) << 8) | Convert.ToUInt32(parts[3]);

                    int maskbits = Convert.ToInt32(parts[4]);
                    
                    uint mask = 0xffffffff;
                    mask <<= (32 - maskbits);

                    uint startIP = ipNum & mask;
                    uint ipEnd = ipNum | (mask ^ 0xffffffff);
                    
                    for (uint i = startIP; i <= ipEnd; i++)
                    {
                        byte[] bytes = BitConverter.GetBytes(i);
                        ipAddresses.Add(new IPAddress(new[] { bytes[3], bytes[2], bytes[1], bytes[0] }));
                    }

                    int aliveIPs = 0;
                    int deadIPs = 0;

                    int processed = 0;
                    int total = ipAddresses.Count;

                    stopwatch.Start();

                    List<Tuple<IPAddress, IPAddress, double, int>> ipAddressesWithDistance = new List<Tuple<IPAddress, IPAddress, double, int>>();

                    Parallel.ForEach(ipAddresses, (ip) =>
                    {
                        lock(locker)
                        {
                            ClearCurrentConsoleLine();
                            processed++;
                            Console.WriteLine("Processing: " + processed + " of " + total);                          
                            Console.SetCursorPosition(0, Console.CursorTop - 1);

                            List<double> rttList = new List<double>();

                            // Ping 5 times the IP address.
                            for(int i = 0; i <= 5; i++){
                                PingReply reply = pingSender.Send(ip, timeout, buffer, options);

                                if(reply.Status == IPStatus.Success)
                                {
                                    rttList.Add(reply.RoundtripTime);
                                }
                            }

                            if(rttList.Count > 0)
                            {
                                double avg = rttList.Average();
                                double averageOneWayTripTimeSec = ((avg - avgGatewayDelay) / 1000) / 2;

                                if(averageOneWayTripTimeSec < 0)
                                {
                                    averageOneWayTripTimeSec = 0;
                                }

                                // Measure hops / traceroute.
                                List<Tuple<int>> hops = new List<Tuple<int>>();
            
                                int timeoutTraceRoute = 10000; // 10 seconds.
                                int maxHops = 30;

                                for (int hopIndex = 0; hopIndex < maxHops; hopIndex++)
                                {
                                    int ttl = hopIndex + 1;

                                    PingReply reply = default(PingReply);
                                    PingOptions pingOptions = new PingOptions(ttl, true);

                                    reply = pingSender.Send(ip, timeoutTraceRoute, buffer, pingOptions);

                                    Tuple<int> traceRouteHopInfo = new Tuple<int>(ttl);
                                    hops.Add(traceRouteHopInfo);

                                    if (reply.Status == IPStatus.Success)
                                    {
                                        break;
                                    }
                                }
                                
                                // 299792458 meters/second the speed of light

                                double distance = (299792458.0 * averageOneWayTripTimeSec) / 1000; // km
                                ipAddressesWithDistance.Add(new Tuple<IPAddress, IPAddress, double, int>(ip, ipAddress, Math.Round(distance, 3), hops.Max(x => x.Item1)));

                                hops.Clear();
                                aliveIPs++;
                            }
                            else
                            {
                                deadIPs++;
                            }
                        }
                    });
                    
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;

                    ipAddresses.Clear();

                    Console.WriteLine("\n\nRemote IP, Local IP, Distance (km), Hops");
                    Console.WriteLine("----------------------------------------\n");

                    ipAddressesWithDistance = ipAddressesWithDistance.OrderBy(x => x.Item3).OrderBy(y => y.Item4).ToList();

                    foreach(Tuple<IPAddress, IPAddress, double, int> ipAddressWithDistance in ipAddressesWithDistance)
                    {
                        Console.WriteLine(ipAddressWithDistance.Item1 + "," + ipAddressWithDistance.Item2 + "," + ipAddressWithDistance.Item3 + "," + ipAddressWithDistance.Item4);
                    }

                    Console.WriteLine("\nYour public IP address is: " + ipAddressString);
                    Console.WriteLine("Your default gateway is: " + gatewayIP + "\n");

                    Console.WriteLine("Total alive IP addresses: " + aliveIPs);
                    Console.WriteLine("Total dead IP addresses: " + deadIPs);

                    Console.WriteLine("\nFinished in: {0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                    Console.WriteLine("Started on: {0}\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }
        }

        #region Methods
        protected static string toip(uint ip)
        {
            return String.Format("{0}.{1}.{2}.{3}", ip >> 24, (ip >> 16) & 0xff, (ip >> 8) & 0xff, ip & 0xff);
        }
        protected static double GateWayRTT(IPAddress gatewayIPAddress)
        {
            Ping pingSender = new Ping ();

            PingOptions options = new PingOptions ();
            options.DontFragment = true;
            options.Ttl = 128;

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;

            List<double> rttList = new List<double>();

            // Ping 5 times the IP address of the default gateway.
            for(int i = 0; i <= 5; i++){
                PingReply reply = pingSender.Send(gatewayIPAddress, timeout, buffer, options);

                if(reply.Status == IPStatus.Success)
                {
                    rttList.Add(reply.RoundtripTime);
                }
            }

            return rttList.Average();
        }
        protected static IPAddress GetDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                .FirstOrDefault();
        }
        protected static uint IpToInt(string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);
            byte[] bytes = address.GetAddressBytes();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }
        protected static async Task<IPAddress> GetMyIpAsync()
        {
            WebClient client = new WebClient();
            string ip = await client.DownloadStringTaskAsync(new Uri("https://checkip.amazonaws.com/"));

            if(!String.IsNullOrEmpty(ip))
            {
                return IPAddress.Parse(ip.Trim());
            }
            else
            {
                return IPAddress.Parse("127.0.0.1");
            }
        }
        protected static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, currentLineCursor);
        }
        #endregion
    }
}
