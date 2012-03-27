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
using System.Collections;

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
        private static int EIdx;
        private static int CuedButtonNo;
        private static int NoOfKeys = 26;
        private Rectangle[] Buttons = new Rectangle[26];

        //ArrayList Pattern has elements of type PatternUnit converted to Object, so don't forget data conversions when adding or removing elements from Pattern
        private ArrayList Pattern = new ArrayList();

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
            Buttons[16] = B16;
            Buttons[17] = B17;
            Buttons[18] = B18;
            Buttons[19] = B19;
            Buttons[20] = B20;
            Buttons[21] = B21;
            Buttons[22] = B22;
            Buttons[23] = B23;
            Buttons[24] = B24;
            Buttons[25] = B25;

            TransformToThirteenKeys();
            showVisualCue(getRandomButton());
        }

        //Changes layout to 6 keys 0 - 5
        private void TransformToThirteenKeys()
        {
            int LeftAmount = 65;
            int TopAmount = 500;
            int LeftIncrement = 140;
            for (int i = 0; i < NoOfKeys; i++)
            {
                if (i == 13)
                {
                    LeftAmount = 65;
                    TopAmount = 60;
                }

                Buttons[i].Margin = new Thickness(LeftAmount, TopAmount, 0, 0);
                Buttons[i].Width = 110;
                Buttons[i].Height = 360;
                Buttons[i].Visibility = System.Windows.Visibility.Visible;

                LeftAmount += LeftIncrement;
            }
        }

        //Changes layout to 8 keys 0 - 7
        private void TransformToEightKeys()
        {
            int LeftAmount = 80;
            int TopAmount = 500;
            int LeftIncrement = 230;
            for (int i = 0; i < NoOfKeys; i++)
            {
                if ((i > 7 && i < 13) || (i > 20 && i < 26))
                {
                    Buttons[i].Margin = new Thickness(10000, 10000, 0, 0);
                    Buttons[i].Width = 180;
                    Buttons[i].Height = 360;
                    Buttons[i].Visibility = System.Windows.Visibility.Collapsed;
                    continue;
                }
                else if (i == 13)
                {
                    LeftAmount = 80;
                    TopAmount = 60;
                }

                Buttons[i].Margin = new Thickness(LeftAmount, TopAmount, 0, 0);
                Buttons[i].Width = 150;
                Buttons[i].Height = 360;
                Buttons[i].Visibility = System.Windows.Visibility.Visible;

                LeftAmount += LeftIncrement;
            }
        }

        //Changes layout to 6 keys 0 - 5 & 13 - 18
        private void TransformToSixKeys()
        {
            int LeftAmount = 80;
            int TopAmount = 500;
            int LeftIncrement = 316;
            for (int i = 0; i < NoOfKeys; i++)
            {
                if ((i > 5 && i < 13) || (i > 18 && i < 26))
                {
                    Buttons[i].Margin = new Thickness(10000, 10000, 0, 0);
                    Buttons[i].Width = 180;
                    Buttons[i].Height = 360;
                    Buttons[i].Visibility = System.Windows.Visibility.Collapsed;
                    continue;
                }
                else if (i == 13)
                {
                    LeftAmount = 80;
                    TopAmount = 60;
                }

                Buttons[i].Margin = new Thickness(LeftAmount, TopAmount, 0, 0);
                Buttons[i].Width = 180;
                Buttons[i].Height = 360;
                Buttons[i].Visibility = System.Windows.Visibility.Visible;

                LeftAmount += LeftIncrement;
            }
        }

        //Add an ellipse to Rectangle R
        private void showVisualCue(Rectangle R)
        {
            E.Height = R.Height;
            E.Width = R.Width;

            int idx = Int32.Parse(R.Name.Substring(1));
            CuedButtonNo = idx;
            EIdx = idx;

            E.Visibility = System.Windows.Visibility.Visible;
            E.Margin = new Thickness(R.Margin.Left, R.Margin.Top, 0, 0);
        }

        //Hide Ellipse from screen
        private void hideVisualCue()
        {
            EIdx = -1;
            E.Margin = new Thickness(1000, 1000, 0, 0);
            E.Visibility = System.Windows.Visibility.Collapsed;
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

            bool handled = sendNoteOsc(EIdx);

            FrameworkElement fe = e.Source as FrameworkElement;
            Console.WriteLine("__\n [TouchDown] " + fe.Name + " " + EIdx);

            if (handled)
            {
                hideVisualCue();
                showVisualCue(getRandomButton());
            }
        }

        private Rectangle getRandomButton()
        {
            int Random = (new Random()).Next(26);

            ////JUST A TEMPORARY TESTING LINE OF CODE... CAN'T SEE THESE FOUR KEYS ON MY SCREEN. ;)
            //while (Random > 9 && Random < 13 || Random > 22 && Random < 26)//TESTING
            //{//TESTING
            //    Random = (new Random()).Next(26);//TESTING
            //}//TESTING

            return Buttons[Random];
        }
    }

    class PatternUnit
    {
        private int Note;
        private int Duration;

        public PatternUnit()
        { 
            
        }

        public PatternUnit(int Note, int Duration)
        {
            this.Note = Note;
            this.Duration = Duration;
        }

        public int GetNote()
        {
            return this.Note;
        }

        public void SetNote(int Note)
        {
            this.Note = Note;
        }

        public int GetDuration()
        {
            return this.Duration;
        }

        public void SetDuration(int Duration)
        {
            this.Duration = Duration;
        }
    }
}