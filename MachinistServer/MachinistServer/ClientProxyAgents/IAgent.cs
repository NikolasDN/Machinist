using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachinistServer.ClientProxyAgents
{
    public interface IAgent
    {
        void Connect();
        void Send(string s1, string s2);
        event EventHandler OnDataReceive;
        void Disconnect();

    }
}
