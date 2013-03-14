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

namespace PortHoles
{
    /// <summary>
    /// Interaction logic for CamWindow.xaml
    /// </summary>
    public partial class CamWindow : UserControl
    {
        public CamWindow()
        {
            InitializeComponent();
        }
        
        public ImageSource WebCamImage
        {
            get { return this.receivedImg.Source; }
            set { this.receivedImg.Source = value;  }
        }

        public string Status
        {
            get { return this.status.Text; }
            set { this.status.Text = value; }
        }

        public string Username
        {
            get { return this.username.Text; }
            set { this.username.Text = value; }
        }

    }
}
