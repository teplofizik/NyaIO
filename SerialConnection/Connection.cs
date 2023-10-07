using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NyaIO.Data;

namespace NyaIO.SerialConnection
{

    /// <summary>
    /// Подключение к устройству
    /// </summary>
    public class Connection
    {
        public event Events.PacketReceivedEventHandler PacketPeceived;

        /// <summary>
        /// Используемый порт
        /// </summary>
        private string Port;

        /// <summary>
        /// Последовательный порт
        /// </summary>
        System.IO.Ports.SerialPort SPort;

        /// <summary>
        /// Запущен ли поток приёма и открыто ли подключение
        /// Флаг, что поток не стоит закрывать
        /// </summary>
        private bool IsRunning = false;

        /// <summary>
        /// Открыто ли подключение к порту
        /// </summary>
        private bool IsConnected = false;

        /// <summary>
        /// Управляющий объект приёмного потока
        /// </summary>
        private Thread ConnectionThread;

        /// <summary>
        /// Парсер протокола
        /// </summary>
        private ProtocolParser Parser;

        /// <summary>
        /// Конструктор соединения
        /// </summary>
        /// <param name="Port">Последовательный порт (COM12 и т.д.)</param>
        /// <param name="Parser">Парсер протокола</param>
        public Connection(string Port, ProtocolParser Parser)
        {
            this.Port = Port;
            this.Parser = Parser;
        }

        /// <summary>
        /// Активно ли подключение к порту
        /// </summary>
        public bool Connected => IsConnected;

        /// <summary>
        /// Открыть подключение
        /// </summary>
        public void Start()
        {
            if (TryOpen())
            {
                IsRunning = true;
                ConnectionThread = new Thread(ThreadLoop);
                ConnectionThread.Start();

                IsConnected = true;
            }
        }

        /// <summary>
        /// Закрыть подключение
        /// </summary>
        public void Stop()
        {
            if (IsConnected)
            {
                IsRunning = false;
                // Ждём завершения...
                ConnectionThread.Join();
                ConnectionThread = null;

                SPort.Close();
                SPort = null;
                IsConnected = false;
            }
        }

        /// <summary>
        /// Запись пакета в порт
        /// </summary>
        /// <param name="Pkt">Пакет</param>
        protected void Write(byte[] Data)
        {
            if (Connected)
            {
                SPort.Write(Data, 0, Data.Length);
            }
        }

        /// <summary>
        /// Попробовать открыть подключение
        /// </summary>
        /// <returns>true, если получилось</returns>
        private bool TryOpen()
        {
            try
            {
                var P = new System.IO.Ports.SerialPort(Port);
                P.BaudRate = 115200;
                P.Open();

                if (P.IsOpen)
                {
                    SPort = P;
                    return true;
                }
            }

            catch (Exception E)
            {
                Debug.WriteLine($"Error on port open: {E.Message}");
            }

            return false;
        }

        /// <summary>
        /// Количество байтов для чтения
        /// </summary>
        private int AvailableBytes => SPort.BytesToRead;

        /// <summary>
        /// Чтение данных из буфера
        /// </summary>
        /// <returns></returns>
        private byte[] ReadExistingBytes()
        {
            var Bytes = AvailableBytes;
            var Res = new byte[Bytes];
            if (Bytes > 0)
            {
                int Readed = SPort.Read(Res, 0, Bytes);
                if (Readed != Bytes)
                {
                    Debug.WriteLine("Count of readed bytes differs from available count.");

                    Res = Res.ReadArray(0, Readed);
                }
            }
            return Res;
        }

        /// <summary>
        /// Поток приёмника данных
        /// </summary>
        private void ThreadLoop()
        {
            while (IsRunning)
            {
                if (AvailableBytes > 0)
                {
                    // Обработаем байты из порта
                    var Readed = ReadExistingBytes();
                    Parser.Process(Readed);

                    // Проверим, не был ли получен пакет
                    var Packet = Parser.GetParsedPacket();
                    if (Packet != null)
                    {
                        PacketPeceived?.Invoke(this, new Events.PacketReceivedEventArgs(Packet));
                    }
                }
                Thread.Sleep(1);
            }
        }
    }
}