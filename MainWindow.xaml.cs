using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Diagnostics;
using Forms = System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Interop;

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
        public int timeOfWork = 10;
        public int timeOfRest = 10;
        private Forms.NotifyIcon nIcon = new();
        private ContextMenuStrip contextMenu;
        private MiniTimer miniTimer;
        private Options optionsWindow;

        //Hotkey
        private const int HOTKEY_ID = 1;
        private const int HOTKEY_ID_MINITIMER = 2;
        private const uint MOD_CONTROL = 0x0002; // Control key
        private const uint VK_I = 0x49; // 'I' key
        private const uint VK_H = 0x48; // 'H' key
        private const int WM_HOTKEY = 0x0312;
        private HwndSource _source;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public MainWindow()
        {
            InitializeComponent();
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
                player.Open(new Uri("Sounds/notif.mp3", UriKind.Relative));
                player.Play();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lecture son : {ex.Message}");
            }
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
        }

        protected override void OnClosed(EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
            UnregisterHotKey(handle, HOTKEY_ID_MINITIMER);
            _source.RemoveHook(HwndHook);

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
                    OpenOptions();
                    handled = true;
                }
                else if (id == HOTKEY_ID_MINITIMER)
                {
                    ToggleMiniTimer();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void OpenOptions()
        {
            if (optionsWindow == null)
            {
                optionsWindow = new Options(timeOfWork, timeOfRest);
                optionsWindow.Closed += (sender, args) =>
                {
                    timeOfWork = optionsWindow.WorkTime;
                    timeOfRest = optionsWindow.RestTime;
                    optionsWindow = null;
                };
                optionsWindow.Show();
            }
            else
            {
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