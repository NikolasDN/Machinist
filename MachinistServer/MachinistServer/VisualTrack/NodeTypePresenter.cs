using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;

namespace MachinistServer.VisualTrack
{
    public class NodeTypePresenter
    {
        public string Name { get; set; }
        public System.Windows.Media.Imaging.BitmapImage Bitmap1 { get; set; }
        public System.Windows.Media.Imaging.BitmapImage Bitmap2 { get; set; }
        public int StateAddress1 { get; set; }
        public int StateAddress2 { get; set; }
    }
}
