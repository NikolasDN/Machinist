using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachinistServer.VisualTrack
{
    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Rotation { get; set; }
        public string NodeTypeName { get; set; }
        public int NodeAddress1 { get; set; }
        public int NodeAddress2 { get; set; }
        public int NodeNr1 { get; set; }
        public int NodeNr2 { get; set; }
    }
}
