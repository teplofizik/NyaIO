using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace NyaIO.SerialConnection
{
    /// <summary>
    /// Абстрактный парсер поледовательного протокола
    /// </summary>
    public abstract class ProtocolParser
    {
        /// <summary>
        /// Мьютекс
        /// </summary>
        private Mutex PacketMutex = new Mutex();

        /// <summary>
        /// Очередь пакетов
        /// </summary>
        private Queue<ProtocolPacket> Packets = new Queue<ProtocolPacket>();

        /// <summary>
        /// Сброс парсера
        /// </summary>
        public virtual void Reset()
        {
            PacketMutex.WaitOne();
            Packets.Clear();
            PacketMutex.ReleaseMutex();
        }

        /// <summary>
        /// Обработать байты, полуцченные по UART
        /// </summary>
        /// <param name="Data"></param>
        public virtual void Process(byte[] Data)
        {
            foreach (var D in Data)
                ProcessByte(D);
        }

        /// <summary>
        /// Обработка байта
        /// </summary>
        /// <param name="Data"></param>
        protected virtual void ProcessByte(byte Data)
        {

        }

        /// <summary>
        /// Добавить обработанный пакет в очередь выдачи
        /// </summary>
        /// <param name="Packet"></param>
        protected void AddPacket(ProtocolPacket Packet)
        {
            PacketMutex.WaitOne();
            Packets.Enqueue(Packet);
            PacketMutex.ReleaseMutex();
        }

        /// <summary>
        /// Получить распознанный пакет
        /// </summary>
        /// <returns></returns>
        public ProtocolPacket GetParsedPacket()
        {
            ProtocolPacket Res = null;

            PacketMutex.WaitOne();
            if (Packets.Count > 0)
                Res = Packets.Dequeue();

            PacketMutex.ReleaseMutex();

            return Res;
        }
    }
}
