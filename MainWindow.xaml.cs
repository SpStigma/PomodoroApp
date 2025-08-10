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
using System.Windows.Threading;
using System.Media;
using System.Numerics;
using System.Diagnostics;

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
        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(1);
            //Call the methode
            timer.Tick += timer_Tick!;
            frontTimer.Content = formatTime(time);

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

        private void timer_Tick(object sender, EventArgs e)
        {
            time++;
            frontTimer.Content = formatTime(time);

            double angle = (time % 60) * (360 / 60);
            HandRotation.Angle = angle;

            //2500
            if (isWorkCycle && time == 3)
            {
                timer.Stop();
                PlayNotificationSound();
                new AutoCloseMessage("Temps de Repos Unlock", 3).Show();
                SwitchCycle();
            }
            if (numberOfPomodoro != 4)
            {
                //300
                if (!isWorkCycle && time == 3)
                {
                    timer.Stop();
                    PlayNotificationSound();
                    new AutoCloseMessage("Au travail", 3).Show();
                    SwitchCycle();
                }
                else
                {
                    if (!isWorkCycle && time == 2500)
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
            frontTimer.Content = formatTime(time);
            ChangeTextButton();
            timer.Start();
        }

        private string formatTime(int global)
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
                MessageBox.Show($"Erreur lecture son : {ex.Message}");
            }
        }
    }
}