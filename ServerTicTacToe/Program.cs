using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GameDll;
using MessagePacketDll;
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
            } while (!int.TryParse(s, out length) || !int.TryParse(s1, out cellsize)||cellsize<100||length<3);
            Players = new List<Game>();
            server = new TcpListener(IPAddress.Parse("127.0.0.1"),1000);
            server.Start();
            server.BeginAcceptTcpClient(Callback, server);
            Console.WriteLine("Serving players...");
            Console.ReadLine();
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
                if (Players.Where(x => x.Player2 == null).Count() == 0)
                {
                    buff = Encoding.UTF8.GetBytes("WAIT");
                    ns.Write(buff, 0, buff.Length);
                    Players.Add(new Game(length) { Player1 = client });
                    Task.Run(() => Serve(Players.Where(x => x.Player2 == null).ToList()[0],1));
                    return;
                }
                buff = Encoding.UTF8.GetBytes("READY");
                ns.Write(buff, 0, buff.Length);
                var game = Players.Where(x => x.Player2 == null).ToList()[0];
                game.Player2 = client;
                buff = Encoding.UTF8.GetBytes("YOUR TURN");
                var ns1 = game.Player1.GetStream();
                ns1.Write(buff, 0, buff.Length);
                Task.Run(() => Serve(game,2));
            }
            catch
            {
                return;
            }
        }
        static void Serve(Game game, int player)
        {
            try
            {
                do
                {
                    byte[] buff = new byte[256];
                    NetworkStream ns;
                    if (player == 1)
                        ns = game.Player1.GetStream();
                    else
                        ns = game.Player2.GetStream();
                    int bytes;
                    MessagePacket mp;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        do
                        {
                            bytes = ns.Read(buff, 0, buff.Length);
                            ms.Write(buff, 0, bytes);
                        } while (ns.DataAvailable);
                        mp = MessagePacket.FromBytes(ms.ToArray());
                    }
                    if (mp == null)
                    {
                        buff = Encoding.UTF8.GetBytes("ERROR");
                        ns.Write(buff, 0, buff.Length);
                        continue;
                    }
                    if(mp.Cell!=null&&game.whoseTurn!=player)
                    {
                        buff = MessagePacket.ToBytes(new MessagePacket() { Message = "NOT YOUR TURN" });
                        ns.Write(buff, 0, buff.Length);
                        continue;
                    }
                    if(mp.Message=="DISCONNECT")
                    {
                        try
                        {
                            if (player == 1)
                                game.Player2.GetStream().Write(buff, 0, buff.Length);
                            else
                                game.Player1.GetStream().Write(buff, 0, buff.Length);
                        }
                        catch
                        {
                        }
                        Players.Remove(game);
                        return;
                    }
                    if (!game.field.Contains(mp.Cell))
                    {
                        if (game.whoseTurn == 1)
                        {
                            game.field[mp.Cell.X, mp.Cell.Y] = 0;
                            game.whoseTurn = 2;
                            var ns1 = game.Player2.GetStream();
                            buff = MessagePacket.ToBytes(new MessagePacket(mp.Cell) { Message="o"});
                            ns1.Write(buff,0,buff.Length);
                            ns.Write(buff,0,buff.Length);
                            game.Player2.GetStream().Write(buff,0,buff.Length);
                        }
                        else
                        {
                            game.field[mp.Cell.X, mp.Cell.Y] = 1;
                            game.whoseTurn = 1;
                            var ns1 = game.Player1.GetStream();
                            buff = MessagePacket.ToBytes(new MessagePacket(mp.Cell) { Message = "x" });
                            ns1.Write(buff, 0, buff.Length);
                            ns.Write(buff, 0, buff.Length);
                            game.Player1.GetStream().Write(buff, 0, buff.Length);
                        }
                    }
                    else
                    {
                        buff = MessagePacket.ToBytes(new MessagePacket() { Message = "This field is busy!" });
                        ns.Write(buff, 0, buff.Length);
                    }
                } while (!game.End);
                if(game.Winner==2)
                {
                    byte[] buff = MessagePacket.ToBytes(new MessagePacket() { Message="DRAW"});
                    game.Player1.GetStream().Write(buff,0,buff.Length);
                    game.Player2.GetStream().Write(buff,0,buff.Length);
                }
                else if(game.Winner == 0)
                {
                    byte[] buff = MessagePacket.ToBytes(new MessagePacket() { Message = "WIN" });
                    game.Player1.GetStream().Write(buff, 0, buff.Length);
                    buff = MessagePacket.ToBytes(new MessagePacket() { Message = "LOSE" });
                    game.Player2.GetStream().Write(buff, 0, buff.Length);
                }
                else if (game.Winner == 1)
                {
                    byte[] buff = MessagePacket.ToBytes(new MessagePacket() { Message = "LOSE" });
                    game.Player1.GetStream().Write(buff, 0, buff.Length);
                    buff = MessagePacket.ToBytes(new MessagePacket() { Message = "WIN" });
                    game.Player2.GetStream().Write(buff, 0, buff.Length);
                }
            }
            catch 
            {
                byte[] buff = MessagePacket.ToBytes(new MessagePacket() { Message = "ENEMY DISCONNECTED" });
                try
                {
                    if(player==1)
                        game.Player2.GetStream().Write(buff, 0, buff.Length);
                    else
                        game.Player1.GetStream().Write(buff, 0, buff.Length);
                }
                catch
                {
                }
                Players.Remove(game);
                return; 
            }
        }
    }
}
