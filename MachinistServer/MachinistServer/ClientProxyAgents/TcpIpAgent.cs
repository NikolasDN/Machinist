using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net.Sockets;

namespace MachinistServer.ClientProxyAgents
{
    public class TcpIpAgent : INotifyPropertyChanged, IAgent 
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public bool IsConnected { get; set; }
        public bool IsDisconnected
        {
            get
            {
                return !IsConnected;
            }
        }

        public event EventHandler OnDataReceive;

        private TcpClient _client;

        public TcpIpAgent()
        {
            _client = new TcpClient();
        }

        public void Connect()
        {
            _client.Connect(HostName, Port);
            IsConnected = true;
            this.OnPropertyChanged("IsConnected");
            this.OnPropertyChanged("IsDisconnected");
        }

        // also get something back
        public void Send(string s1, string s2)
        {
            IList<byte> toSend = new List<byte>();
            toSend.Add(Helper.StringToByte(s1));
            if (!string.IsNullOrEmpty(s2))
            {
                toSend.Add(Helper.StringToByte(s2));
            }

            NetworkStream serverStream = _client.GetStream();
            byte[] outStream = toSend.ToArray();
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            // Voorlopig nog niets terugverwachten
            //byte[] inStream = new byte[10025];
            //serverStream.Read(inStream, 0, (int)_client.ReceiveBufferSize);
            //string returndata = System.Text.Encoding.ASCII.GetString(inStream);
            //OnDataReceive(null, new DataEventArg() { Data = returndata });
        }

        public void Disconnect()
        {
            _client.Close();
            IsConnected = false;
            this.OnPropertyChanged("IsConnected");
            this.OnPropertyChanged("IsDisconnected");
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
