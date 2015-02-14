using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Data;
using System.ComponentModel;
using System.Threading;

namespace MachinistServer.ClientProxyAgents
{
    public class Rs232Agent : INotifyPropertyChanged, IAgent 
    {
        private Queue<byte[]> rs232Queue;
        //private List<byte[]> queueList;
        //private int counter = 0;

        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public bool IsConnected { get; set; }
        public Handshake Handshake { get; set; }
        public bool RtsEnable { get; set; }
        public bool DtrEnable { get; set; }
        public bool IsDisconnected
        {
            get
            {
                return !IsConnected;
            }
        }

       //private Thread t;
        
        private SerialPort _port;
        
        public event EventHandler OnDataReceive;

        public Rs232Agent()
        {
            PortName = (string)PortNames.CurrentItem;
            BaudRate = 2400;
            Parity = System.IO.Ports.Parity.None;
            DataBits = 8;
            StopBits = System.IO.Ports.StopBits.One;
            Handshake = System.IO.Ports.Handshake.None;
            RtsEnable = true;

        }

        private void ProcesQueue()
        {
            while (rs232Queue != null)
            //while (queueList != null)
            {
                if (rs232Queue.Count > 0)
                //if (queueList.Count() > counter)
                {
                    byte[] bytes = rs232Queue.Dequeue();
                    //byte[] bytes = queueList[counter];
                    //counter++;
                    _port.Write(bytes, 0, bytes.Length);
                    //List<string> log = new List<string>();
                    //log.Add(bytes[0].ToString());
                    //log.Add(bytes[1].ToString());
                    //System.IO.File.AppendAllLines("c:\\temp\\rs232.log", log);
                }
                Thread.Sleep(250);
            }
        }

        //public void TestThread()
        //{
        //    while (true)
        //    {
        //        Thread.Sleep(10000);
        //        if (OnDataReceive != null)
        //        {
        //            OnDataReceive(null, new DataEventArg() { Data = "Jo" });
        //            /*char[] c = new char[127];
        //            string s = new string(c);
        //            OnDataReceive(null, new DataEventArg() { Data = s + s });*/
        //        }
        //        Thread.Sleep(10000);
        //        if (OnDataReceive != null)
        //        {
        //            OnDataReceive(null, new DataEventArg() { Data = "Ju" });
        //        }
        //        Thread.Sleep(10000);
        //        if (OnDataReceive != null)
        //        {
        //            OnDataReceive(null, new DataEventArg() { Data = "Ev" });
        //        }
        //        Thread.Sleep(10000);
        //        if (OnDataReceive != null)
        //        {
        //            OnDataReceive(null, new DataEventArg() { Data = "Ni" });
        //        }
        //    }
        //}

        public void Connect()
        {
            _port = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
            _port.Handshake = Handshake;
            _port.RtsEnable = RtsEnable;
            _port.DtrEnable = DtrEnable;
            //_port.DataReceived += new SerialDataReceivedEventHandler(_port_DataReceived);
            if (!_port.IsOpen && !IsConnected)
            {
                _port.Open();
                IsConnected = true;
                this.OnPropertyChanged("IsConnected");
                this.OnPropertyChanged("IsDisconnected");

                rs232Queue = new Queue<byte[]>();
                //queueList = new List<byte[]>();
                Thread t = new Thread(ProcesQueue);
                t.Start();

                // track simulation
                //t = new Thread(TestThread);
                //t.Start();
            }
        }

        //public void testReceive(string text)
        //{
        //    OnDataReceive(new DataEventArg() { Data = text }, null);
        //}

        void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(1000);
                //string data = _port.ReadLine();
                //byte[] result = new byte[2];
                //_port.Read(result, 0, result.Length);
                //string data = result.ToString();
                string data = _port.ReadExisting();
                OnDataReceive(null, new DataEventArg() { Data = data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Disconnect()
        {
            if (_port != null)
            {
                _port.Close();
                _port = null;
                IsConnected = false;
                this.OnPropertyChanged("IsConnected");
                this.OnPropertyChanged("IsDisconnected");
            }

            if (rs232Queue != null)
            //if (queueList != null)
            {
                rs232Queue.Clear();
                //queueList.Clear();
                rs232Queue = null;
                //queueList = null;
            }


            //if (t != null)
            //{
            //    if (t.IsAlive)
            //    {
            //        t.Abort();
            //        t = null;
            //    }
            //}
        }

        private void Send(byte[] bytes)
        {
            if (_port != null)
            {
                rs232Queue.Enqueue(bytes);
                //queueList.Add(bytes);
            }
            else
            {
                throw new Exception("No port instance");
            }
        }

        public void Send(string s1, string s2)
        {
            IList<byte> toSend = new List<byte>();
            toSend.Add(Helper.StringToByte(s1));
            if (!string.IsNullOrEmpty(s2))
            {
                toSend.Add(Helper.StringToByte(s2));
            }
            Send(toSend.ToArray());
        }

        public CollectionView PortNames
        {
            get
            {
                IList<string> portNames = new List<string>(SerialPort.GetPortNames());
                return new CollectionView(portNames);
            }
        }

        public CollectionView Parities
        {
            get
            {
                IList<Parity> parities = Helper.EnumToList<Parity>();
                return new CollectionView(parities);
            }
        }

        public CollectionView StopBitList
        {
            get
            {
                IList<StopBits> stopBitList = Helper.EnumToList<StopBits>();
                return new CollectionView(stopBitList);
            }
        }

        public CollectionView Handshakes
        {
            get
            {
                IList<Handshake> handshakes = Helper.EnumToList<Handshake>();
                return new CollectionView(handshakes);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
