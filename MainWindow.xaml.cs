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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using GroupLab.Networking;

using System.Windows.Interop;
using System.Drawing.Imaging;
using System.Drawing;

using TouchlessLib;


namespace PortHoles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SharedDictionary sd;
        Subscription svideo;

        TouchlessMgr tm;

        String uname = "ae";

        public MainWindow()
        {
            InitializeComponent();

            this.sd = new SharedDictionary();
            this.sd.Url = "tcp://localhost:testando";
            this.sd.Open();

            this.svideo = new Subscription();
            this.svideo.Pattern = "/*/video";
            this.svideo.Dictionary = this.sd;
            this.svideo.Notified += new SubscriptionEventHandler(svideo_Notified);
        }

        void svideo_Notified(object sender, SubscriptionEventArgs e)
        {
            String key = "/" + this.uname + "/video";
            if (key == e.Path)
            {
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    Console.WriteLine("Key: " + e.Path + " = " + e.Value + "; because: " + e.Reason + "\n");
                    var memoryStream = new MemoryStream((byte[])e.Value);
                    var image = Bitmap.FromStream(memoryStream);
                    ImageSource iSource = Imaging.CreateBitmapSourceFromHBitmap(((Bitmap)image).GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    this.receivedImg.Source = iSource;
                }));
            }

        }

        void CurrentCamera_OnImageCaptured(object sender, CameraEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(delegate()
            {
                ImageSource iSource = Imaging.CreateBitmapSourceFromHBitmap(e.Image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                this.myImg.Source = iSource;

                var memoryStream = new MemoryStream();
                e.Image.Save(memoryStream, ImageFormat.Jpeg);
                var byteArray = memoryStream.ToArray();

                //this.receivedImg.Source = iSource;
                String key = "/" + this.uname + "/video";
                this.sd[key] = byteArray;
            }));
        }

        private void cameraOn_Click(object sender, RoutedEventArgs e)
        {
            this.tm = new TouchlessMgr();
            this.tm.CurrentCamera = this.tm.Cameras[0];
            this.tm.CurrentCamera.OnImageCaptured += new EventHandler<CameraEventArgs>(CurrentCamera_OnImageCaptured);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.uname = this.usernameBox.Text;
            Console.WriteLine(uname.ToString());
        }

        private void submitStatus_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            this.sd.Close();
            this.sd.Url = this.serverBox.Text;
            this.sd.Open();
            Console.WriteLine(this.sd.Status);
        }

        private void cameraOff_Click(object sender, RoutedEventArgs e)
        {
            this.tm.CurrentCamera.OnImageCaptured -= new EventHandler<CameraEventArgs>(CurrentCamera_OnImageCaptured);
        }

    }
}
