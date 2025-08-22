using System.Windows;

namespace PomodoroApp
{
    public partial class MiniTimer : Window
    {
        public MiniTimer()
        {
            InitializeComponent();
        }

        // Permet de mettre à jour le chrono depuis MainWindow
        public void UpdateTimer(string time, double angle)
        {
            MiniFrontTimer.Content = time;
            MiniHandRotation.Angle = angle;
        }
    }
}
