using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace PomodoroApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new();
        private int time = 0;
        private bool isWorkCycle = true;
        private readonly MediaPlayer player = new();
        private int numberOfPomodoro = 0;
        public int timeOfWork = 25;
        public int timeOfRest = 900;
        private Forms.NotifyIcon nIcon = new();
        private ContextMenuStrip contextMenu;
        private MiniTimer miniTimer;
        private Options optionsWindow;
        private string currentNotificationSound = "Sounds/notif.mp3";
        private DispatcherTimer stopSoundTimer;

        //Hotkey
        private const int HOTKEY_ID = 1;
        private const int HOTKEY_ID_MINITIMER = 2;
        private const int HOTKEY_ID_RESTORE = 3;
        private const uint MOD_CONTROL = 0x0002; // Control key
        private const uint VK_I = 0x49; // 'I' key
        private const uint VK_H = 0x48; // 'H' key
        private const uint VK_R = 0x52; // 'R' key
        private const int WM_HOTKEY = 0x0312;
        private HwndSource _source;


        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public MainWindow()
        {
            InitializeComponent();
            DrawClockFace();
            LoadSoundCloudMusic();
            timer.Interval = TimeSpan.FromSeconds(1);
            //Call the methode
            timer.Tick += Timer_Tick;
            frontTimer.Content = FormatTime(time);

            //NotifyIcon init
            nIcon.Icon = new System.Drawing.Icon("Icon/pomodoro.ico");
            nIcon.Text = "Pomodoro Timer";
            nIcon.Visible = true;
            nIcon.DoubleClick += NotifyDoubleClick;

            //Creation of the contect menu
            contextMenu = new ContextMenuStrip();
            // Créé un item dans le menu pour apres assigné un evènement puis l'ajouté au menu
            ToolStripMenuItem ouvrirItem = new ToolStripMenuItem("Ouvrir");
            ouvrirItem.Click += (s, e) => Restaure();
            contextMenu.Items.Add(ouvrirItem);

            ToolStripMenuItem options = new ToolStripMenuItem("Options");
            options.Click += (s, e) => OpenOptions();

            //Close the context menu
            ToolStripMenuItem quit = new ToolStripMenuItem("Quitter");
            quit.Click += (s, e) => CloseAllWindows();
            contextMenu.Items.Add(quit);


            contextMenu.Items.Add(options);

            nIcon.ContextMenuStrip = contextMenu;
            Debug.WriteLine($"{timeOfRest} et {timeOfWork}");

        }

        private void DrawClockFace()
        {
            // Centre et Rayon du cadran
            double centerX = 100;
            double centerY = 100;
            double radius = 100;

            // On boucle de 0 à 59 (les 60 secondes)
            for (int i = 0; i < 60; i++)
            {
                // Calcul de l'angle (6 degrés par seconde)
                double angleDegree = i * 6;
                double angleRad = (angleDegree - 90) * (Math.PI / 180);

                // Déterminer si c'est un "Gros trait" (toutes les 5 secondes) ou un petit
                bool isMajor = (i % 5 == 0);

                // Configuration du trait
                double lineLength = isMajor ? 15 : 8; // Longueur du trait
                double thickness = isMajor ? 3 : 1;   // Épaisseur
                System.Windows.Media.Brush color = System.Windows.Media.Brushes.Black;

                // Calcul des coordonnées du trait
                // Point extérieur (sur le bord du cercle)
                double xOuter = centerX + radius * Math.Cos(angleRad);
                double yOuter = centerY + radius * Math.Sin(angleRad);

                // Point intérieur
                double xInner = centerX + (radius - lineLength) * Math.Cos(angleRad);
                double yInner = centerY + (radius - lineLength) * Math.Sin(angleRad);

                // Création de la ligne
                System.Windows.Shapes.Line line = new System.Windows.Shapes.Line()
                {
                    X1 = xOuter,
                    Y1 = yOuter,
                    X2 = xInner,
                    Y2 = yInner,
                    Stroke = color,
                    StrokeThickness = thickness
                };

                ClockCanvas.Children.Add(line);

                // Si c'est un point majeur (0, 5, 10...), on ajoute le chiffre
                if (isMajor)
                {
                    // Le texte "60" remplace le "0" pour faire plus joli
                    string numberText = (i == 0) ? "60" : i.ToString();

                    TextBlock tb = new TextBlock()
                    {
                        Text = numberText,
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.Black
                    };

                    // On place le texte un peu plus à l'intérieur (radius - 25)
                    double textRadius = radius - 28;
                    double xText = centerX + textRadius * Math.Cos(angleRad);
                    double yText = centerY + textRadius * Math.Sin(angleRad);

                    // On doit centrer le TextBlock par rapport à son point calculé
                    // Comme on ne connait pas sa taille exacte avant le rendu, on estime ou on utilise Measure
                    // Ici une astuce simple : Canvas.Left/Top
                    tb.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(tb, xText - (tb.DesiredSize.Width / 2));
                    Canvas.SetTop(tb, yText - (tb.DesiredSize.Height / 2));

                    ClockCanvas.Children.Add(tb);
                }
            }
        }

        private async void LoadSoundCloudMusic()
        {
            try
            {
                await MusicPlayer.EnsureCoreWebView2Async();

                string targetUrl = "https://soundcloud.com/lofi_girl/sets/chill-guitar";
                string encodedUrl = Uri.EscapeDataString(targetUrl);

                // CHANGEMENT ICI : visual=false
                // Cela force le lecteur en mode "barre fine" classique
                string widgetUrl = $"https://w.soundcloud.com/player/?url={encodedUrl}&visual=false&auto_play=false&show_artwork=true";

                MusicPlayer.Source = new Uri(widgetUrl);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erreur SoundCloud : " + ex.Message);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                // Hide the window when minimized
                this.Hide();
            }
            else if (WindowState == WindowState.Normal)
            {
                // Show the window when restored
                this.Show();
            }
        }
        private void Restaure()
        {
            // Show the window when the icon is double-clicked
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();

        }
        private void NotifyDoubleClick(Object sender, EventArgs e)
        {
            // Show the window when the icon is double-clicked
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();

        }

        private void ToggleMainWindow()
        {
            if (this.IsVisible && this.WindowState != WindowState.Minimized)
            {
                this.Hide();
            }
            else
            {
                Restaure();
            }
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
            ChangeTextButton();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            time++;
            frontTimer.Content = FormatTime(time);

            double angle = (time % 60) * (360.0 / 60.0);
            HandRotation.Angle = angle;

            if (miniTimer != null && miniTimer.IsVisible)
            {
                miniTimer.UpdateTimer(FormatTime(time), angle);
            }

            //2500
            if (isWorkCycle && time == timeOfWork)
            {
                timer.Stop();
                PlayNotificationSound();
                new AutoCloseMessage("Temps de Repos Unlock", 3).Show();
                SwitchCycle();
            }
            if (numberOfPomodoro != 4)
            {
                //300
                if (!isWorkCycle && time == timeOfRest)
                {
                    timer.Stop();
                    PlayNotificationSound();
                    new AutoCloseMessage("Au travail", 3).Show();
                    SwitchCycle();
                }
                else
                {
                    if (!isWorkCycle && time == timeOfWork)
                    {
                        timer.Stop();
                        numberOfPomodoro = 0;
                        PlayNotificationSound();
                        new AutoCloseMessage("Temps de Repos Unlock", 3).Show();
                        SwitchCycle();
                    }
                }
            }

        }

        private void SwitchCycle()
        {
            if (isWorkCycle)
            {
                numberOfPomodoro++;
                Debug.WriteLine($"Nombre de Pomodoro : {numberOfPomodoro}");
            }
            isWorkCycle = !isWorkCycle;
            time = 0;
            frontTimer.Content = FormatTime(time);
            timer.Start();
        }

        private string FormatTime(int global)
        {
            int minutes = global / 60;
            int secs = global % 60;

            return $"{minutes:D2}:{secs:D2}";
        }

        private void ChangeTextButton()
        {
            if (!timer.IsEnabled)
            {
                StartPauseButton.Content = "Start";
            }
            else
            {
                StartPauseButton.Content = "Pause";
            }
        }
        private void PlayNotificationSound()
        {
            try
            {
                player.Open(new Uri(currentNotificationSound, UriKind.RelativeOrAbsolute));
                player.Play();

                // Stop sound 5 sec later
                stopSoundTimer = new DispatcherTimer();
                stopSoundTimer.Interval = TimeSpan.FromSeconds(5);
                stopSoundTimer.Tick += StopSoundTimer_Tick;
                stopSoundTimer.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lecture son : {ex.Message}");
            }
        }

        private void StopSoundTimer_Tick(object sender, EventArgs e)
        {
            stopSoundTimer.Stop();
            player.Stop();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var handle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(handle);
            _source.AddHook(HwndHook);

            // Enregistre Ctrl+I
            RegisterHotKey(handle, HOTKEY_ID, MOD_CONTROL, VK_I);
            // Enregistre Ctrl+H pour le mini-timer
            RegisterHotKey(handle, HOTKEY_ID_MINITIMER, MOD_CONTROL, VK_H);
            // Enregistre Ctrl+R pour restaurer la fenêtre principale
            RegisterHotKey(handle, HOTKEY_ID_RESTORE, MOD_CONTROL, VK_R);
        }

        protected override void OnClosed(EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
            UnregisterHotKey(handle, HOTKEY_ID_MINITIMER);
            UnregisterHotKey(handle, HOTKEY_ID_RESTORE);
            _source.RemoveHook(HwndHook);

            if (nIcon != null)
            {
                nIcon.Dispose(); // Removes the icon from the tray immediately
                nIcon = null;
            }

            // Ferme toutes les fenêtres ouvertes
            CloseAllWindows();
            base.OnClosed(e);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == HOTKEY_ID)
                {
                    ToggleOptions();
                    handled = true;
                }
                else if (id == HOTKEY_ID_MINITIMER)
                {
                    ToggleMiniTimer();
                    handled = true;
                }
                else if (id == HOTKEY_ID_RESTORE)
                {
                    ToggleMainWindow();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void OpenOptions()
        {
            if (optionsWindow == null)
            {
                // On crée la fenêtre Options en lui passant les valeurs actuelles
                optionsWindow = new Options(timeOfWork, timeOfRest);

                // On définit le chemin actuel du son dans la fenêtre Options
                optionsWindow.NotificationSound = currentNotificationSound;
                optionsWindow.AudioPathTextBox.Text = currentNotificationSound;

                // Gestion de la fermeture de la fenêtre Options
                optionsWindow.Closed += (sender, args) =>
                {
                    // On récupère les valeurs modifiées
                    timeOfWork = optionsWindow.WorkTime;
                    timeOfRest = optionsWindow.RestTime;
                    currentNotificationSound = optionsWindow.NotificationSound;

                    // On applique directement le son choisi au MediaPlayer
                    player.Open(new Uri(currentNotificationSound, UriKind.RelativeOrAbsolute));

                    // On libère la référence
                    optionsWindow = null;
                };

                // Affichage non modal
                optionsWindow.Show();
            }
            else
            {
                // Si la fenêtre existe déjà, on la ramène au premier plan
                optionsWindow.Activate();
            }
        }

        private void ToggleMiniTimer()
        {
            if (miniTimer == null)
            {
                miniTimer = new MiniTimer();
            }

            if (miniTimer.IsVisible)
            {
                miniTimer.Hide();
            }
            else
            {
                miniTimer.Show();
            }
        }

        private void ToggleOptions()
        {
            if (optionsWindow == null)
            {
                OpenOptions();
            }
            else
            {
                optionsWindow.Close();
                optionsWindow = null;
            }
        }

        private void CloseAllWindows()
        {
            try
            {
                // Ferme le MiniTimer s'il existe
                if (miniTimer != null)
                {
                    miniTimer.Close();
                    miniTimer = null;
                }

                // Ferme Options si ouverte
                if (optionsWindow != null)
                {
                    optionsWindow.Close();
                    optionsWindow = null;
                }

                this.Close(); // Ferme la fenêtre principale
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la fermeture : {ex.Message}");
            }
        }

    }
}