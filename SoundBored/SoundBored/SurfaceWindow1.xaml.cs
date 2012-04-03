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
using System.IO;

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
        private DateTime LastPlayedTime;
        private bool MadeErrorOnLastNote;
        private int NoOfErrorsOnLastNote;
        private double LatenessThreshold;

        private static int NoOfKeys = 34;
        private HashSet<int> UnusedKeys;
        private Rectangle[] Buttons = new Rectangle[NoOfKeys];
        private Ellipse E = new Ellipse();

        private Label UserNameLabel = new Label();
        private Label PasswordLabel = new Label();
        private Label TitleLabel = new Label();
        private SurfaceButton LoginButton = new SurfaceButton();
        private SurfaceButton FreePlayButton = new SurfaceButton();
        private SurfaceTextBox username = new SurfaceTextBox();
        private SurfaceTextBox password = new SurfaceTextBox();
        
        private Timer AppTimer;

        private static int Tempo = 120;
        private static double TimerIncrement;

        private static double ThirtySecondth = 62.5;
        private static double Sixteenth = 125;
        private static double Eighth = 250;
        private static double Quarter = 500;
        private static double Half = 1000;
        private static double Whole = 2000;

        private static int LeftMargin = 60;
        private static int TopMargin = 120;

        private static PatternUnit CurrentPatternUnit;
        private static int CurrentNote;
        private static int CurrentDuration;
        private static int CurrentNoteIndex;

        private static Boolean FreePlayMode = true;
        private static Boolean BigKeys = false;

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
            StartSplashScreen();
        }

        private void InitializeKeyInterface(bool Bigkeys, bool FreePlay)
        {
            int tmp;
            int numKeys;

            FreePlayMode = FreePlay;
            BigKeys = Bigkeys;

            if (BigKeys)
                numKeys = 26;
            else
                numKeys = NoOfKeys;
            for (int i = 0; i < numKeys; i++)
            {
                Buttons[i] = new Rectangle();
                tmp = i % (numKeys / 2);
                if (tmp == 1 || tmp == 3 || tmp == 6 || tmp == 8 || tmp == 10 || tmp == 13 || tmp == 15 || tmp == 18 || tmp == 20 || tmp == 22)
                    InitializeBlackKey(i);
                else
                    InitializeWhiteKey(i);
                //InitializeRectangle(i);
            }

            InitializeEllipse();

            if (BigKeys)
            {
                TransformToThirteenKeys();
            }
            else
            {
                TransformToNKeys(NoOfKeys);
            }

            if (!FreePlayMode)
            {
                StartTest();
            }
        }

        private void StartSplashScreen()
        {
            InitializeLabel();
            InitializeText();
            InitializeButton();
        }

        private void InitializeLabel()
        {
            UserNameLabel.Name = "username";
            UserNameLabel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            UserNameLabel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            UserNameLabel.Content = "Username";
            UserNameLabel.Height = 40;
            UserNameLabel.Width = 96;
            UserNameLabel.Foreground = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

            PasswordLabel.Name = "password";
            PasswordLabel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            PasswordLabel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            PasswordLabel.Content = "Password";
            PasswordLabel.Height = 40;
            PasswordLabel.Width = 96;
            PasswordLabel.Foreground = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

            TitleLabel.Name = "Title";
            TitleLabel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            TitleLabel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            TitleLabel.Content = "Sound Bored";
            TitleLabel.Height = 124;
            TitleLabel.Width = 252;
            TitleLabel.Foreground = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xFF, 0xFF));
            TitleLabel.FontSize = 40;

            Canvas.SetLeft(UserNameLabel, 0.0);
            Canvas.SetLeft(PasswordLabel, 0.0);
            Canvas.SetLeft(TitleLabel, 0.0);
            Canvas.SetTop(UserNameLabel, 0.0);
            Canvas.SetTop(PasswordLabel, 0.0);
            Canvas.SetTop(TitleLabel, 0.0);

            UserNameLabel.Margin = new Thickness(1104, 289, 0, 0);
            UserNameLabel.Visibility = System.Windows.Visibility.Visible;
            PasswordLabel.Margin = new Thickness(1104, 338, 0, 0);
            PasswordLabel.Visibility = System.Windows.Visibility.Visible;
            TitleLabel.Margin = new Thickness(540, 258, 0, 0);
            TitleLabel.Visibility = System.Windows.Visibility.Visible;

            C.Children.Add(UserNameLabel);
            C.Children.Add(PasswordLabel);
            C.Children.Add(TitleLabel);
        }

        private void InitializeText()
        {
            username.Name = "username";
            username.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            username.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            username.Height = 23;
            username.Width = 120;

            password.Name = "password";
            password.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            password.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            password.Height = 23;
            password.Width = 120;

            Canvas.SetLeft(username, 0.0);
            Canvas.SetLeft(password, 0.0);
            Canvas.SetTop(username, 0.0);
            Canvas.SetTop(password, 0.0);

            username.Margin = new Thickness(1233, 294, 0, 0);
            username.Visibility = System.Windows.Visibility.Visible;
            password.Margin = new Thickness(1233, 343, 0, 0);
            password.Visibility = System.Windows.Visibility.Visible;

            C.Children.Add(username);
            C.Children.Add(password);
        }

        private void InitializeButton()
        {
            LoginButton.Name = "Login";
            LoginButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            LoginButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            LoginButton.Height = 30;
            LoginButton.Width = 90;
            LoginButton.Content = "Login";
            LoginButton.FontSize = 14;
            LoginButton.Click += Login_Clicked;

            FreePlayButton.Name = "FreePlay";
            FreePlayButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            FreePlayButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            FreePlayButton.Height = 30;
            FreePlayButton.Width = 90;
            FreePlayButton.Content = "Free Play!";
            FreePlayButton.FontSize = 14;
            FreePlayButton.Click += FreePlay;

            Canvas.SetLeft(LoginButton, 0.0);
            Canvas.SetLeft(LoginButton, 0.0);
            Canvas.SetTop(FreePlayButton, 0.0);
            Canvas.SetTop(FreePlayButton, 0.0);

            LoginButton.Margin = new Thickness(1266, 395, 0, 0);
            LoginButton.Visibility = System.Windows.Visibility.Visible;
            FreePlayButton.Margin = new Thickness(620, 395, 0, 0);
            FreePlayButton.Visibility = System.Windows.Visibility.Visible;

            C.Children.Add(LoginButton);
            C.Children.Add(FreePlayButton);
        }

        private void InitializeEllipse()
        {
            E.Name = "E";
            E.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            E.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            E.Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xA0, 0x00, 0xFF, 0xFF));
            Canvas.SetLeft(E, 0.0);
            Canvas.SetTop(E, 0.0);
            E.Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xA0, 0x00, 0xFF, 0xFF));
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

        private void InitializeWhiteKey(int i)
        {
            Buttons[i].Name = "B" + i;
            Buttons[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Buttons[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Buttons[i].Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            Canvas.SetLeft(Buttons[i], 0.0);
            Canvas.SetTop(Buttons[i], 0.0);
            Buttons[i].Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            Buttons[i].TouchDown += B_TouchDown;
        }

        private void InitializeBlackKey(int i)
        {
            Buttons[i].Name = "B" + i;
            Buttons[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Buttons[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Buttons[i].Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            Canvas.SetLeft(Buttons[i], 0.0);
            Canvas.SetTop(Buttons[i], 0.0);
            Buttons[i].Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            Buttons[i].TouchDown += B_TouchDown;
        }

        //TODO: FIX THIS
        private void TransformToNKeys(int n)
        {
            UnusedKeys = new HashSet<int>();

            C.Children.Clear();

            int LeftAmount = LeftMargin;
            int TopAmount = 600;
            int LeftIncrement = 100;
            int RectWidth = 100;
            int RectHeight = 360;

            for (int i = 0; i < NoOfKeys; i++)
            {
                if (i == n/2)
                {
                    LeftAmount = LeftMargin;
                    TopAmount = TopMargin;
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

            for (int i = 0; i < 26; i++)
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
                        E.Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xA0, 0x00, 0xFF, 0xFF));
                        E.Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xA0, 0x00, 0xFF, 0xFF));
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

            if (FreePlayMode)
            {
                m = new OscMessage(src, "/soundBored/playNote");
                m.Append<int>(idx);
                m.Send(dst);
                return true;
            }

            if (CuedButtonNo != EIdx)
            {
                return false;
            }

            if (CuedButtonNo == idx)
            {
                LastPlayedTime = DateTime.Now;
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

                MadeErrorOnLastNote = true;
                NoOfErrorsOnLastNote++;

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

            //JUST A TEMPORARY TESTING LINE OF CODE... CAN'T SEE THESE FOUR KEYS ON MY SCREEN. ;)
            while (Random > 9 && Random < 13 || Random > 22 && Random < 26)//TESTING
            {//TESTING
                Random = (new Random()).Next(26);//TESTING
            }//TESTING

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

        private ArrayList ReadPatternFromFile(string FileName)
        {
            ArrayList ReadPattern = new ArrayList();
            PatternUnit TempPatternUnit;

            FileStream fs = new FileStream(FileName, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            string TempoString = sr.ReadLine();
            Tempo = Int32.Parse(TempoString);

            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                string[] Pieces = line.Split(':');
                int note = int.Parse(Pieces[0]);
                int duration = int.Parse(Pieces[1]);
                TempPatternUnit = new PatternUnit(note, duration);
                ReadPattern.Add(TempPatternUnit);
            }

            sr.Close();
            fs.Close();

            Quarter = 1 / (Tempo / 60);
            Half = 2 * Quarter;
            Whole = 2 * Half;

            Eighth = Quarter / 2;
            Sixteenth = Eighth / 2;
            ThirtySecondth = Sixteenth / 2;

            return ReadPattern;    
        }

        private void StartTest()
        {
            //TODO What happens when you click Start Test
            TimerIncrement = ThirtySecondth;
            AppTimer = new Timer(TimerIncrement);

            Pattern = GenerateRandomPattern(6, 5);
            CurrentDuration = 0;
            CurrentNoteIndex = -1;

            LatenessThreshold = Quarter;

            AppTimer.Elapsed += HandleTimerElapsedEvent;
            AppTimer.Start();
            AppTimer.Interval = TimerIncrement;
        }

        private void Login_Clicked(object sender, RoutedEventArgs e)
        {
            InitializeKeyInterface(true, false);
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

        private void FreePlay(object sender, RoutedEventArgs e)
        {
            //TODO What happens when you click Free Play
            InitializeKeyInterface(false, true);
        }

        private void ShowTestResults()
        { 
            //TODO Display Results of the Test
            //NECESSARY CRAP IF YOU WANT TO MODIFY ANY CONTROL THAT'S OWNED BY THE MAIN THREAD
            ChartCanvas.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        ChartCanvas.Visibility = System.Windows.Visibility.Visible;
                    }
                ));
        }

        private void HandleTimerElapsedEvent(Object Source, ElapsedEventArgs e)
        { 
            //TODO What happens when AppTimer's Elapsed Event Fire

            if (CurrentDuration == 0)
            {
                if (CurrentNoteIndex > -1)
                { 
                    
                }

                if (CurrentNoteIndex == Pattern.Count - 1)
                {
                    AppTimer.Enabled = false;
                    HideVisualCue();
                    ShowTestResults();

                    Console.WriteLine("Test Done : " + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);
                    return;
                }

                CurrentNoteIndex++;

                ((PatternUnit)Pattern[CurrentNoteIndex]).CorrectTime = DateTime.Now;
                ((PatternUnit)Pattern[CurrentNoteIndex]).MadeErrors = MadeErrorOnLastNote;
                ((PatternUnit)Pattern[CurrentNoteIndex]).NoOfErrors = NoOfErrorsOnLastNote;

                MadeErrorOnLastNote = false;
                NoOfErrorsOnLastNote = 0;

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
                ((PatternUnit)Pattern[CurrentNoteIndex]).ActualTime = LastPlayedTime;
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
        private DateTime correcttime = new DateTime();
        private DateTime actualtime = new DateTime();
        private bool islate = false;
        private bool madeerrors = false;
        private int nooferrors;

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

        public DateTime CorrectTime
        {
            get
            {
                return correcttime;
            }

            set
            {
                correcttime = value;
            }
        }

        public DateTime ActualTime
        {
            get
            {
                return actualtime;
            }

            set
            {
                actualtime = value;
            }
        }

        public bool IsLate
        {
            get
            {
                return islate;
            }

            set
            {
                islate = value;
            }
        }

        public bool MadeErrors
        {
            get
            {
                return madeerrors;
            }

            set
            {
                madeerrors = value;
            }
        }

        public int NoOfErrors
        {
            get
            {
                return nooferrors;
            }

            set
            {
                nooferrors = value;
            }
        }

        public void IncrementErrorsMade()
        {
            nooferrors++;
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