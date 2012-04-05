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
        private TimeSpan LatenessThreshold;

        private static int NoOfKeys = 88;
        private HashSet<int> UnusedKeys;
        private Rectangle[] Buttons = new Rectangle[NoOfKeys];
        private Ellipse E = new Ellipse();

        private Label UserNameLabel = new Label();
        private Label PasswordLabel = new Label();
        private Label TitleLabel = new Label();
        
        private SurfaceButton LoginButton = new SurfaceButton();
        private SurfaceButton FreePlaySinglePlayerButton = new SurfaceButton();
        private SurfaceButton FreePlayTwoPlayerButton = new SurfaceButton();
        private SurfaceButton LogoutButton = new SurfaceButton();
        private SurfaceButton RandWalkButton = new SurfaceButton();
        private SurfaceButton RandAIButton = new SurfaceButton();
        private SurfaceButton PreloadButton = new SurfaceButton();
        private SurfaceButton DemoButton = new SurfaceButton();
        private SurfaceButton MenuButton = new SurfaceButton();
        private SurfaceButton BackButton = new SurfaceButton();

        private SurfaceTextBox UserNameTextBox = new SurfaceTextBox();
        private SurfaceTextBox PasswordTextBox = new SurfaceTextBox();



        private string UserName;
        private string Password;
        private string UserFileName;
        private const string PatternPath = "../../patterns/";
        private const string UserDataPath = "../../userdata/";
        
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
        private static int pianoWidth = 1920 - (2 * LeftMargin) - (int)(1920*.185);

        private static PatternUnit CurrentPatternUnit;
        private static int CurrentNote;
        private static int CurrentDuration;
        private static int CurrentNoteIndex;
        private static int KeyMidiNote = 60;

        private static Boolean FreePlayMode = true;
        private static Boolean BigKeys = false;
        private static Boolean twoPlayer = false;

        //ArrayList Pattern has elements of type PatternUnit converted to Object, so don't forget data conversions when adding or removing elements from Pattern
        private ArrayList Pattern = new ArrayList();
        private ArrayList PatternMetrics = new ArrayList();

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

        private void TransformKeyInterface(bool Bigkeys, bool FreePlay, String key)
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
                tmp = i % (12);
                if (tmp == 1 || tmp == 4 || tmp == 6 || tmp == 8 || tmp == 11)
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
                if (twoPlayer)
                    TransformTwoPlayerNKeys(NoOfKeys, key);
                else
                    TransformOnePlayerNKeys(NoOfKeys, key);
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
            TransformStartScreen();
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
        }

        private void InitializeText()
        {
            UserNameTextBox.Name = "username";
            UserNameTextBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            UserNameTextBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            UserNameTextBox.Height = 23;
            UserNameTextBox.Width = 120;

            PasswordTextBox.Name = "password";
            PasswordTextBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            PasswordTextBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            PasswordTextBox.Height = 23;
            PasswordTextBox.Width = 120;

            Canvas.SetLeft(UserNameTextBox, 0.0);
            Canvas.SetLeft(PasswordTextBox, 0.0);
            Canvas.SetTop(UserNameTextBox, 0.0);
            Canvas.SetTop(PasswordTextBox, 0.0);

            UserNameTextBox.Margin = new Thickness(1233, 294, 0, 0);
            UserNameTextBox.Visibility = System.Windows.Visibility.Visible;
            PasswordTextBox.Margin = new Thickness(1233, 343, 0, 0);
            PasswordTextBox.Visibility = System.Windows.Visibility.Visible;

           
        }

        private void InitializeButton()
        {
            LoginButton.Name = "Login";
            LoginButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            LoginButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            LoginButton.Height = 60;
            LoginButton.Width = 180;
            LoginButton.Content = "Login";
            LoginButton.FontSize = 14;
            LoginButton.Click += Login_Clicked;

            FreePlaySinglePlayerButton.Name = "FreePlay";
            FreePlaySinglePlayerButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            FreePlaySinglePlayerButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            FreePlaySinglePlayerButton.Height = 60;
            FreePlaySinglePlayerButton.Width = 180;
            FreePlaySinglePlayerButton.Content = "Free Play!";
            FreePlaySinglePlayerButton.FontSize = 14;
            FreePlaySinglePlayerButton.Click += FreePlaySinglePlayer;
            
            FreePlayTwoPlayerButton.Name = "TwoplayerFreePlay";
            FreePlayTwoPlayerButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            FreePlayTwoPlayerButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            FreePlayTwoPlayerButton.Height = 60;
            FreePlayTwoPlayerButton.Width = 180;
            FreePlayTwoPlayerButton.Content = "2 Person Free Play!";
            FreePlayTwoPlayerButton.FontSize = 14;
            FreePlayTwoPlayerButton.Click += FreePlayTwoPlayer;

            LogoutButton.Name = "Logout";
            LogoutButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            LogoutButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            LogoutButton.Height = 60;
            LogoutButton.Width = 180;
            LogoutButton.Content = "Logout";
            LogoutButton.FontSize = 14;
            LogoutButton.Click += Logout_Clicked;

            RandWalkButton.Name = "RandWalk";
            RandWalkButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RandWalkButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            RandWalkButton.Height = 60;
            RandWalkButton.Width = 180;
            RandWalkButton.Content = "Play Random Walk!";
            RandWalkButton.FontSize = 14;
            RandWalkButton.Click += RandWalk_Clicked;

            RandAIButton.Name = "RandAI";
            RandAIButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RandAIButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            RandAIButton.Height = 60;
            RandAIButton.Width = 180;
            RandAIButton.Content = "Play Random AI!";
            RandAIButton.FontSize = 14;

            PreloadButton.Name = "Preload";
            PreloadButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            PreloadButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            PreloadButton.Height = 60;
            PreloadButton.Width = 180;
            PreloadButton.Content = "Play Preloaded Song!";
            PreloadButton.FontSize = 14;

            DemoButton.Name = "Demo";
            DemoButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            DemoButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            DemoButton.Height = 60;
            DemoButton.Width = 180;
            DemoButton.Content = "Play Demo!";
            DemoButton.FontSize = 14;

            MenuButton.Name = "Menu";
            MenuButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            MenuButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            MenuButton.Height = 60;
            MenuButton.Width = 180;
            MenuButton.Content = "Menu";
            MenuButton.FontSize = 14;
            MenuButton.Click += Menu_Clicked;

            BackButton.Name = "Back";
            BackButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            BackButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            BackButton.Height = 60;
            BackButton.Width = 180;
            BackButton.Content = "Back";
            BackButton.FontSize = 14;
            BackButton.Click += Logout_Clicked;

            Canvas.SetTop(LogoutButton, 0.0);
            Canvas.SetLeft(LogoutButton, 0.0);
            Canvas.SetTop(RandWalkButton, 0.0);
            Canvas.SetLeft(RandWalkButton, 0.0);
            Canvas.SetTop(RandAIButton, 0.0);
            Canvas.SetLeft(RandAIButton, 0.0);
            Canvas.SetTop(PreloadButton, 0.0);
            Canvas.SetLeft(PreloadButton, 0.0);
            Canvas.SetTop(DemoButton, 0.0);
            Canvas.SetLeft(DemoButton, 0.0);

            Canvas.SetTop(MenuButton, 0.0);
            Canvas.SetLeft(MenuButton, 0.0);

            Canvas.SetTop(BackButton, 0.0);
            Canvas.SetLeft(BackButton, 0.0);

            Canvas.SetTop(LoginButton, 0.0);
            Canvas.SetLeft(LoginButton, 0.0);
            Canvas.SetTop(FreePlaySinglePlayerButton, 0.0);
            Canvas.SetLeft(FreePlaySinglePlayerButton, 0.0);
            Canvas.SetTop(FreePlayTwoPlayerButton, 0.0);
            Canvas.SetLeft(FreePlayTwoPlayerButton, 0.0);

            LogoutButton.Margin = new Thickness(1700, 20, 0, 0);
            LogoutButton.Visibility = System.Windows.Visibility.Visible;
            RandWalkButton.Margin = new Thickness(320, 395, 0, 0);
            RandWalkButton.Visibility = System.Windows.Visibility.Visible;
            RandAIButton.Margin = new Thickness(720, 395, 0, 0);
            RandAIButton.Visibility = System.Windows.Visibility.Visible;
            PreloadButton.Margin = new Thickness(1120, 395, 0, 0);
            PreloadButton.Visibility = System.Windows.Visibility.Visible;
            DemoButton.Margin = new Thickness(1520, 395, 0, 0);
            DemoButton.Visibility = System.Windows.Visibility.Visible;
            
            MenuButton.Margin = new Thickness(1700, 20, 0, 0);
            MenuButton.Visibility = System.Windows.Visibility.Visible;

            BackButton.Margin = new Thickness(1700, 20, 0, 0);
            BackButton.Visibility = System.Windows.Visibility.Visible;

            LoginButton.Margin = new Thickness(1266, 395, 0, 0);
            LoginButton.Visibility = System.Windows.Visibility.Visible;
            FreePlaySinglePlayerButton.Margin = new Thickness(620, 395, 0, 0);
            FreePlaySinglePlayerButton.Visibility = System.Windows.Visibility.Visible;
            FreePlayTwoPlayerButton.Margin = new Thickness(620, 595, 0, 0);
            FreePlayTwoPlayerButton.Visibility = System.Windows.Visibility.Visible;
            
        }

        private void TransformStartScreen()
        {
            C.Children.Clear();

            C.Children.Add(UserNameLabel);
            C.Children.Add(PasswordLabel);
            C.Children.Add(TitleLabel);
            
            C.Children.Add(UserNameTextBox);
            C.Children.Add(PasswordTextBox);
            
            C.Children.Add(LoginButton);
            C.Children.Add(FreePlaySinglePlayerButton);
            C.Children.Add(FreePlayTwoPlayerButton);
        }

        private void TransformMenu()
        {
            C.Children.Clear();

            C.Children.Add(LogoutButton);
            C.Children.Add(RandWalkButton);
            C.Children.Add(RandAIButton);
            C.Children.Add(PreloadButton);
            C.Children.Add(DemoButton);
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
            //Buttons[i].TouchUp += B_TouchUp;
        }

        private void InitializeWhiteKey(int i)
        {
            Buttons[i].Name = "W" + i;
            Buttons[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Buttons[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Buttons[i].Stroke = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            Canvas.SetLeft(Buttons[i], 0.0);
            Canvas.SetTop(Buttons[i], 0.0);
            Buttons[i].Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            Buttons[i].TouchDown += B_TouchDown;
            Buttons[i].TouchUp += B_TouchUp;
            Buttons[i].TouchLeave += B_TouchUp;
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
            Buttons[i].TouchUp += B_TouchUp;
            Buttons[i].TouchLeave += B_TouchUp;
        }

        //TODO: FIX THIS -- Hanging black key in between levels
        private void TransformTwoPlayerNKeys(int n, String key)
        {
            UnusedKeys = new HashSet<int>();

            C.Children.Clear();

            int LeftAmount = (1920 - pianoWidth) / 2;
            int TopAmount = 600;
            int RectWidth = pianoWidth / 26;
            int RectHeight = 360;

            for (int i = 0; i < NoOfKeys; i++)
            {
                if (i == n/2)
                {
                    LeftAmount = (1920 - pianoWidth) / 2;
                    TopAmount = TopMargin;
                    if (twoPlayer)
                    {
                        LeftAmount = 1920 - LeftAmount - RectWidth;
                        RectWidth = -pianoWidth / 26;
                        TopAmount = TopMargin;
                    }
                }
                
                if (Buttons[i].Name.Equals("W" + i))
                {
                    Buttons[i].Width = Math.Abs(RectWidth);
                    Buttons[i].Height = RectHeight;
                    Buttons[i].Margin = new Thickness(LeftAmount, TopAmount, 0, 0);
                    Buttons[i].Visibility = System.Windows.Visibility.Visible;
                    C.Children.Add(Buttons[i]);
                    LeftAmount += RectWidth;
                }
                else
                {
                    Buttons[i].Width = Math.Abs(RectWidth) / 2;
                    Buttons[i].Height = RectHeight - 100;
                    LeftAmount -= Math.Abs(RectWidth) / 4;
                    Buttons[i].Margin = new Thickness(LeftAmount, TopAmount, 0, 0);
                    if (i >= n / 2 && twoPlayer)
                        Buttons[i].Margin = new Thickness(LeftAmount-RectWidth, TopAmount+100, 0, 0);
                    Buttons[i].Visibility = System.Windows.Visibility.Visible;
                    LeftAmount += Math.Abs(RectWidth) / 4;
                }
            }
            for (int i = 0; i < NoOfKeys; i++)
            {
                if (Buttons[i].Name.Equals("B"+i))
                {
                    C.Children.Add(Buttons[i]);
                }
            }
            C.Children.Add(BackButton);
        }
        // TODO: FIX -- Give the player the middle keys close and the low keys above and left and the high keys above and right
        private void TransformOnePlayerNKeys(int n, String key)
        {
            UnusedKeys = new HashSet<int>();

            C.Children.Clear();

            int LeftAmount = (1920 - pianoWidth) / 2;
            int TopAmount = 600;
            int RectWidth = pianoWidth / 26;
            int RectHeight = 360;

            for (int i = 0; i < NoOfKeys; i++)
            {
                if (i == n / 2)
                {
                    LeftAmount = (1920 - pianoWidth) / 2;
                    TopAmount = TopMargin;
                    if (twoPlayer)
                    {
                        LeftAmount = 1920 - LeftAmount - RectWidth;
                        RectWidth = -pianoWidth / 26;
                        TopAmount = TopMargin;
                    }
                }

                if (Buttons[i].Name.Equals("W" + i))
                {
                    Buttons[i].Width = Math.Abs(RectWidth);
                    Buttons[i].Height = RectHeight;
                    Buttons[i].Margin = new Thickness(LeftAmount, TopAmount, 0, 0);
                    Buttons[i].Visibility = System.Windows.Visibility.Visible;
                    C.Children.Add(Buttons[i]);
                    LeftAmount += RectWidth;
                }
                else
                {
                    Buttons[i].Width = Math.Abs(RectWidth) / 2;
                    Buttons[i].Height = RectHeight - 100;
                    LeftAmount -= Math.Abs(RectWidth) / 4;
                    Buttons[i].Margin = new Thickness(LeftAmount, TopAmount, 0, 0);
                    if (i >= n / 2 && twoPlayer)
                        Buttons[i].Margin = new Thickness(LeftAmount - RectWidth, TopAmount + 100, 0, 0);
                    Buttons[i].Visibility = System.Windows.Visibility.Visible;
                    LeftAmount += Math.Abs(RectWidth) / 4;
                }
            }
            for (int i = 0; i < NoOfKeys; i++)
            {
                if (Buttons[i].Name.Equals("B" + i))
                {
                    C.Children.Add(Buttons[i]);
                }
            }
            C.Children.Add(BackButton);
        }

        //Changes layout to 26 keys 0 - 25
        private void TransformToThirteenKeys()
        {
            UnusedKeys = new HashSet<int>();

            C.Children.Clear();

            int LeftAmount = 65;
            int TopAmount = 600;
            int LeftIncrement = 140;
            int RectWidth = 110;
            int RectHeight = 360;

            for (int i = 0; i < 26; i++)
            {
                if (i == 13)
                {
                    LeftAmount = 65;
                    TopAmount = TopMargin;
                }

                Buttons[i].Width = RectWidth;
                Buttons[i].Height = RectHeight;
                Buttons[i].Margin = new Thickness(LeftAmount, TopAmount, 0, 0);
                Buttons[i].Visibility = System.Windows.Visibility.Visible;

                C.Children.Add(Buttons[i]);

                LeftAmount += LeftIncrement;
            }

            C.Children.Add(MenuButton);

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
            int index = int.Parse(fe.Name.Substring(1));
            if (fe.Name.Substring(0,1).Equals("W"))
                Buttons[index].Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0xB1, 0xB1, 0xB1));
            else
                Buttons[index].Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0x42, 0x42, 0x42));

            //if (handled)
            //{
            //    HideVisualCue();
            //    ShowVisualCue(getRandomButton());
            //}
        }

        //Handles releasing rectangles
        private void B_TouchUp(object sender, TouchEventArgs e)
        {
            FrameworkElement fe = e.Source as FrameworkElement;
            String letter = fe.Name.Substring(0,1);
            int index = int.Parse(fe.Name.Substring(1));
            if (letter.Equals("B"))
                Buttons[index].Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            else
                Buttons[index].Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
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

        private ArrayList ReadHistoryFromFile(string FileName)
        {
            ArrayList History = new ArrayList();

            FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate);
            StreamReader sr = new StreamReader(fs);

            string TempDifficulty;
            string TempAccuracy;
            string TempDateTime;

            while((TempDateTime = sr.ReadLine()) != null)
            {
                TempAccuracy = sr.ReadLine();
                TempDifficulty = sr.ReadLine();

                History.Add(new PatternMetric(DateTime.Parse(TempDateTime), int.Parse(TempDifficulty), double.Parse(TempAccuracy)));
            }

            return History;
        }

        private bool WriteHistoryToFile(string FileName)
        {
            Console.WriteLine("Write User History");

            FileStream fs = new FileStream(FileName, FileMode.Create);
            StreamWriter sr = new StreamWriter(fs);

            try
            {
                foreach (PatternMetric Metric in PatternMetrics)
                {
                    sr.WriteLine(Metric.PatternTimePlayed.ToString());
                    Console.WriteLine(Metric.PatternTimePlayed.ToString());
                    sr.WriteLine(Metric.PatternDifficulty.ToString());
                    Console.WriteLine(Metric.PatternDifficulty.ToString());
                    sr.WriteLine(Metric.PatternAccuracy.ToString());
                    Console.WriteLine(Metric.PatternAccuracy.ToString());
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Write Error: " + e.Message);
                return false;
            }

            return true;
        }

        private ArrayList ReadPatternFromFile(string FileName)
        {
            ArrayList ReadPattern = new ArrayList();
            PatternUnit TempPatternUnit;

            FileStream fs = new FileStream(FileName, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            string TempString;

            while((TempString = sr.ReadLine()).IndexOf("//") != -1)
            {   
            }
            
            Tempo = Int32.Parse(TempString);

            while ((TempString = sr.ReadLine()).IndexOf("//") != -1)
            {
            }

            KeyMidiNote = Int32.Parse(TempString);

            while ((TempString = sr.ReadLine()).IndexOf("//") != -1)
            {
            }

            PatternMetrics.Add(new PatternMetric());
            ((PatternMetric)PatternMetrics[PatternMetrics.Count - 1]).PatternDifficulty = Int32.Parse(TempString);
            ((PatternMetric)PatternMetrics[PatternMetrics.Count - 1]).PatternTimePlayed = DateTime.Now;

            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                if (line.IndexOf("//") != -1)
                {
                    continue;
                }

                string[] Pieces = line.Split(':');
                int note = int.Parse(Pieces[0]); //MIDI Note Number
                note -= KeyMidiNote;

                if (note < 0)
                {
                    note = -1;
                }
                else if (note > NoOfKeys)
                {
                    note = NoOfKeys - 1;
                }

                int duration = int.Parse(Pieces[1]);
                TempPatternUnit = new PatternUnit(note, duration);
                ReadPattern.Add(TempPatternUnit);
            }

            sr.Close();
            fs.Close();

            Quarter = 1000.0 / (Tempo / 60);
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

            //Pattern = GenerateRandomPattern(6, 5);
            Pattern = ReadPatternFromFile(PatternPath + "RandomPattern.pat");
            CurrentDuration = 0;
            CurrentNoteIndex = -1;

            LatenessThreshold = new TimeSpan((long)(Half * 10000));

            AppTimer.Elapsed += HandleTimerElapsedEvent;
            AppTimer.Start();
            AppTimer.Interval = TimerIncrement;
        }

        private void Login_Clicked(object sender, RoutedEventArgs e)
        {
            UserName = UserNameTextBox.Text;
            Password = PasswordTextBox.Text;

            if (UserName.Length == 0 || Password.Length == 0)
            {
                Console.WriteLine("Username or Password was empty");
                return;
            }

            UserFileName = HashGenerate(UserName, Password) + ".user";

            ReadHistoryFromFile(UserDataPath + UserFileName);

            //InitializeKeyInterface(true, false, "C");
            TransformMenu();
        }

        private void Logout_Clicked(object sender, RoutedEventArgs e)
        {
            UserName = null;
            Password = null;
            UserNameTextBox.Text = "";
            PasswordTextBox.Text = "";
            Console.WriteLine("Logging out...");
            TransformStartScreen();
        }

        private void RandWalk_Clicked(object sender, RoutedEventArgs e)
        {
            TransformKeyInterface(true, false, "C");
        }

        private void Menu_Clicked(object sender, RoutedEventArgs e)
        {
            ChartCanvas.Children.Remove(MenuButton);
            StopTest();
            AppTimer.Enabled = false;
            ChartCanvas.Visibility = System.Windows.Visibility.Collapsed;
            TransformMenu();
        }

        private string HashGenerate(string UserName, string Password)
        {
            string GeneratedHash = "";
            byte[] unamebytes;
            byte[] pwordbytes;

            while (UserName.Length < 25)
            {
                UserName += UserName;
            }

            UserName = UserName.Substring(0, 25);

            unamebytes = Encoding.ASCII.GetBytes(UserName.ToCharArray());

            while (Password.Length < 25)
            {
                Password += Password;
            }

            Password = Password.Substring(0, 25);

            pwordbytes = Encoding.ASCII.GetBytes(Password.ToCharArray());

            for (int i = 0; i < 25; i++)
            {
                GeneratedHash += (unamebytes[i] + (byte)(Math.Pow(-1, i)) * pwordbytes[i]).ToString();
            }

            return GeneratedHash;
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

        private void FreePlaySinglePlayer(object sender, RoutedEventArgs e)
        {
            //TODO What happens when you click Free Play
            twoPlayer = false;
            TransformKeyInterface(false, true, "C");
        }
        private void FreePlayTwoPlayer(object sender, RoutedEventArgs e)
        {
            twoPlayer = true;
            TransformKeyInterface(false, true, "C");
        }

        private void ShowTestResults()
        { 
            //TODO Display Results of the Test

            WriteHistoryToFile(UserDataPath + UserFileName);
            C.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        C.Children.Remove(MenuButton);
                    }
                ));
            //NECESSARY CRAP IF YOU WANT TO MODIFY ANY CONTROL THAT'S OWNED BY THE MAIN THREAD
            ChartCanvas.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        ChartCanvas.Children.Add(MenuButton);
                        ChartCanvas.Visibility = System.Windows.Visibility.Visible;
                    }
                ));
        }

        private void HandleTimerElapsedEvent(Object Source, ElapsedEventArgs e)
        { 
            //What happens when AppTimer's Elapsed Event Fire

            if (CurrentDuration == 0)
            {
                if (CurrentNoteIndex == Pattern.Count - 1)
                {
                    ((PatternUnit)Pattern[CurrentNoteIndex]).MadeErrors = MadeErrorOnLastNote;
                    ((PatternUnit)Pattern[CurrentNoteIndex]).NoOfErrors = NoOfErrorsOnLastNote;
                    ((PatternUnit)Pattern[CurrentNoteIndex]).IsLate = (((PatternUnit)Pattern[CurrentNoteIndex - 1]).ActualTime - ((PatternUnit)Pattern[CurrentNoteIndex - 1]).CorrectTime) > LatenessThreshold;

                    AppTimer.Enabled = false;
                    HideVisualCue();
                    ShowTestResults();

                    Console.WriteLine("Test Done : " + e.SignalTime.Minute + ":" + e.SignalTime.Second + ":" + e.SignalTime.Millisecond);
                    return;
                }

                CurrentNoteIndex++;

                ((PatternUnit)Pattern[CurrentNoteIndex]).CorrectTime = DateTime.Now;

                if (CurrentNoteIndex > 0)
                {
                    ((PatternUnit)Pattern[CurrentNoteIndex - 1]).MadeErrors = MadeErrorOnLastNote;
                    ((PatternUnit)Pattern[CurrentNoteIndex - 1]).NoOfErrors = NoOfErrorsOnLastNote;
                    ((PatternUnit)Pattern[CurrentNoteIndex - 1]).IsLate = (((PatternUnit)Pattern[CurrentNoteIndex - 1]).ActualTime - ((PatternUnit)Pattern[CurrentNoteIndex - 1]).CorrectTime) > LatenessThreshold;
                }

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

    class PatternMetric
    {
        private DateTime patterntimeplayed;
        private int patterndifficulty;
        private double patternaccuracy;

        public DateTime PatternTimePlayed
        {
            get
            {
                return patterntimeplayed;
            }
            set
            {
                patterntimeplayed = value;
            }
        }

        public int PatternDifficulty
        {
            get
            {
                return patterndifficulty;
            }
            set
            {
                patterndifficulty = value;
            }
        }

        public double PatternAccuracy
        {
            get
            {
                return patternaccuracy;
            }
            set
            {
                patternaccuracy = value;
            }
        }

        public PatternMetric()
        { 
            
        }

        public PatternMetric(DateTime TimePlayed, int Difficulty, double Accuracy)
        {
            this.PatternTimePlayed = TimePlayed;
            PatternDifficulty = Difficulty;
            PatternAccuracy = Accuracy;
        }
    }
}