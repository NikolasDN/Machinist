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
using System.IO;
using System.Drawing.Imaging;
using System.Data;
using System.Data.Entity;
using System.Windows.Media.Effects;


namespace MachinistServer.VisualTrack
{
    /// <summary>
    /// Interaction logic for TrackWindow.xaml
    /// </summary>
    public partial class TrackWindow : Window
    {
        //private Track _track;
        private Point? _startPoint;
        private int _gridSize = 10;
        private string _mode;
        private BitmapImage _bi;
        private string _bn;
        private int _iterator;
        private const int _imageModifier = 0;
        private Image selectedImage = null;
        private Image editableImage = null;
        private Dictionary<string, Node> nodeInfo = new Dictionary<string, Node>();
        private Line selectedTrack = null;
        private Line orientationTrack = null;
                
        public TrackWindow()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += new MouseButtonEventHandler(TrackWindow_MouseLeftButtonDown);
            this.MouseMove += new MouseEventHandler(TrackWindow_MouseMove);
            MyGrid.SelectionChanged += new SelectionChangedEventHandler(MyGrid_SelectionChanged);
        }

        void TrackWindow_MouseMove(object sender, MouseEventArgs e)
        {
            switch (_mode)
            {
                case "Rails":
                    {
                        if (_startPoint.HasValue)
                        {
                            Point scrPt = Mouse.GetPosition(MyCanvas);
                            int x = ((int)Math.Round(scrPt.X / _gridSize) * _gridSize);
                            int y = ((int)Math.Round(scrPt.Y / _gridSize) * _gridSize);
                            scrPt.X = x;
                            scrPt.Y = y;

                            if (!MyCanvas.Children.Contains(orientationTrack))
                            {
                                // Add a Line Element
                                orientationTrack = new Line();
                                orientationTrack.Stroke = System.Windows.Media.Brushes.Cyan;
                                orientationTrack.X1 = _startPoint.Value.X;
                                orientationTrack.Y1 = _startPoint.Value.Y;
                                orientationTrack.HorizontalAlignment = HorizontalAlignment.Left;
                                orientationTrack.VerticalAlignment = VerticalAlignment.Center;
                                orientationTrack.StrokeThickness = 1; // 4;
                                int index = 0;
                                foreach (UIElement uiElement in MyCanvas.Children)
                                {
                                    if (uiElement is Image)
                                    {
                                        break;
                                    }
                                    index++;
                                }
                                MyCanvas.Children.Insert(index, orientationTrack);    
                            }

                            double angle = Math.Atan2(scrPt.Y - _startPoint.Value.Y, scrPt.X - _startPoint.Value.X) * 180 / Math.PI;
                            if (angle % 45 == 0)
                            {
                                orientationTrack.X2 = scrPt.X;
                                orientationTrack.Y2 = scrPt.Y;
                            }
                        }
                        break;
                    }
            }
        }

       
        void MyGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            NodeTypePresenter ntp = dataGrid.SelectedItem as NodeTypePresenter;
            _mode = ntp.Name;
            if (ntp.Bitmap1 != null)
            {
                _bi = ntp.Bitmap1;
                _bn = ntp.Name;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddGrid();

            MyGrid.ItemsSource = GetNodeTypes();
            
        }

