using MachinistServer.ClientProxyAgents;
using MachinistServer.VisualTrack;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace MachinistServer.BigScreen
{
    /// <summary>
    /// Interaction logic for BigScreenWindow.xaml
    /// </summary>
    public partial class BigScreenWindow : Window
    {
        private int _iterator = 0;
        private int _gridSize = 10;
        private Dictionary<string, Node> _nodeInfo = new Dictionary<string, Node>();
        private Rs232Agent _agent;

        public BigScreenWindow(Rs232Agent agent)
        {
            InitializeComponent();

            _agent = agent;
            Initialize();
        }

        private void Initialize()
        {
            string fileName = GetFileNameToOpen();

            if (!string.IsNullOrEmpty(fileName))
            {
                Track track = new Track();

                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(track.GetType());
                System.IO.FileStream fs = new FileStream(fileName, FileMode.Open);
                track = (Track)x.Deserialize(fs);
                fs.Close();

                TrackToXaml(track);
            }
        }

        private string GetFileNameToOpen()
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Tracks (.xml)|*.xml";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                return dlg.FileName;
            }

            return null;
        }

        private void TrackToXaml(Track track)
        {
            AlterCoordinatesAndCanvasSize(track);

            _iterator = 0;

            // clear
            List<Line> lines = MyCanvas.Children.OfType<Line>().ToList();
            foreach (Line l in lines)
            {
                MyCanvas.Children.Remove(l);
            }
            List<Image> nodes = MyCanvas.Children.OfType<Image>().ToList();
            foreach (Image i in nodes)
            {
                MyCanvas.Children.Remove(i);
            }

            // draw rails
            foreach (Rail rail in track.Rails)
            {
                // Add a Line Element
                Line myLine = new Line();
                myLine.Stroke = System.Windows.Media.Brushes.DarkBlue;
                myLine.X1 = rail.StartX;
                myLine.X2 = rail.EndX;
                myLine.Y1 = rail.StartY;
                myLine.Y2 = rail.EndY;
                myLine.HorizontalAlignment = HorizontalAlignment.Left;
                myLine.VerticalAlignment = VerticalAlignment.Center;
                myLine.StrokeThickness = 2; // 4;
                myLine.Focusable = true;
                MyCanvas.Children.Add(myLine);
            }

            // draw nodes
            foreach (Node node in track.Nodes)
            {
                node.Rechtdoor = true;
                Image myImage = new Image();
                string fileName = node.NodeTypeName;
                if (fileName.Contains("wissel"))
                {
                    fileName = fileName + "_1";
                }
                ImageSource imageSource = new BitmapImage(new Uri(@"pack://application:,,,/BigScreen/Images/" + fileName + ".png", UriKind.Absolute));
                myImage.Source = imageSource;
                myImage.Name = node.NodeTypeName + "_" + _iterator.ToString();
                _iterator++;
                myImage.Width = _gridSize;
                myImage.Height = _gridSize;
                Canvas.SetTop(myImage, node.Y);
                Canvas.SetLeft(myImage, node.X);
                myImage.Tag = node.Rotation;
                RotateTransform rt = new RotateTransform((int)myImage.Tag, ((double)_gridSize / 2.0), ((double)_gridSize / 2.0));
                myImage.RenderTransform = rt;
                myImage.Stretch = Stretch.Fill;
                myImage.Focusable = true;
                myImage.MouseLeftButtonDown += myImage_MouseLeftButtonDown;
                MyCanvas.Children.Add(myImage);
                _nodeInfo.Add(myImage.Name, node);
            }

        }

        private void AlterCoordinatesAndCanvasSize(Track track)
        {
            int minX = Math.Min(track.Nodes.Min(s => s.X), Math.Min(track.Rails.Min(s => s.StartX), track.Rails.Min(s => s.EndX)));
            int minY = Math.Min(track.Nodes.Min(s => s.Y), Math.Min(track.Rails.Min(s => s.StartY), track.Rails.Min(s => s.EndY)));
            int maxX = Math.Max(track.Nodes.Max(s => s.X), Math.Max(track.Rails.Max(s => s.StartX), track.Rails.Max(s => s.EndX)));
            int maxY = Math.Max(track.Nodes.Max(s => s.Y), Math.Max(track.Rails.Max(s => s.StartY), track.Rails.Max(s => s.EndY)));

            foreach (Rail rail in track.Rails)
            {
                rail.StartX -= minX;
                rail.EndX -= minX;
                rail.StartY -= minY;
                rail.EndY -= minY;
            }

            foreach (Node node in track.Nodes)
            {
                node.X -= minX;
                node.Y -= minY;
            }

            MyCanvas.Width = (maxX - minX); // *2;
            MyCanvas.Height = (maxY - minY); // *2;
        }

        void myImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // get the name of the touched node
            string name = ((Image)sender).Name;

            if (name.StartsWith("kruispunt"))
            {
                // doe niks
            }
            else
            {
                if (name.StartsWith("ontkoppelrail"))
                {
                    SendData(_nodeInfo[name].NodeAddress1, _nodeInfo[name].NodeAddress2);
                }
                else
                {
                    if (name.StartsWith("driewegwissel"))
                    {
                        switch (_nodeInfo[name].Stand)
                        {
                            case 0:
                                {
                                    // ga naar rechtdoor
                                    SendData(33, _nodeInfo[name].NodeAddress2);
                                    SendData(33, _nodeInfo[name].NodeAddress1);
                                    _nodeInfo[name].Stand++;

                                    ChangeImage((Image)sender, name, 2);
                                    break;
                                }
                            case 1:
                                {
                                    // ga naar links
                                    SendData(33, _nodeInfo[name].NodeAddress2);
                                    SendData(34, _nodeInfo[name].NodeAddress1);
                                    _nodeInfo[name].Stand++;

                                    ChangeImage((Image)sender, name, 3);
                                    break;
                                }
                            case 2:
                                {
                                    // ga naar rechts
                                    SendData(34, _nodeInfo[name].NodeAddress2);
                                    SendData(33, _nodeInfo[name].NodeAddress1);
                                    _nodeInfo[name].Stand = 0;

                                    ChangeImage((Image)sender, name, 1);
                                    break;
                                }
                        }
                    }
                    else
                    {
                        if (name.StartsWith("dubbelekruiswissel"))
                        {
                            switch (_nodeInfo[name].Stand)
                            {
                                case 0:
                                    {
                                        // ga naar plat rechtdoor
                                        SendData(34, _nodeInfo[name].NodeAddress2);
                                        SendData(34, _nodeInfo[name].NodeAddress1);
                                        _nodeInfo[name].Stand++;

                                        ChangeImage((Image)sender, name, 2);
                                        break;
                                    }
                                case 1:
                                    {
                                        // ga naar rechts
                                        SendData(33, _nodeInfo[name].NodeAddress2);
                                        SendData(34, _nodeInfo[name].NodeAddress1);
                                        _nodeInfo[name].Stand++;

                                        ChangeImage((Image)sender, name, 4);
                                        break;
                                    }
                                case 2:
                                    {
                                        // ga naar links
                                        SendData(34, _nodeInfo[name].NodeAddress2);
                                        SendData(33, _nodeInfo[name].NodeAddress1);
                                        _nodeInfo[name].Stand++;

                                        ChangeImage((Image)sender, name, 3);
                                        break;
                                    }
                                case 3:
                                    {
                                        // ga naar schuin rechtdoor
                                        SendData(33, _nodeInfo[name].NodeAddress2);
                                        SendData(33, _nodeInfo[name].NodeAddress1);
                                        _nodeInfo[name].Stand = 0;

                                        ChangeImage((Image)sender, name, 1);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            // alle gewone wissels
                            int commando;

                            if (_nodeInfo[name].Rechtdoor)
                            {
                                commando = 34;
                                _nodeInfo[name].Rechtdoor = false;
                            }
                            else
                            {
                                commando = 33;
                                _nodeInfo[name].Rechtdoor = true;
                            }

                            SendData(commando, _nodeInfo[name].NodeAddress1);
                            if (_nodeInfo[name].Rechtdoor)
                            {
                                ChangeImage((Image)sender, name, 1);
                            }
                            else
                            {
                                ChangeImage((Image)sender, name, 2);
                            }
                        }
                    }
                }
            }	
        }

        private void SendData(int s1, int s2)
        {
            if (s1 == 33 || s1 == 34)
            {
                s2 = s2 - 1;
            }
            _agent.Send(s1.ToString(), s2.ToString());
        }

        private void ChangeImage(Image image, string name, int nr)
        {
            string[] baseName = name.Split('_');
            ImageSource imageSource = new BitmapImage(new Uri(@"pack://application:,,,/BigScreen/Images/" + baseName[0] + "_" + nr.ToString() + ".png", UriKind.Absolute));
            image.Source = imageSource;
        }

    }
    
}
