using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicTacToe
{
    public partial class Form1 : Form
    {
        TcpClient client;
        public Form1()
        {
            InitializeComponent();
            Task.Run(Init);
        }

        private void DrawField(int length,int cellsize)
        {
            this.Size = new Size(cellsize*length+cellsize, cellsize * length + cellsize);
            for (int i = 1; i < length+1; i++)
            {
                for (int j = 1; j < length+1; j++)
                {
                    this.Controls.Add(new Button() {Left=cellsize*j,Top= cellsize * i, Tag=$"{i}\n{j}"});
                }
            }
        }

        private int[] ConnectWait()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => {
                    Controls.Add(new Label() { Text = "Waiting for server response...", Top = 150,Tag="WL" });
                    Controls.Add(new ProgressBar() { Style=ProgressBarStyle.Marquee, Tag="WPB",Top=200 });
                }));
            }
            else
            {
                Controls.Add(new Label() { Text = "Waiting for server response...", Top = 150, Tag = "WL" });
                Controls.Add(new ProgressBar() { Style = ProgressBarStyle.Marquee, Tag = "WPB" });
            }
            try
            {
                client = new TcpClient("127.0.0.1", 1000);
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
                this.Close();
                return;
            }
            byte[] buff = new byte[256];
            int bytes;
            using (var ns = client.GetStream())
            {
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
                    else
                    {
                        f = true;
                    }
                } while (!f);
            }

            if (InvokeRequired)
                Invoke(new Action(() => Controls.Cast<Control>().Where(c => c.Tag.ToString() == "WL" || c.Tag.ToString() == "WPB").ToList().ForEach(x => Controls.Remove(x))));
            else
                Controls.Cast<Control>().Where(c => c.Tag.ToString() == "WL" || c.Tag.ToString() == "WPB").ToList().ForEach(x => Controls.Remove(x));
            DrawField(res[0], res[1]);
            Task.Run(Play);
        }

        private void Play()
        {
            try
            {
               
            }
            catch
            {
                
            }
        }

        private void SwitchField(bool f)
        {
            Controls.Cast<Control>().Where(x=>x is Button).ToList().ForEach(x => (x as Button).Enabled = f);
        }
    }
}