        private List<NodeTypePresenter> GetNodeTypes()
        {
            List<NodeTypePresenter> result = new List<NodeTypePresenter>();
            
            result.Add(new NodeTypePresenter() { Name = "Rails" });
            result.Add(new NodeTypePresenter() { Name = "Linkerwissel", Bitmap1 = ToBitmapImage(Properties.Resources.linkerwissel) });
            result.Add(new NodeTypePresenter() { Name = "Rechterwissel", Bitmap1 = ToBitmapImage(Properties.Resources.rechterwissel) });
            result.Add(new NodeTypePresenter() { Name = "Driewegwissel", Bitmap1 = ToBitmapImage(Properties.Resources.driewegwissel) });
            result.Add(new NodeTypePresenter() { Name = "Enkele kruiswissel", Bitmap1 = ToBitmapImage(Properties.Resources.enkelekruiswissel) });
            result.Add(new NodeTypePresenter() { Name = "Dubbele kruiswissel", Bitmap1 = ToBitmapImage(Properties.Resources.dubbelekruiswissel) });
            result.Add(new NodeTypePresenter() { Name = "Kruispunt", Bitmap1 = ToBitmapImage(Properties.Resources.kruispunt) });
            result.Add(new NodeTypePresenter() { Name = "Ontkoppelrail", Bitmap1 = ToBitmapImage(Properties.Resources.ontkoppelrail) });

            return result;
        }

        private BitmapImage ToBitmapImage(System.Drawing.Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        void AddGrid()
        {
            for (int x = _gridSize; x < this.Width; x += _gridSize)
            {
                for (int y = _gridSize; y < this.Height; y += _gridSize)
                {
                    Ellipse el = new Ellipse();
                    el.Fill = Brushes.DarkGray;
                    el.Height = 3;
                    el.Width = 3;
                    Canvas.SetTop(el, y - 1);
                    Canvas.SetLeft(el, x - 1);
                    //if (!MyCanvas.Children.Contains(el))
                    //{
                        MyCanvas.Children.Add(el);
                    //}
                }
            }
        }

        void TrackWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point scrPt = Mouse.GetPosition(MyCanvas);
            int x = ((int)Math.Round(scrPt.X / _gridSize) * _gridSize);
            int y = ((int)Math.Round(scrPt.Y / _gridSize) * _gridSize);
            scrPt.X = x;
            scrPt.Y = y;


            switch (_mode)
            {
                case "Rails":
                    {
                        if (_startPoint.HasValue)
                        {
                            if (MyCanvas.Children.Contains(orientationTrack))
                            {
                                MyCanvas.Children.Remove(orientationTrack);
                            }

                            // Add a Line Element
                            Line myLine = new Line();
                            myLine.Stroke = System.Windows.Media.Brushes.DarkBlue;
                            myLine.X1 = _startPoint.Value.X;
                            myLine.X2 = scrPt.X;
                            myLine.Y1 = _startPoint.Value.Y;
                            myLine.Y2 = scrPt.Y;
                            myLine.HorizontalAlignment = HorizontalAlignment.Left;
                            myLine.VerticalAlignment = VerticalAlignment.Center;
                            myLine.StrokeThickness = 2; // 4;
                            myLine.Focusable = true;
                            myLine.MouseEnter += new MouseEventHandler(myLine_MouseEnter);
                            myLine.MouseLeave += new MouseEventHandler(myLine_MouseLeave);
                            myLine.KeyDown += new KeyEventHandler(myLine_KeyDown);
                            int index = 0;
                            foreach (UIElement uiElement in MyCanvas.Children)
                            {
                                if (uiElement is Image)
                                {
                                    break;
                                }
                                index++;
                            }
                            MyCanvas.Children.Insert(index, myLine);

                            _startPoint = null;
                        }
                        else
                        {
                            _startPoint = scrPt;
                        }
                        break;
                    }
                default:
                    {
                        // select image
                        Image myImage = null;
                        foreach (UIElement uiElement in MyCanvas.Children)
                        {
                            if (uiElement is Image && Canvas.GetTop(uiElement) == scrPt.Y - _imageModifier - (_gridSize / 2) && Canvas.GetLeft(uiElement) == scrPt.X - _imageModifier - (_gridSize / 2))
                            {
                                myImage = (Image)uiElement;
                            }
                        }

                        // rotate image
                        //if (myImage != null)
                        //{
                        //    myImage.Tag = ((int)myImage.Tag) + 45;
                        //    RotateTransform rt = new RotateTransform((int)myImage.Tag, ((double)_gridSize / 2.0), ((double)_gridSize / 2.0));
                        //    myImage.RenderTransform = rt;
                        //}

                        // add image
                        if (myImage == null)
                        {
                            if (_bi != null)
                            {
                                myImage = new Image();
                                myImage.Source = _bi;
                                myImage.Name = _bn.ToLower().Replace(" ", "") + "_" + _iterator.ToString();
                                _iterator++;
                                myImage.Width = _gridSize;
                                myImage.Height = _gridSize;
                                Canvas.SetTop(myImage, scrPt.Y - _imageModifier - (_gridSize / 2));
                                Canvas.SetLeft(myImage, scrPt.X - _imageModifier - (_gridSize / 2));
                                myImage.Tag = 0;
                                myImage.Stretch = Stretch.Fill;
                                myImage.Focusable = true;
                                myImage.MouseEnter += new MouseEventHandler(myImage_MouseEnter);
                                myImage.MouseLeave += new MouseEventHandler(myImage_MouseLeave);
                                myImage.MouseLeftButtonDown += new MouseButtonEventHandler(myImage_MouseLeftButtonDown);
                                myImage.MouseRightButtonDown += new MouseButtonEventHandler(myImage_MouseRightButtonDown);
                                myImage.KeyDown += new KeyEventHandler(myImage_KeyDown);
                                MyCanvas.Children.Add(myImage);
                                nodeInfo.Add(myImage.Name, new Node() { NodeAddress1 = 0, NodeAddress2 = 0, NodeNr1 = 0, NodeNr2 = 0 });
                            }
                        }
                        
                        break;
                    }
            }
        }

