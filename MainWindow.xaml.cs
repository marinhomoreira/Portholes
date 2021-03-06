﻿using System;
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

using System.Text.RegularExpressions;

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
        Subscription status;

        TouchlessMgr tm;

        String uname;


        Dictionary<string, CamWindow> dic;
        List<CamWindow> availableCams;

        public MainWindow()
        {
            InitializeComponent();

            this.sd = new SharedDictionary();
            this.sd.Url = "tcp://192.168.0.123:test";
            this.sd.Open();

            this.svideo = new Subscription();
            this.svideo.Pattern = "/*/video";
            this.svideo.Dictionary = this.sd;
            this.svideo.Notified += new SubscriptionEventHandler(svideo_Notified);

            this.status = new Subscription();
            this.status.Pattern = "/*/status";
            this.status.Dictionary = this.sd;
            this.status.Notified += new SubscriptionEventHandler(status_Notified);

            this.dic = new Dictionary<string, CamWindow>();
            this.availableCams = new List<CamWindow>();
            availableCams.Add(this.cam1);
            availableCams.Add(this.cam2);
            availableCams.Add(this.cam3);
        }

        void svideo_Notified(object sender, SubscriptionEventArgs e)
        {
            string[] values = Regex.Split(e.Path, @"/");
            string username = values[1];

            if (!dic.ContainsKey(username))
            {
                dic.Add(username, availableCams.ElementAt(0));
                availableCams.RemoveAt(0);
            }

            String key = "/" + this.uname + "/video";
            if (key != e.Path && e.Reason.ToString() == "Change")
            {
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    var memoryStream = new MemoryStream((byte[])e.Value);
                    var image = Bitmap.FromStream(memoryStream);
                    ImageSource iSource = Imaging.CreateBitmapSourceFromHBitmap(((Bitmap)image).GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    this.dic[username].WebCamImage = iSource;
                    this.dic[username].Username = username;
                }));
            }

            if (e.Reason.ToString() == "Remove")
            {
                
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    gridao.Children.Remove(this.dic[username]);

                }));
                this.dic.Remove(username);
            }
        }

        void status_Notified(object sender, SubscriptionEventArgs e)
        {
            string[] values = Regex.Split(e.Path, @"/");
            string username = values[1];
            String key = "/" + this.uname + "/status";
            if (key == e.Path)
            {
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    Console.WriteLine("Key: " + e.Path + " = " + e.Value + "; because: " + e.Reason + "\n");
                    this.selfStatus.Text = this.uname + ": " + e.Value;
                }));
            }
            else
            {
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    Console.WriteLine("Key: " + e.Path + " = " + e.Value + "; because: " + e.Reason + "\n");
                    this.dic[username].Status = "Status: " + (String)e.Value;
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
            String key = "/" + this.uname + "/status";
            this.sd[key] = this.statusBox.Text;
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
            String key = "/" + this.uname + "/*";
            this.sd.Remove(key);
        }

    }
}
