using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FSUIPC;
using System.Windows.Threading;

namespace FSTimeSync
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timerMain = new DispatcherTimer();

        private Offset<uint> airspeed = new Offset<uint>(0x02BC);
        private Offset<uint> avionicsMaster = new Offset<uint>(0x2E80);

        public MainWindow()
        {
            InitializeComponent();
            configureForm();
            timerMain.Interval = TimeSpan.FromMilliseconds(50);
            timerMain.Tick += timerMain_Tick;
            DispatcherTimer LiveTime = new DispatcherTimer();
            LiveTime.Interval = TimeSpan.FromSeconds(1);
            LiveTime.Tick += LiveTime_Tick;
            LiveTime.Start();
        }
        void btnToggleConnection_Click(object sender, RoutedEventArgs e)
        {
            if (FSUIPCConnection.IsOpen)
            {
                this.timerMain.Stop();
                FSUIPCConnection.Close();
            }
            else
            {
                try
                {
                    this.lblConnectionStatus.Text = "Looking for Simulator";
                    this.lblConnectionStatus.Foreground = Brushes.Goldenrod;
                    FSUIPCConnection.Open();
                    this.timerMain.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection Failed\n\n" + ex.Message, "FSUIPC", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            configureForm();
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            try
            {
                FSUIPCConnection.Process();

                //double airspeedKnots = (double)this.airspeed.Value / 128d;
                //this.txtAirspeed.Text = airspeedKnots.ToString("F0");
            }
            catch (Exception ex)
            {
                this.timerMain.Stop();
                MessageBox.Show("Communication with FSUIPC Failed\n\n" + ex.Message, "FSUIPC", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                configureForm();
            }
        }

        private void configureForm()
        {
            if (FSUIPCConnection.IsOpen)
            {
                this.btnToggleConnection.Content = "Disconnect";
                this.lblConnectionStatus.Text = "Connected to " + FSUIPCConnection.FlightSimVersionConnected.ToString();
                this.lblConnectionStatus.Foreground = Brushes.Green;
            }
            else
            {
                this.btnToggleConnection.Content = "Connect";
                this.lblConnectionStatus.Text = "Disconnected";
                this.lblConnectionStatus.Foreground = Brushes.Red;
            }
        }

        void LiveTime_Tick(object sender, EventArgs e)
        {
            LiveTimeLabel.Content = DateTime.Now.ToString("HH:mm:ss");
        }

        void Window_Closing()
        {
            this.timerMain.Stop();
            FSUIPCConnection.Close();
        }
    }
}
