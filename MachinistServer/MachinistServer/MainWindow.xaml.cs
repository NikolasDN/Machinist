﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Net;

namespace MachinistServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ClientProxyAgents.Rs232Agent agent = new ClientProxyAgents.Rs232Agent();
        private HostService.TcpIpServer _server;

        delegate void updateCallback(string tekst);
        delegate void updateAndSendCallback(string s1, string s2);
        
        public MainWindow()
        {
            InitializeComponent();

            DisplayIPAddress();
            ClientProxyAgents.Rs232Agent agent = new ClientProxyAgents.Rs232Agent();
            DataContext = agent;
        }

        private void DisplayIPAddress()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList.Where(f => f.AddressFamily.ToString() == "InterNetwork").ToArray();

            Title = "MachinistServer: ";
            for (int i = 0; i < addr.Length; i++)
            {
                Title += addr[i].ToString() + "   ";
            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Disconnect();
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((ClientProxyAgents.Rs232Agent)DataContext).Connect();
                //((ClientProxyAgents.Rs232Agent)DataContext).OnDataReceive += new EventHandler(MainWindow_OnDataReceive);
                //agent.Connect();
                //agent.OnDataReceive += new EventHandler(MainWindow_OnDataReceive);

                _server = new HostService.TcpIpServer(3000);
                _server.OnDataReceive += new EventHandler(_server_OnDataReceive);
                _server.OnError += new EventHandler(_server_OnError);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Connection with TRACK failed");
            }
        }

        
        void _server_OnError(object sender, EventArgs e)
        {
            try
            {
                ErrorEventArg data = (ErrorEventArg)e;
                if (data != null)
                {
                    // show incoming data from clients
                    UpdateSpecialReceiveFromClients(data.Error.ToString());

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Receive from SERVER failed");
            }
        }

        void _server_OnDataReceive(object sender, EventArgs e)
        {
            try
            {
                DataEventArg data = (DataEventArg)e;
                if (data != null)
                {
                    // show incoming data from clients
                    UpdateTextBoxReceiveFromClients(data.Data);

                    // send incoming data to track
                    /*int i1 = (int)data.Data[0];
                    int i2 = 0;
                    if (data.Data.Length > 1)
                    {
                        i2 = (int)data.Data[1];
                    }

                    UpdateInputTextBoxesAndSend(i1.ToString(), i2.ToString());*/
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Receive from SERVER failed");
            }
        }

        //void MainWindow_OnDataReceive(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        DataEventArg data = (DataEventArg)e;
        //        if (data != null)
        //        {
        //            // show incoming data from the track
        //            UpdateTextBoxResult(data.Data);

        //            // send incoming data to clients
        //            int i1 = (int)data.Data[0];
        //            int i2 = 0;
        //            if (data.Data.Length > 1)
        //            {
        //                i2 = (int)data.Data[1];
        //            }

        //            //_server.ReturnToAllClients(i1.ToString(), i2.ToString());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString(), "Receive from TRACK failed");
        //    }
        //}

        void UpdateSpecialReceiveFromClients(string tekst)
        {
            if (textBoxResult.Dispatcher.CheckAccess() == false)
            {
                updateCallback uCallBack = new updateCallback(UpdateSpecialReceiveFromClients);
                this.Dispatcher.Invoke(uCallBack, tekst);
            }
            else
            {
                textBoxServerReceive.AppendText(tekst);
                textBoxServerReceive.AppendText(Environment.NewLine);
                textBoxServerReceive.ScrollToEnd();
            }
        }

        void UpdateTextBoxReceiveFromClients(string tekst)
        {
            if (textBoxResult.Dispatcher.CheckAccess() == false)
            {
                updateCallback uCallBack = new updateCallback(UpdateTextBoxReceiveFromClients);
                this.Dispatcher.Invoke(uCallBack, tekst);
            }
            else
            {
                //update your element here
                /*if (tekst.Length > 2)
                {
                    MessageBox.Show(string.Format("Fout {0}", tekst.Length.ToString()));
                }*/
                Char? firstCommand = null;
                foreach (Char c in tekst)
                {
                    int i = (int)c;
                    textBoxServerReceive.AppendText(i.ToString());
                    textBoxServerReceive.AppendText(Environment.NewLine);
                    textBoxServerReceive.ScrollToEnd();

                    if (firstCommand.HasValue)
                    {
                        string s1 = ((int)(firstCommand.Value)).ToString(); //((int)(tekst[0])).ToString();
                        string s2 = ((int)(c)).ToString(); //((int)(tekst[1])).ToString();

                        if (s1 == "33" || s1 == "34")
                        {
                            s2 = (Convert.ToInt32(s2) - 1).ToString();
                        }
                        textBoxString1.Text = s1;
                        textBoxString2.Text = s2;
                        SendToTrack(s1, s2);

                        firstCommand = null;
                    }
                    else
                    {
                        firstCommand = c;
                    }
                }
            }
        }

        /*void UpdateTextBoxResult(string tekst)
        {
            if (textBoxResult.Dispatcher.CheckAccess() == false)
            {
                updateCallback uCallBack = new updateCallback(UpdateTextBoxResult);
                this.Dispatcher.Invoke(uCallBack, tekst);
            }
            else
            {
                //update your element here
                foreach (Char c in tekst)
                {
                    int i = (int)c;
                    string hexString = Helper.Int32ToHexString(i);
                    string visualByte = Helper.Int32ToVisualByte(i);

                    textBoxResult.AppendText(string.Format("{0} - {1} - {2}", i.ToString(), hexString, visualByte));
                    textBoxResult.AppendText(Environment.NewLine);
                    textBoxResult.ScrollToEnd();
                }
            }
        }*/

        void UpdateInputTextBoxesAndSend(string s1, string s2)
        {
            if (textBoxString1.Dispatcher.CheckAccess() == false)
            {
                updateAndSendCallback uCallBack = new updateAndSendCallback(UpdateInputTextBoxesAndSend);
                this.Dispatcher.Invoke(uCallBack, s1, s2);
            }
            else
            {
                if (s1 == "33" || s1 == "34")
                {
                    s2 = (Convert.ToInt32(s2) - 1).ToString();
                }
                textBoxString1.Text = s1;
                textBoxString2.Text = s2;
                SendToTrack(s1, s2);
            }
        }

        private void buttonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            try
            {
                ((ClientProxyAgents.Rs232Agent)DataContext).Disconnect();
                //((ClientProxyAgents.Rs232Agent)DataContext).OnDataReceive -= MainWindow_OnDataReceive;
                //agent.Disconnect();
                //agent.OnDataReceive -= MainWindow_OnDataReceive;

                if (_server != null)
                {
                    _server.Disconnect();
                    _server = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Disconnection with TRACK failed");
            }
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            SendToTrack(textBoxString1.Text, textBoxString2.Text);
        }

        private void SendToTrack(string s1, string s2)
        {
            try
            {
                ((ClientProxyAgents.Rs232Agent)DataContext).Send(s1, s2);
                //agent.Send(s1, s2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Send to TRACK failed");
            }
        }

        private void buttonTestClient_Click(object sender, RoutedEventArgs e)
        {
            TestClientWindow w = new TestClientWindow();
            w.Show();
        }

        private void buttonDrawing_Click(object sender, RoutedEventArgs e)
        {
            VisualTrack.TrackWindow w = new VisualTrack.TrackWindow();
            w.Show();
        }

        private void buttonSendToClients_Click(object sender, RoutedEventArgs e)
        {
            _server.ReturnToAllClients("120", "120");
        }

        private void buttonBigScreen_Click(object sender, RoutedEventArgs e)
        {
            BigScreen.BigScreenWindow w = new BigScreen.BigScreenWindow();
            w.Show();
        }
    }
}