        void myImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_mode != "Rails")
            {
                Image myImage = (Image)sender;
                myImage.Tag = ((int)myImage.Tag) + 45;
                RotateTransform rt = new RotateTransform((int)myImage.Tag, ((double)_gridSize / 2.0), ((double)_gridSize / 2.0));
                myImage.RenderTransform = rt;
            }
        }

        void myImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_mode != "Rails")
            {
                if (editableImage != null)
                {
                    editableImage.Effect = null;
                    //editableImage = null;

                    nodeInfo[editableImage.Name].NodeAddress1 = Convert.ToInt32(textBlockAddress1.Text);
                    nodeInfo[editableImage.Name].NodeAddress2 = Convert.ToInt32(textBlockAddress2.Text);
                    nodeInfo[editableImage.Name].NodeNr1 = Convert.ToInt32(textBlockNodeNr1.Text);
                    nodeInfo[editableImage.Name].NodeNr2 = Convert.ToInt32(textBlockNodeNr2.Text);
                }

                editableImage = (Image)sender;
                editableImage.Effect = new DropShadowEffect();

                labelNodeName.Content = editableImage.Name;
                textBlockAddress1.Text = nodeInfo[editableImage.Name].NodeAddress1.ToString();
                textBlockAddress2.Text = nodeInfo[editableImage.Name].NodeAddress2.ToString();
                textBlockNodeNr1.Text = nodeInfo[editableImage.Name].NodeNr1.ToString();
                textBlockNodeNr2.Text = nodeInfo[editableImage.Name].NodeNr2.ToString();
            }           
        }

        //void myImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
            
        //    if (selectedImage == (Image)sender)
        //    {
        //        // rotate image
        //        selectedImage.Tag = ((int)selectedImage.Tag) + 45;
        //        RotateTransform rt = new RotateTransform((int)selectedImage.Tag, ((double)_gridSize / 2.0), ((double)_gridSize / 2.0));
        //        selectedImage.RenderTransform = rt;
        //    }
        //    else
        //    {
        //        if (selectedImage != null)
        //        {
        //            selectedImage.Effect = null;
        //        }

        //        selectedImage = (Image)sender;
        //        selectedImage.Effect = new DropShadowEffect();
        //        selectedImage.Focus();
        //    }

        //}

        void myImage_KeyDown(object sender, KeyEventArgs e)
        {
            //if (selectedImage != null)
            //{
            //    selectedImage.Focusable = false;
            //    MyCanvas.Children.Remove(selectedImage);
            //}
            MyCanvas.Children.Remove((Image)sender);
        }

        void myImage_MouseLeave(object sender, MouseEventArgs e)
        {
            //selectedImage.Effect = null;
            selectedImage = null;
        }

        void myImage_MouseEnter(object sender, MouseEventArgs e)
        {
            selectedImage = (Image)sender;
            //selectedImage.Effect = new DropShadowEffect();
            selectedImage.Focus();
        }

        void myLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (selectedTrack != null)
            {
                selectedTrack.Focusable = false;
                MyCanvas.Children.Remove(selectedTrack);
            }
        }

        void myLine_MouseLeave(object sender, MouseEventArgs e)
        {
            selectedTrack = (Line)sender;
            selectedTrack.Stroke = Brushes.DarkBlue;
            selectedTrack = null;
        }

        void myLine_MouseEnter(object sender, MouseEventArgs e)
        {
            selectedTrack = (Line)sender;
            selectedTrack.Stroke = Brushes.Yellow;
            selectedTrack.Focus();
        }

        private void TrackToXaml(Track track)
        {
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
            _iterator = 0;
            nodeInfo.Clear();
            
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
                myLine.MouseEnter += new MouseEventHandler(myLine_MouseEnter);
                myLine.MouseLeave += new MouseEventHandler(myLine_MouseLeave);
                myLine.KeyDown += new KeyEventHandler(myLine_KeyDown);
                MyCanvas.Children.Add(myLine);
            }

            // draw nodes
            foreach (Node node in track.Nodes)
            {
                Image myImage = new Image();
                myImage.Source = ToBitmapImage((System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject(node.NodeTypeName));
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
                myImage.MouseEnter += new MouseEventHandler(myImage_MouseEnter);
                myImage.MouseLeave += new MouseEventHandler(myImage_MouseLeave);
                myImage.MouseLeftButtonDown += new MouseButtonEventHandler(myImage_MouseLeftButtonDown);
                myImage.MouseRightButtonDown += new MouseButtonEventHandler(myImage_MouseRightButtonDown);
                myImage.KeyDown += new KeyEventHandler(myImage_KeyDown);
                MyCanvas.Children.Add(myImage);
                nodeInfo.Add(myImage.Name, node);
            }
                        
        }

        private Track XamlToTrack()
        {
            Track result = new Track();
            result.Rails = new List<Rail>();
            result.Nodes = new List<Node>();
            //result.NodeTypes = new NodeTypeFactory().MakeNodeTypes();
            result.Trains = new List<Train>();

            foreach (Line rail in MyCanvas.Children.OfType<Line>())
            {
                result.Rails.Add(new Rail() { StartX = (int)rail.X1, StartY = (int)rail.Y1, EndX = (int)rail.X2, EndY = (int)rail.Y2 });
            }
            foreach (Image node in MyCanvas.Children.OfType<Image>())
            {
                result.Nodes.Add(new Node() { NodeTypeName = node.Name.Split('_')[0], X = (int)Canvas.GetLeft(node), Y = (int)Canvas.GetTop(node), Rotation = (int)node.Tag, NodeAddress1 = nodeInfo[node.Name].NodeAddress1, NodeAddress2 = nodeInfo[node.Name].NodeAddress2, NodeNr1 = nodeInfo[node.Name].NodeNr1, NodeNr2 = nodeInfo[node.Name].NodeNr2 });
            }

            return result;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            string fileName = GetFileNameToSave();

            if (!string.IsNullOrEmpty(fileName))
            {
                Track track = XamlToTrack();

                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(track.GetType());
                System.IO.FileStream fs = new FileStream(fileName, FileMode.Create);
                x.Serialize(fs, track);
                fs.Close();
            }
        }

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
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

        private string GetFileNameToSave()
        {
            // Create OpenFileDialog
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

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
    }
}
