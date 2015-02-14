using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachinistServer.VisualTrack
{
    public class NodeType
    {
        public string Name { get; set; }
        public byte[] Bitmap1 { get; set; }
        public byte[] Bitmap2 { get; set; }
        public int StateAddress1 { get; set; }
        public int StateAddress2 { get; set; }
    }
}
