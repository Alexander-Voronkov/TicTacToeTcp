using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace GameDll
{
    public class Game
    {
        public TcpClient Player1 { get; set; }
        public TcpClient Player2 { get; set; }
        public int whoseTurn { get; set; }
        public int?[,] field { get; set; }
        
        public bool End { get { return CalculateEnd(); } }

        public int? Winner { get; private set; }
        public Game(int length)
        {
            field = new int?[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j  < length; j ++)
                {
                    field[i, j] = null;
                }
            }
            Winner = null;
            Player1 = null;
            Player2 = null;
            whoseTurn = 1; // 1 - Player1, 2 - Player2
        }

        private bool CalculateEnd()
        {
            if(CheckDiags(0)||CheckRows(0)||CheckCols(0))
            {
                Winner = 0;
                return true;
            }
            else if(CheckDiags(1) || CheckRows(1) || CheckCols(1))
            {
                Winner = 1;
                return true;
            }
            else if (field.IsFull())
            {
                Winner = 2;
                return true;
            }

            Winner = null;
            return false;
        }

        private bool CheckDiags(int symb)
        {
            bool toright=true, toleft=true;
            for (int i = 0; i < field.GetUpperBound(0)+1; i++)
            {
                toright &= (field[i,i] == symb);
                toleft &= (field[field.GetUpperBound(0) - i,i] == symb);
            }

            if (toright || toleft) return true;

            return false;
        }

        private bool CheckCols(int symb)
        {
            bool f;
            for (int i = 0; i < field.GetUpperBound(0) + 1; i++)
            {
                f = true;
                for (int j = 0; j < field.GetUpperBound(0) + 1; j++)
                {
                    f &= (field[i, j] == symb);
                }
                if(f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckRows(int symb)
        {
            bool f;
            for (int i = 0; i < field.GetUpperBound(0) + 1; i++)
            {
                f = true;
                for (int j = 0; j < field.GetUpperBound(0) + 1; j++)
                {
                    f &= (field[j, i] == symb);
                }
                if (f)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static class FieldExtension
    {
        public static bool Contains(this int?[,]field, Point p)
        {
            if (field[p.X, p.Y] != null)
                return true;
            return false;
        }

        public static bool IsFull(this int?[,] field)
        {
            for (int i = 0; i < field.GetUpperBound(0)+1; i++)
            {
                for (int j = 0; j < field.GetUpperBound(1)+1; j++)
                {
                    if (field[i, j] == null)
                        return false;
                }
            }
            return true;
        }

    }
}
