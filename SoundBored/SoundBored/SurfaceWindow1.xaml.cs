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

        private static int srcPort = 5566;
        private static int dstPort = 6655;
        private static IPEndPoint src = new IPEndPoint(IPAddress.Loopback, srcPort);
        private static IPEndPoint dst = new IPEndPoint(IPAddress.Loopback, dstPort);
        private static int CuedButtonNo;
        private Rectangle[] Buttons = new Rectangle[16];

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

        //Changes layout to 6 keys 0 - 5
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

        //Changes layout to 8 keys 0 - 7
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

        //Add an ellipse to Rectangle R
        private void showVisualCue(Rectangle R)
        {
            E.Height = R.Height;
            E.Width = R.Width;

            int idx = Int32.Parse(R.Name.Substring(1));
            CuedButtonNo = idx;

            E.Visibility = System.Windows.Visibility.Visible;
            E.Margin = new Thickness(R.Margin.Left, R.Margin.Top, 0, 0);
        }

        //Hide Ellipse from screen
        private void hideVisualCue()
        {
            E.Margin = new Thickness(1000, 1000, 0, 0);
            E.Visibility = System.Windows.Visibility.Collapsed;
        }

        //Called as soon as Canvas C is loaded... Init screen...
        private void C_Loaded(object sender, RoutedEventArgs e)
        {
            Buttons[0] = B0;
            Buttons[1] = B1;
            Buttons[2] = B2;
            Buttons[3] = B3;
            Buttons[4] = B4;
            Buttons[5] = B5;
            Buttons[6] = B6;
            Buttons[7] = B7;
            Buttons[8] = B8;
            Buttons[9] = B9;
            Buttons[10] = B10;
            Buttons[11] = B11;
            Buttons[12] = B12;
            Buttons[13] = B13;
            Buttons[14] = B14;
            Buttons[15] = B15;


            TransformToEightKeys();
            showVisualCue(B3);
        }

        //Send an OSC Message
        //Either a playNote mssg if note is cued or an errorNote message if note is played erroneously
        private bool sendNoteOsc(int idx)
        {
            bool isCued = false;
            OscMessage m;

            if (CuedButtonNo == idx)
            {
                m = new OscMessage(src, "/soundBored/playNote");
                isCued = true;
                Console.WriteLine("playNote " + idx);
            }
            else
            {
                m = new OscMessage(src, "/soundBored/errorNote");

                Console.WriteLine("errorNote " + idx);
            }

            m.Append<int>(idx);
            
            try
            {
                m.Send(dst);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return isCued;
        }

        //Handles button presses of rectangles
        private bool handleButtonPress(FrameworkElement fe)
        {
            Int32 idx;
            bool handled = false;
            try
            {
                idx = Int32.Parse(fe.Name.Substring(1)); // TODO: Warning: This assumes button-4's name is "B4" ... very brittle
                handled = sendNoteOsc(idx);
            }
            catch (Exception e)
            {
                Console.WriteLine(fe.Name + e.Message + e.StackTrace);
            }

            return handled;
        }

        //Handles touch down on rectangles
        private void B_TouchDown(object sender, TouchEventArgs e)
        {
            //TODO: Add colour change or rectangle pressed effect...

            FrameworkElement fe = e.Source as FrameworkElement;
            Console.WriteLine("__\n [TouchDown] " + fe.Name);
            bool handled = handleButtonPress(fe);

            if (handled)
            {
                hideVisualCue();
                showVisualCue(getRandomButton());
            }
        }

        private void E_TouchDown(object sender, TouchEventArgs e)
        {
            //TODO: Add colour change or Ellipse pressed effect...

            bool handled = sendNoteOsc(CuedButtonNo);

            FrameworkElement fe = e.Source as FrameworkElement;
            Console.WriteLine("__\n [TouchDown] " + fe.Name);

            if (handled)
            {
                hideVisualCue();
                showVisualCue(getRandomButton());
            }
        }

        private Rectangle getRandomButton()
        {
            int Random = (new Random()).Next(16);

            //JUST A TEMPORARY TESTING LINE OF CODE... CAN'T SEE THESE FOUR KEYS ON MY SCREEN. ;)
            while (Random == 6 || Random == 7 || Random == 14 || Random == 15)//TESTING
            {//TESTING
                Random = (new Random()).Next(16);//TESTING
            }//TESTING

            return Buttons[Random];
        }
    }
}