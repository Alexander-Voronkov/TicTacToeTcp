using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GameDll;
using MessagePacket;
namespace ServerTicTacToe
{
    internal class Program
    {
        static TcpListener server;
        static List<Game> Players;
        static int length;
        static int cellsize;
        static void Main(string[] args)
        {
            string s, s1;
            do
            {
                Console.Clear();
                Console.WriteLine("Enter field size");
                s = Console.ReadLine();
                Console.WriteLine("Enter cell size");
                s1 = Console.ReadLine();
            } while (!int.TryParse(s, out length) || !int.TryParse(s1, out cellsize));
            Players = new List<Game>();
            server = new TcpListener(IPAddress.Parse("127.0.0.1"),1000);
            server.Start();
            server.BeginAcceptTcpClient(Callback, server);
        }

        static void Callback(IAsyncResult iar)
        {
            try
            {
                var serv = iar.AsyncState as TcpListener;
                var client = serv.EndAcceptTcpClient(iar);
                serv.BeginAcceptTcpClient(Callback,serv);
                var ns = client.GetStream();
                byte[] buff;
                int bytes;
                var sb = new StringBuilder();
                buff = Encoding.UTF8.GetBytes($"SUCCESS\n{length}\n{cellsize}");
                ns.Write(buff, 0, buff.Length);
                bytes = ns.Read(buff, 0, buff.Length);
                bool f = true;
                do
                {
                    if (Players.Where(x => x.Player2 == null).Count() == 0)
                    {
                        if (f)
                        {
                            buff = Encoding.UTF8.GetBytes("WAIT");
                            ns.Write(buff, 0, buff.Length);
                            Players.Add(new Game(length) { Player1 = client });
                            f = false;
                        }
                    }
                } while (Players.Where(x => x.Player2 == null).Count() == 0);
                buff = Encoding.UTF8.GetBytes("READY");
                ns.Write(buff, 0, buff.Length);
                var key = Players.Where(x => x.Player2 == null).ToList()[0];
                key.Player2 = client;
                buff = Encoding.UTF8.GetBytes("YOUR TURN");
                var ns1 = key.Player1.GetStream();
                ns1.Write(buff, 0, buff.Length);
                Task.Run(()=>Serve(key));
            }
            catch
            {
                return;
            }
        }
        static void Serve(Game game)
        {
            do
            {

            }while(!game.End)
        }
    }
}
