using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NyaIO.Data;

namespace NyaIO.SerialConnection
{
    /// <summary>
    /// Какой-то пакет последовательного протокола
    /// </summary>
    public class ProtocolPacket : RawPacket
    {
        public ProtocolPacket(byte[] Raw) : base(Raw)
        {

        }

        public ProtocolPacket(int Size) : base(Size)
        {

        }
    }

}
