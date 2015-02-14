using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachinistServer.VisualTrack
{
    public class Track
    {
        public List<Rail> Rails { get; set; }
        //public List<NodeType> NodeTypes { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Train> Trains { get; set; }
    }
}
