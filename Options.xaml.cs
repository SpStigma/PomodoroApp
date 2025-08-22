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

            WorkTimeTextBox.Text = currentWorkTime.ToString();
            RestTimeTextBox.Text = currentRestTime.ToString();
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
            int work, rest;

            if (!int.TryParse(WorkTimeTextBox.Text, out work) || !int.TryParse(RestTimeTextBox.Text, out rest) || work <= 0 || rest <= 0)
                return;

            WorkTime = work;
            RestTime = rest;

            OptionsSaved?.Invoke(); // notifie MainWindow
            Close();
        }
    }
}
