using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MachinistServer.HostService
{
    public class TcpIpServer
    {
        private int Port { get; set; }
        public event EventHandler OnDataReceive;
        public event EventHandler OnError;
        //public event EventHandler OnNewClientConnect;

        private TcpListener _server;
        private Thread ListenThread;

        public Dictionary<string, TcpClient> ClientList;

        public bool IsConnected { get; set; }
        public bool IsDisconnected
        {
            get
            {
                return !IsConnected;
            }
        }

        public TcpIpServer(int port)
        {
            //string host = Dns.GetHostName();
            //IPHostEntry ipEntry = Dns.GetHostEntry(host);
            //IPAddress[] addr = ipEntry.AddressList;

            ClientList = new Dictionary<string, TcpClient>();
            Port = port;
            
            _server = new TcpListener(IPAddress.Any, Port);
            ListenThread = new Thread(new ThreadStart(ListenForClients));
            ListenThread.Start();
            IsConnected = true;
        }

        public void Disconnect()
        {
            if (_server != null)
            {
                _server.Stop();
                _server = null;
            }
            if (ListenThread != null)
            {
                ListenThread.Abort();
                ListenThread = null;
            }
            IsConnected = false;
        }

        private void ListenForClients()
        {
            _server.Start();

            while (true)
            {
                TcpClient client = null;
                try
                {
                    //blocks until a client has connected to the server
                    client = _server.AcceptTcpClient();
                }
                catch (Exception ex)
                {
                    OnError(this, new ErrorEventArg() { Error = ex });
                }
                    //add new clients to the client list
                    //if (!ClientList.Contains(client))
                    //{
                    //    ClientList.Add(client);
                    //}
                    //if (client != null && client.Connected)
                    //{
                if (client != null)
                {
                    try
                    {
                        IPEndPoint remoteEP = (IPEndPoint)client.Client.RemoteEndPoint;
                        if (!ClientList.ContainsKey(remoteEP.Address.ToString()))
                        {
                            TcpClient newClient = new TcpClient();
                            //newClient.Connect(remoteEP.Address, 3000);
                            ClientList.Add(remoteEP.Address.ToString(), newClient);
                            //OnNewClientConnect(this, new DataEventArg() { Data = remoteEP.Address.ToString() });
                        }

                        //create a thread to handle communication
                        //with connected client
                        Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                        clientThread.Start(client);

                        // connect to smartphone server to send wissel-info
                        //ClientProxyAgents.TcpIpAgent agent = new ClientProxyAgents.TcpIpAgent();
                        //agent.HostName = remoteEP.Address.ToString();
                        //agent.Port =3010;
                        //agent.Connect();
                        //agent.OnDataReceive += new EventHandler(TestClientWindow_OnDataReceive);

                    }
                    catch (Exception ex)
                    {
                        OnError(this, new ErrorEventArg() { Error = ex });
                    }

                    
                }
                 
            }
            
        }

        public void ReturnToAllClients(string s1, string s2)
        {
            IList<byte> toSend = new List<byte>();
            toSend.Add(Helper.StringToByte(s1));
            if (!string.IsNullOrEmpty(s2))
            {
                toSend.Add(Helper.StringToByte(s2));
            }
            byte[] outStream = toSend.ToArray();

            List<string> clientsToRemove = new List<string>();
            foreach (KeyValuePair<string, TcpClient> client in ClientList)
            {
                if (client.Value != null && client.Value.Connected)
                {
                    if (!client.Value.Connected)
                    {
                        IPEndPoint remoteEP = (IPEndPoint)client.Value.Client.RemoteEndPoint;
                        client.Value.Connect(remoteEP);
                    }
                    try
                    {
                        NetworkStream serverStream = client.Value.GetStream();
                        serverStream.Write(outStream, 0, outStream.Length);
                        serverStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        clientsToRemove.Add(client.Key);
                        OnError(this, new ErrorEventArg() { Error = ex });
                    }
                }
                if (client.Value == null)
                {
                    IPEndPoint remoteEP = (IPEndPoint)client.Value.Client.RemoteEndPoint;
                    clientsToRemove.Add(remoteEP.Address.ToString());
                }
                //if (client == null)
                //{
                //    clientsToRemove.Add(client);
                //}
            }
            foreach (string client in clientsToRemove)
            {
                ClientList.Remove(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                if (OnDataReceive != null)
                {
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    OnDataReceive(null, new DataEventArg() { Data = encoder.GetString(message, 0, bytesRead) });
                }
            }

            tcpClient.Close();
        }
    }
}
