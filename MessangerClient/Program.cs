using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient
{
    class Program
    {
        private static string Adres = "192.168.88.186";
        private static int port = 8005;
        static int buffersize = 1024;
        static TcpClient tcpClient;
        static NetworkStream stream;
        static void Main(string[] args)
        {
            try
            {
                tcpClient = new TcpClient();
                try
                {
                    tcpClient.Connect(IPAddress.Parse(Adres), port);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                }

                if (tcpClient.Connected)
                    Console.WriteLine("connected");

                stream = tcpClient.GetStream();

                byte[] datalength = new byte[8];
                var bytes = stream.Read(datalength, 0, 8);
                var bytesleft = BitConverter.ToInt64(datalength, 0);

                byte[] filenamelength = new byte[4];
                stream.Read(filenamelength, 0, 4);
                var namelength = BitConverter.ToInt32(filenamelength, 0);

                byte[] name = new byte[namelength];
                stream.Read(name, 0, namelength);
                string nameoffile = Encoding.UTF8.GetString(name);

                Console.WriteLine(nameoffile);

                var numberofpackages = bytesleft / buffersize;

                //byte[] data = new byte[bytesleft];
                //stream.Read(data, 0, data.Length);
                byte[] buffer = new byte[buffersize];
                var username = Environment.UserName;

                int bytesread = 0;
                using (FileStream fs = new FileStream($@"C:\Users\{username}\Desktop\{nameoffile}", FileMode.Create))
                {
                    while (bytesleft > 0)
                    {
                        if (stream.DataAvailable)
                        {
                            int nextpacketsize = 0;
                            if (bytesleft > buffersize)
                            {
                                nextpacketsize = buffersize;
                            }
                            else
                            {
                                nextpacketsize = (int) bytesleft;
                            }

                            var bytred = stream.Read(buffer, 0, nextpacketsize);
                            fs.Write(buffer, 0, bytred);
                            bytesread += bytred;
                            bytesleft -= bytred;
                            var percent = (bytesread / buffersize) * 100 / numberofpackages;
                            Console.WriteLine(percent + "%");
                        }

                    }
                }

                
                

                //var username = Environment.UserName;
                //File.WriteAllBytes($@"C:\Users\{username}\Desktop\{nameoffile}", data);
                Console.WriteLine("complete");

                stream.Close();
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace + ex.Source + ex.TargetSite);
            }
            finally
            {
                if(stream!=null)
                    stream.Close();
                if(tcpClient!=null)
                    tcpClient.Close();
            }

            Console.ReadLine();
        }
    }
}
