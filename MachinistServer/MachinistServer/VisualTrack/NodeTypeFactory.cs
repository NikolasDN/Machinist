using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachinistServer.VisualTrack
{
    public class NodeTypeFactory
    {
        public List<NodeType> MakeNodeTypes()
        {
            List<NodeType> result = new List<NodeType>();

            result.Add(new NodeType() { Bitmap1 = Helper.BmpToBytes(Properties.Resources.dubbelekruiswissel_1), Bitmap2 = Helper.BmpToBytes(Properties.Resources.dubbelekruiswissel_2), Name = "dubbelekruiswissel" });
            result.Add(new NodeType() { Bitmap1 = Helper.BmpToBytes(Properties.Resources.dubbelekruiswissel_3), Bitmap2 = Helper.BmpToBytes(Properties.Resources.dubbelekruiswissel_4), Name = "dubbelekruiswissel" });
            result.Add(new NodeType() { Bitmap1 = Helper.BmpToBytes(Properties.Resources.enkelekruiswissel_1), Bitmap2 = Helper.BmpToBytes(Properties.Resources.enkelekruiswissel_2), Name = "enkelekruiswissel" });
            result.Add(new NodeType() { Bitmap1 = Helper.BmpToBytes(Properties.Resources.linkerwissel_1), Bitmap2 = Helper.BmpToBytes(Properties.Resources.linkerwissel_2 ), Name = "linkerwissel" });
            result.Add(new NodeType() { Bitmap1 = Helper.BmpToBytes(Properties.Resources.rechterwissel_1), Bitmap2 = Helper.BmpToBytes(Properties.Resources.rechterwissel_2), Name = "rechterwissel" });
            
            return result;
        }
    }
}
