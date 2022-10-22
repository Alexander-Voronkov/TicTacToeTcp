using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MessagePacket
{
    [Serializable]
    public class MessagePacket
    {
        public Point Cell { get; set; }
        public char Step { get; set; }
        public MessagePacket(Point where, char step)
        {
            Cell = where;
            Step = step;
        }
        public MessagePacket() { }

        public static byte[] ToBytes(MessagePacket mess)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, mess);
                return stream.ToArray();
            }
        }

        public static MessagePacket FromBytes(byte[] arr)
        {
            using (MemoryStream stream = new MemoryStream(arr))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    if (formatter.Deserialize(stream) is MessagePacket message)
                        return message;
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
