using MessagePacketDll;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicTacToe
{
    public partial class Form1 : Form
    {
        TcpClient client;
        bool first;
        public Form1()
        {
            InitializeComponent();
            Task.Run(Init);
        }

        private void DrawField(int length,int cellsize)
        {
            if(InvokeRequired)
            Invoke(new Action(() => {
                this.Height = cellsize * length+cellsize;
                this.Width = cellsize * length+cellsize;
            }));
            else
            {
                this.Height = cellsize * length + cellsize;
                this.Width = cellsize * length + cellsize;
            }
            
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    var temp = new Button() { Left = cellsize * j+50, Top = cellsize * i+50, Tag = $"{i}\n{j}", Height=cellsize,Width=cellsize };
                    temp.Click += Play;
                    if(InvokeRequired)
                        Invoke(new Action(() => { this.Controls.Add(temp); }));
                    else
                        this.Controls.Add(temp);
                }
            }
        }

        private int[] ConnectWait()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => {
                    Controls.Add(new Label() { Text = "Waiting for server response...", Top = 150,Tag="WL", Left=150, Width = 300 });
                    Controls.Add(new ProgressBar() { Style=ProgressBarStyle.Marquee, Tag="WPB",Top=200,Left=200 });
                }));
            }
            else
            {
                Controls.Add(new Label() { Text = "Waiting for server response...", Top = 150, Tag = "WL", Left = 150, Width = 300 }); ;
                Controls.Add(new ProgressBar() { Style = ProgressBarStyle.Marquee, Tag = "WPB", Left = 200 });
            }
            try
            {
                client = new TcpClient();
                client.Connect("127.0.0.1", 1000);
            }
            catch 
            {
                MessageBox.Show("Server is currently off! Try later!");
                return null;
            }
            MessageBox.Show("Successfully connected to server!");
            var ns = client.GetStream();
            byte[] buff = new byte[256];
            int bytes;
            var sb = new StringBuilder();
            do
            {
                bytes=ns.Read(buff,0,buff.Length);
                sb.Append(Encoding.UTF8.GetString(buff,0,bytes));
            } while (ns.DataAvailable);
            var res = sb.ToString().Split('\n');
            if (res[0] == "SUCCESS" && res.Length == 3)
            {
                try
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => {
                            Controls.Cast<Control>().Where(x => x.Tag.ToString() == "WL").ToList()[0].Text = "Waiting for available players...";
                        }));
                    }
                    else
                    {
                        Controls.Cast<Control>().Where(x => x.Tag.ToString() == "WL").ToList()[0].Text = "Waiting for available players...";
                    }
                    buff = Encoding.UTF8.GetBytes("OK");
                    ns.Write(buff, 0, buff.Length);

                    return new int[] { int.Parse(res[1]), int.Parse(res[2]) };
                }
                catch 
                {
                    MessageBox.Show("An error occured!"); 
                    return null;
                }
            }
            MessageBox.Show("An error occured!");
            return null;
        }

        private void Init()
        {
            int[] res = ConnectWait();
            if (res == null)
            {
                if (InvokeRequired)
                    Invoke(new Action(() => Close()));
                else
                    Close();
                return;
            }
            byte[] buff = new byte[256];
            int bytes;
            var ns = client.GetStream();
            bool f = false;
            do
            {
                var sb = new StringBuilder();
                do
                {
                    bytes = ns.Read(buff, 0, buff.Length);
                    sb.Append(Encoding.UTF8.GetString(buff, 0, bytes));
                } while (ns.DataAvailable);
                if (sb.ToString() == "WAIT")
                {
                    continue;
                }
                else f = true;
            } while (!f);

            if (InvokeRequired)
                Invoke(new Action(() => Controls.Cast<Control>().Where(c => c.Tag.ToString() == "WL" || c.Tag.ToString() == "WPB").ToList().ForEach(x => Controls.Remove(x))));
            else
                Controls.Cast<Control>().Where(c => c.Tag.ToString() == "WL" || c.Tag.ToString() == "WPB").ToList().ForEach(x => Controls.Remove(x));
            DrawField(res[0], res[1]);
            Task.Run(() => WaitForResponse(ns));
        }

        private void Play(object sender, EventArgs ea)
        {
            try
            {
                var ns=client.GetStream();
                var arr = (sender as Button).Tag.ToString().Split('\n');
                byte[] buff = MessagePacket.ToBytes(new MessagePacket(new Point(int.Parse(arr[0]), int.Parse(arr[1]))));
                ns.Write(buff,0,buff.Length);
                SwitchBtn(false);
            }
            catch
            {
                MessageBox.Show("Error");
                return;
            }
        }

        private void SwitchBtn(bool f)
        {
            Invoke(new Action(()=> { Controls.Cast<Control>().Where(x => x is Button).ToList().ForEach(x => x.Enabled = f); }));
        }

        private void WaitForResponse(NetworkStream ns)
        {
            while (true)
            {
                MessagePacket mp;
                byte[] buff = new byte[256];
                int bytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    do
                    {
                        bytes = ns.Read(buff, 0, buff.Length);
                        ms.Write(buff, 0, bytes);
                    } while (ns.DataAvailable);
                    mp = MessagePacket.FromBytes(ms.ToArray());
                }
                if(mp==null)
                {
                    MessageBox.Show("ERROR");
                    continue;
                }
                if (mp.Message == "WIN")
                {
                    MessageBox.Show("You win! Congratulations!");
                    if (InvokeRequired)
                        Invoke(new Action(() => Close()));
                    else
                        Close();
                    return;
                }
                else if (mp.Message == "LOSE")
                {
                    MessageBox.Show("You lost. Better luck next time!");
                    if (InvokeRequired)
                        Invoke(new Action(() => Close()));
                    else
                        Close();
                    return;
                }
                else if (mp.Message == "DRAW")
                {
                    MessageBox.Show("Draw. Your enemy is not that bad!");
                    if (InvokeRequired)
                        Invoke(new Action(() => Close()));
                    else
                        Close();
                    return;
                }
                else if (mp.Message == "NOT YOUR TURN")
                {
                    MessageBox.Show("Not your turn! Wait for your opponent's step.");
                    SwitchBtn(true);
                    continue;
                }
                else if (mp.Message == "ENEMY DISCONNECTED")
                {
                    MessageBox.Show("Enemy disconnected.");
                    try
                    {
                        client.Close();
                    }
                    catch { }
                    if (InvokeRequired)
                        Invoke(new Action(() => Close()));
                    else
                        Close();
                    return;
                }
                else if(mp.Message=="This field is busy!")
                {
                    MessageBox.Show(mp.Message);
                    SwitchBtn(true);
                    continue;
                }
                if (InvokeRequired)
                    Invoke(new Action(() =>
                    {
                        Controls.Cast<Control>().Where(x => x.Tag.ToString() == $"{mp.Cell.X}\n{mp.Cell.Y}").ToList()[0].Text = mp.Message;
                    }));
                else
                    Controls.Cast<Control>().Where(x => x.Tag.ToString() == $"{mp.Cell.X}\n{mp.Cell.Y}").ToList()[0].Text = mp.Message;
                SwitchBtn(true);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                byte[] buff = MessagePacket.ToBytes(new MessagePacket() { Message = "DISCONNECT" });
                client.GetStream().Write(buff, 0, buff.Length);
            }
            catch 
            {

            }
        }
    }
}
