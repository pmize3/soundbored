using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Bespoke.Common.Osc;

namespace SoundBored
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {

        private static int    srcPort = 5566;
        private static int    dstPort = 6655;
        private static IPEndPoint src = new IPEndPoint(IPAddress.Loopback, srcPort);
        private static IPEndPoint dst = new IPEndPoint(IPAddress.Loopback, dstPort);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            OscMessage.LittleEndianByteOrder = false;
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;


        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        private void TransformToSixKeys()
        {
            B0.Margin = new Thickness(80, 500, 0, 0);
            B1.Margin = new Thickness(396, 500, 0, 0);
            B2.Margin = new Thickness(712, 500, 0, 0);
            B3.Margin = new Thickness(1028, 500, 0, 0);
            B4.Margin = new Thickness(1344, 500, 0, 0);
            B5.Margin = new Thickness(1660, 500, 0, 0);
            B6.Margin = new Thickness(10000, 500, 0, 0);
            B7.Margin = new Thickness(10000, 500, 0, 0);

            B0.Width = 180;
            B1.Width = 180;
            B2.Width = 180;
            B3.Width = 180;
            B4.Width = 180;
            B5.Width = 180;
            B6.Width = 180;
            B7.Width = 180;

            B0.Height = 360;
            B1.Height = 360;
            B2.Height = 360;
            B3.Height = 360;
            B4.Height = 360;
            B5.Height = 360;
            B6.Height = 360;
            B7.Height = 360;

            B0.Visibility = System.Windows.Visibility.Visible;
            B1.Visibility = System.Windows.Visibility.Visible;
            B2.Visibility = System.Windows.Visibility.Visible;
            B3.Visibility = System.Windows.Visibility.Visible;
            B4.Visibility = System.Windows.Visibility.Visible;
            B5.Visibility = System.Windows.Visibility.Collapsed;
            B6.Visibility = System.Windows.Visibility.Collapsed;
            B7.Visibility = System.Windows.Visibility.Collapsed;

            B8.Margin = new Thickness(80, 60, 0, 0);
            B9.Margin = new Thickness(396, 60, 0, 0);
            B10.Margin = new Thickness(712, 60, 0, 0);
            B11.Margin = new Thickness(1028, 60, 0, 0);
            B12.Margin = new Thickness(1344, 60, 0, 0);
            B13.Margin = new Thickness(1660, 60, 0, 0);
            B14.Margin = new Thickness(10000, 60, 0, 0);
            B15.Margin = new Thickness(10000, 60, 0, 0);

            B8.Visibility = System.Windows.Visibility.Visible;
            B9.Visibility = System.Windows.Visibility.Visible;
            B10.Visibility = System.Windows.Visibility.Visible;
            B11.Visibility = System.Windows.Visibility.Visible;
            B12.Visibility = System.Windows.Visibility.Visible;
            B13.Visibility = System.Windows.Visibility.Collapsed;
            B14.Visibility = System.Windows.Visibility.Collapsed;
            B15.Visibility = System.Windows.Visibility.Collapsed;

            B8.Width = 180;
            B9.Width = 180;
            B10.Width = 180;
            B11.Width = 180;
            B12.Width = 180;
            B13.Width = 180;
            B14.Width = 180;
            B15.Width = 180;

            B8.Height = 360;
            B9.Height = 360;
            B10.Height = 360;
            B11.Height = 360;
            B12.Height = 360;
            B13.Height = 360;
            B14.Height = 360;
            B15.Height = 360;
        }

        private void TransformToEightKeys()
        {
            B0.Margin = new Thickness(80, 500, 0, 0);
            B1.Margin = new Thickness(310, 500, 0, 0);
            B2.Margin = new Thickness(540, 500, 0, 0);
            B3.Margin = new Thickness(770, 500, 0, 0);
            B4.Margin = new Thickness(1000, 500, 0, 0);
            B5.Margin = new Thickness(1230, 500, 0, 0);
            B6.Margin = new Thickness(1460, 500, 0, 0);
            B7.Margin = new Thickness(1690, 500, 0, 0);

            B0.Width = 150;
            B1.Width = 150;
            B2.Width = 150;
            B3.Width = 150;
            B4.Width = 150;
            B5.Width = 150;
            B6.Width = 150;
            B7.Width = 150;

            B0.Height = 360;
            B1.Height = 360;
            B2.Height = 360;
            B3.Height = 360;
            B4.Height = 360;
            B5.Height = 360;
            B6.Height = 360;
            B7.Height = 360;

            B0.Visibility = System.Windows.Visibility.Visible;
            B1.Visibility = System.Windows.Visibility.Visible;
            B2.Visibility = System.Windows.Visibility.Visible;
            B3.Visibility = System.Windows.Visibility.Visible;
            B4.Visibility = System.Windows.Visibility.Visible;
            B5.Visibility = System.Windows.Visibility.Visible;
            B6.Visibility = System.Windows.Visibility.Visible;
            B7.Visibility = System.Windows.Visibility.Visible;

            B8.Margin = new Thickness(80, 60, 0, 0);
            B9.Margin = new Thickness(310, 60, 0, 0);
            B10.Margin = new Thickness(540, 60, 0, 0);
            B11.Margin = new Thickness(770, 60, 0, 0);
            B12.Margin = new Thickness(1000, 60, 0, 0);
            B13.Margin = new Thickness(1230, 60, 0, 0);
            B14.Margin = new Thickness(1460, 60, 0, 0);
            B15.Margin = new Thickness(1690, 60, 0, 0);
            
            B8.Visibility = System.Windows.Visibility.Visible;
            B9.Visibility = System.Windows.Visibility.Visible;
            B10.Visibility = System.Windows.Visibility.Visible;
            B11.Visibility = System.Windows.Visibility.Visible;
            B12.Visibility = System.Windows.Visibility.Visible;
            B13.Visibility = System.Windows.Visibility.Visible;
            B14.Visibility = System.Windows.Visibility.Visible;
            B15.Visibility = System.Windows.Visibility.Visible;

            B8.Width = 150;
            B9.Width = 150;
            B10.Width = 150;
            B11.Width = 150;
            B12.Width = 150;
            B13.Width = 150;
            B14.Width = 150;
            B15.Width = 150;

            B8.Height = 360;
            B9.Height = 360;
            B10.Height = 360;
            B11.Height = 360;
            B12.Height = 360;
            B13.Height = 360;
            B14.Height = 360;
            B15.Height = 360;
        }

        private void C_Loaded(object sender, RoutedEventArgs e)
        {
            TransformToEightKeys();
        }

        private void sendNoteOsc(int idx)
        {
            OscMessage m = new OscMessage(src, "/soundBored/playNote");
            m.Append<int>(idx);
            try
            {
                m.Send(dst);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private void handleButtonPress(FrameworkElement fe)
        {
            Int32 idx;
            try
            {
                idx = Int32.Parse(fe.Name.Substring(1)); // TODO: Warning: This assumes button-4's name is "B4" ... very brittle
                sendNoteOsc(idx);
            }
            catch (Exception e)
            {
                Console.WriteLine(fe.Name + e.Message + e.StackTrace);
            }

        }

       
        private void B_TouchDown(object sender, TouchEventArgs e)
        {
            Console.WriteLine("__\n [TouchDown]");
            FrameworkElement fe = e.Source as FrameworkElement;
            handleButtonPress(fe);
        }

    }
}