using System.Windows;
using System.Windows.Input;

namespace PomodoroApp
{
    public partial class MiniTimer : Window
    {
        public MiniTimer()
        {
            InitializeComponent();
            //Permet de déplacer la fenetre en appelant la méthode MiniTimer_MouseDown
            this.MouseDown += MiniTimer_MouseDown;

            //Positione la fenetre en haut a droite de l'écran
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = SystemParameters.WorkArea.Width - this.Width - 10; // 10 pixels de marge
            this.Top = 10; // 10 pixels de marge en haut
        }

        // Récupère l'event de la souris et déplace la fenêtre si le clic gauche est maintenu
        private void MiniTimer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        // Permet de mettre à jour le chrono depuis MainWindow
        public void UpdateTimer(string time, double angle)
        {
            MiniFrontTimer.Content = time;
            MiniHandRotation.Angle = angle;
        }
    }
}
