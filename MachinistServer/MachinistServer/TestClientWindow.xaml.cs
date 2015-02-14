using System;
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
using System.Windows.Shapes;
using System.ComponentModel;

namespace MachinistServer
{
    /// <summary>
    /// Interaction logic for TestClientWindow.xaml
    /// </summary>
    public partial class TestClientWindow : Window
    {
        private HostService.TcpIpServer _server;

        delegate void updateCallback(string tekst);


        public TestClientWindow()
        {
            InitializeComponent();

            ClientProxyAgents.TcpIpAgent agent = new ClientProxyAgents.TcpIpAgent();
            DataContext = agent;
        }

        void TestClientWindow_Closing(object sender, CancelEventArgs e)
        {
            Disconnect();
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((ClientProxyAgents.TcpIpAgent)DataContext).HostName = textBoxIP.Text;
                ((ClientProxyAgents.TcpIpAgent)DataContext).Port = Convert.ToInt32(textBoxPort.Text);
                ((ClientProxyAgents.TcpIpAgent)DataContext).Connect();
                ((ClientProxyAgents.TcpIpAgent)DataContext).OnDataReceive += new EventHandler(TestClientWindow_OnDataReceive);

                _server = new HostService.TcpIpServer(3000);
                _server.OnDataReceive += new EventHandler(_server_OnDataReceive);
                _server.OnError += new EventHandler(_server_OnError);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Connection with SERVER failed");
            }
        }

        void _server_OnError(object sender, EventArgs e)
        {
            try
            {
                ErrorEventArg data = (ErrorEventArg)e;
                if (data != null)
                {
                    // show incoming data from the server
                    //UpdateTextBoxResult(data.Error.ToString());
                    
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
                    // show incoming data from the server
                    UpdateTextBoxResult(data.Data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Receive from SERVER failed");
            }
        }

        void UpdateTextBoxResult(string tekst)
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
        }

        void TestClientWindow_OnDataReceive(object sender, EventArgs e)
        {
            // voorlopig niks doen
        }

        private void buttonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            try
            {
                ((ClientProxyAgents.TcpIpAgent)DataContext).Disconnect();
                ((ClientProxyAgents.TcpIpAgent)DataContext).OnDataReceive -= TestClientWindow_OnDataReceive;

                ClientProxyAgents.TcpIpAgent agent = new ClientProxyAgents.TcpIpAgent();
                DataContext = agent;

                if (_server != null)
                {
                    _server.Disconnect();
                    _server = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Disconnection with SERVER failed");
            }
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((ClientProxyAgents.TcpIpAgent)DataContext).Send(textBoxString1.Text, textBoxString2.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Send to SERVER failed");
            }
        }
    }
}
