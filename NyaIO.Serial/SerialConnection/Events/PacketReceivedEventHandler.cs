using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NyaIO.SerialConnection.Events
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public ProtocolPacket Packet;

        public PacketReceivedEventArgs(ProtocolPacket Packet)
        {
            this.Packet = Packet;
        }
    }

    public delegate void PacketReceivedEventHandler(object sender, PacketReceivedEventArgs e);
}
