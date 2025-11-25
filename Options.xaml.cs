using System.Windows;

namespace PomodoroApp
{
    public partial class Options : Window
    {
        public int WorkTime { get; set; }
        public int RestTime { get; set; }
        public event Action OptionsSaved;
        public string NotificationSound { get; set; } = "Sounds/notif.mp3";

        public Options(int currentWorkTime, int currentRestTime)
        {
            InitializeComponent();

            WorkMinutesTextBox.Text = (currentWorkTime / 60).ToString();
            WorkSecondsTextBox.Text = (currentWorkTime % 60).ToString();
            RestMinutesTextBox.Text = (currentRestTime / 60).ToString();
            RestSecondsTextBox.Text = (currentRestTime % 60).ToString();
            AudioPathTextBox.Text = NotificationSound;
        }

        private void ChoisirAudio_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Fichiers audio (*.mp3;*.wav)|*.mp3;*.wav";
            if (openFileDialog.ShowDialog() == true)
            {
                NotificationSound = openFileDialog.FileName;
                AudioPathTextBox.Text = NotificationSound;
            }
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            int wMin, wSec, rMin, rSec;

            //Check l'input de l'uilisateur pour voir si c'est un nombre
            if (!int.TryParse(WorkMinutesTextBox.Text, out wMin))
                return;

            if (!int.TryParse(WorkSecondsTextBox.Text, out wSec))
                return;

            if (!int.TryParse(RestMinutesTextBox.Text, out rMin))
                return;

            if (!int.TryParse(RestSecondsTextBox.Text, out rSec))
                return;

            if (wMin < 0 || wSec < 0 || rMin < 0 || rSec < 0)
                return;

            WorkTime = wMin * 60 + wSec;
            RestTime = rMin * 60 + rSec;

            OptionsSaved?.Invoke();
            Close();
        }
    }
}
