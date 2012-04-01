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
using System.Timers;

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
        private static bool IsPlayed;

        private static int NoOfKeys = 26;
        private HashSet<int> UnusedKeys;
        private Rectangle[] Buttons = new Rectangle[26];
        private Ellipse E = new Ellipse();

        private Timer AppTimer;

        private static int Tempo = 120;
        private static double TimerIncrement;

        private static double ThirtySecondth = 62.5;
        private static double Sixteenth = 125;
        private static double Eighth = 250;
        private static double Quarter = 500;
        private static double Half = 1000;
        private static double Whole = 2000;

        private static PatternUnit CurrentPatternUnit;
        private static int CurrentNote;
        private static int CurrentDuration;
        private static int CurrentNoteIndex;

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

            for (int i = 0; i < 26; i++)
            {
                Buttons[i] = new Rectangle();
                InitializeRectangle(i);
            }

            InitializeEllipse();

            TransformToThirteenKeys();

            StartTest();
        }

        private void InitializeEllipse()
        {
            E.Name = "E";
            E.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            E.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            E.Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xA0, 0xFF, 0xFA, 0xFC));
            Canvas.SetLeft(E, 0.0);
            Canvas.SetTop(E, 0.0);
            E.Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xA0, 0xFF, 0xFA, 0xFC));
            E.TouchDown += E_TouchDown;
        }

        private void InitializeRectangle(int i)
        {
            Buttons[i].Name = "B" + i;
            Buttons[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Buttons[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Buttons[i].Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0x14, 0x27, 0x76));
            Canvas.SetLeft(Buttons[i], 0.0);
            Canvas.SetTop(Buttons[i], 0.0);
            Buttons[i].Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0x14, 0x27, 0x76));
            Buttons[i].TouchDown += B_TouchDown;
        }

        //Changes layout to 26 keys 0 - 25
        private void TransformToThirteenKeys()
        {
            UnusedKeys = new HashSet<int>();

            C.Children.Clear();

            int LeftAmount = 65;
            int TopAmount = 500;
            int LeftIncrement = 140;
            int RectWidth = 110;
            int RectHeight = 360;

            for (int i = 0; i < NoOfKeys; i++)
            {
                if (i == 13)
                {
                    LeftAmount = 65;
                    TopAmount = 60;
                }

                Buttons[i].Width = RectWidth;
                Buttons[i].Height = RectHeight;
                Buttons[i].Margin = new Thickness(LeftAmount, TopAmount, 0, 0);
                Buttons[i].Visibility = System.Windows.Visibility.Visible;

                C.Children.Add(Buttons[i]);

                LeftAmount += LeftIncrement;
            }

            C.Children.Add(E);
        }

        //Changes layout to 8 keys 0 - 7 & 13 - 20
        private void TransformToEightKeys()
        {
            UnusedKeys = new HashSet<int>();

            C.Children.Clear();

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
                    UnusedKeys.Add(i);
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

                C.Children.Add(Buttons[i]);

                LeftAmount += LeftIncrement;
            }

            C.Children.Add(E);
        }

        //Changes layout to 6 keys 0 - 5 & 13 - 18
        private void TransformToSixKeys()
        {
            UnusedKeys = new HashSet<int>();

            C.Children.Clear();

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
                    UnusedKeys.Add(i);
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

                C.Children.Add(Buttons[i]);

                LeftAmount += LeftIncrement;
            }

            C.Children.Add(E);
        }

        //Add an ellipse to Rectangle R
        private void ShowVisualCue(Rectangle R)
        {
            //NECESSARY CRAP IF YOU WANT TO MODIFY ANY CONTROL THAT'S OWNED BY THE MAIN THREAD
            R.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        int idx = Int32.Parse(R.Name.Substring(1));
                        CuedButtonNo = idx;
                        EIdx = idx;
                    }
                ));
            
            //NECESSARY CRAP IF YOU WANT TO MODIFY ANY CONTROL THAT'S OWNED BY THE MAIN THREAD
            E.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        E.Height = R.Height;
                        E.Width = R.Width;
                        E.Visibility = System.Windows.Visibility.Visible;
                        E.Margin = new Thickness(R.Margin.Left, R.Margin.Top, 0, 0);
                        E.Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xA0, 0xFF, 0xFA, 0xFC));
                    }
                ));
        }

        //Hide Ellipse from screen
        private void HideVisualCue()
        {
            EIdx = -1;
            IsPlayed = false;

            //NECESSARY CRAP IF YOU WANT TO MODIFY ANY CONTROL THAT'S OWNED BY THE MAIN THREAD
            E.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        E.Margin = new Thickness(10000, 10000, 0, 0);
                        E.Visibility = System.Windows.Visibility.Collapsed;
                    }
                ));
        }

        //Send an OSC Message
        //Either a playNote mssg if note is cued or an errorNote message if note is played erroneously
        private bool sendNoteOsc(int idx)
        {
            bool isCued = false;
            OscMessage m;

            if (CuedButtonNo != EIdx)
            {
                return false;
            }

            if (CuedButtonNo == idx)
            {
                m = new OscMessage(src, "/soundBored/playNote");
                isCued = true;
                Console.WriteLine("playNote " + idx);
                IsPlayed = true;

                //May Have Solved Multihit MultiNote problem with this as well as original problem which was error notes on hitting ellipse more than once per note duration...
                CuedButtonNo = -1;

                //NECESSARY CRAP IF YOU WANT TO MODIFY ANY CONTROL THAT'S OWNED BY THE MAIN THREAD
                E.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                        delegate()
                        {
                            E.Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xA0, 0x1E, 0x90, 0xFF));
                        }
                    ));
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

            //if (handled)
            //{
            //    HideVisualCue();
            //    ShowVisualCue(getRandomButton());
            //}
        }

        private void E_TouchDown(object sender, TouchEventArgs e)
        {
            //TODO: Add colour change or Ellipse pressed effect...

            bool handled = sendNoteOsc(EIdx);

            FrameworkElement fe = e.Source as FrameworkElement;
            Console.WriteLine("__\n [TouchDown] " + fe.Name + " " + EIdx);

            //if (handled)
            //{
            //    HideVisualCue();
            //    ShowVisualCue(getRandomButton());
            //}
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

        private ArrayList GenerateRandomPattern(int Difficulty, int Length)
        {
            //TODO Start at a random MIDI Note Number and do random walk.
            //Therefore Note would have to be translated in terms of number of keys by doing a Mod operation on the number of keys
            //Also Octave would have to slide as a sliding window according to when notes reach the edge of the octave when next note goes into next octave

            ArrayList NewPattern = new ArrayList();
            PatternUnit TempPatternUnit;
            Random RandNote = new Random();
            Random RandDuration = new Random();

            int TempNote = -1, MinNote, MaxNote;

            for (int index = 0; index < Length; index++)
            {
                do
                {
                    if (NewPattern.Count == 0)
                    {
                        TempNote = RandNote.Next(26);
                    }
                    MinNote = TempNote;
                    MaxNote = TempNote;

                    if (MinNote - Difficulty < -1)
                    {
                        MinNote = -1;
                    }
                    else
                    {
                        MinNote -= Difficulty;
                    }

                    if (MaxNote + Difficulty > 26)
                    {
                        MaxNote = 26;
                    }
                    else
                    {
                        MaxNote += Difficulty;
                    }

                    TempNote = RandNote.Next(MinNote, MaxNote);
                }
                //while (UnusedKeys.Contains(TempNote));
                while (UnusedKeys.Contains(TempNote) || ((TempNote > 9 && TempNote < 13) || (TempNote > 22 && TempNote < 26))) ; //COMMENT OUT ON REAL USE ON SURFACE

                TempPatternUnit = new PatternUnit(TempNote, (int) Math.Pow(2.0, RandDuration.Next(1, 5)));
                NewPattern.Add(TempPatternUnit);

                Console.Write("N:{0}, D:{1} : ", TempPatternUnit.Note, TempPatternUnit.Duration);
            }
            Console.WriteLine();
            
            return NewPattern;
        }

        private void StartTest()
        {
            //TODO What happens when you click Start Test
            TimerIncrement = ThirtySecondth;
            AppTimer = new Timer(TimerIncrement);

            Pattern = GenerateRandomPattern(4, 50);
            CurrentDuration = 0;
            CurrentNoteIndex = -1;

            AppTimer.Elapsed += HandleTimerElapsedEvent;
            AppTimer.Start();
            AppTimer.Interval = TimerIncrement;
        }

        private void StopTest()
        {
            //TODO What happens when you click Stop Test


        }

        private void LoadPattern()
        {
            //TODO What happens when you click Load New Pattern


        }

        private void Demo()
        {
            //TODO What happens when you click Demo


        }

        private void FreePlay()
        {
            //TODO What happens when you click Free Play


        }

        private void ShowTestResults()
        { 
            //TODO Display Results of the Test

        }

        private void HandleTimerElapsedEvent(Object Source, ElapsedEventArgs e)
        { 
            //TODO What happens when AppTimer's Elapsed Event Fire

            if (CurrentDuration == 0)
            {
                if (CurrentNoteIndex == Pattern.Count - 1)
                {
                    AppTimer.Enabled = false;
                    HideVisualCue();
                    ShowTestResults();

                    Console.WriteLine("Test Done : " + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);
                    return;
                }

                CurrentNoteIndex++;
                CurrentPatternUnit = new PatternUnit((PatternUnit)Pattern[CurrentNoteIndex]);
                CurrentNote = CurrentPatternUnit.Note;
                CurrentDuration = CurrentPatternUnit.Duration;
                IsPlayed = false;

                Console.WriteLine("CDur == 0 : CN++, Cnot, Cdur, IP = F, HiVisCu : " + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);

                HideVisualCue();
                
                if (CurrentNote >= 0) //Rests Should Not Trigger Visual Cue
                {
                    Console.WriteLine("CNot >= 0 : ShVisCu : " + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);
                    ShowVisualCue(Buttons[CurrentNote]);
                }
            }
            else if (CurrentNoteIndex < 0)
            {
                Console.WriteLine("CNI < 0 : CDur = 0 : " + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);
                CurrentDuration = 0;
            }
            else if (IsPlayed && EIdx >= 0)
            {
                Console.WriteLine("IP == T & EIdx >=0 : CDur-- : " + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);
                CurrentDuration--;
            }
            else if (IsPlayed)
            {
                Console.WriteLine("IP : CDur--" + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);
                CurrentDuration--;
            }
            else if (!IsPlayed && CurrentNote == -1)
            {
                Console.WriteLine("!IP & CNot ==-1 : CDur--" + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);
                CurrentDuration--;
            }
            else
            {
                Console.WriteLine("Else... ??? : " + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);
            }
        }
    }

    class PatternUnit
    {
        private int note;
        private int duration;

        public int Note
        {
            get
            {
                return note;
            }

            set
            {
                note = value;
            }
        }

        public int Duration
        {
            get
            {
                return duration;
            }

            set
            {
                duration = value;
            }
        }

        public PatternUnit()
        { 
            
        }

        public PatternUnit(int Note, int Duration)
        {
            this.Note = Note;
            this.Duration = Duration;
        }

        public PatternUnit(PatternUnit PattrnUnit)
        {
            this.Note = PattrnUnit.Note;
            this.Duration = PattrnUnit.Duration;
        }
    }
}