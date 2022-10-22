using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameDll
{
    public class Game
    {
        public TcpClient Player1 { get; set; }
        public TcpClient Player2 { get; set; }
        public bool whoseTurn { get; set; }
        public int[,] field { get; set; }
        
        public bool End { get { return CalculateEnd(); } }

        public bool? Winner { get; private set; }
        public Game(int length)
        {
            field = new int[length, length];
            Winner = null;
            Player1 = null;
            Player2 = null;
            whoseTurn = true; // true - Player1, false - Player2
        }

        private bool CalculateEnd()
        {
            bool f=false;
            bool? who=null;
            for (int i = 0; i < field.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < field.GetUpperBound(0) + 1; j++)
                {

                }
            }
            if (f)
            {
                Winner = who;
                return f;
            }
            else
            {
                Winner = null;
                return false;
            }
        }
    }
}
