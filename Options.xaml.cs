using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PomodoroApp
{
    /// <summary>
    /// Logique d'interaction pour Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public int WorkTime { get; set; }
        public int RestTime { get; set; }
        public Options(int currentWorkTime, int currentRestTime)
        {
            InitializeComponent();

            WorkTimeTextBox.Text = currentWorkTime.ToString();
            RestTimeTextBox.Text = currentRestTime.ToString();
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            int work, rest;

            bool parseWork = int.TryParse(WorkTimeTextBox.Text, out work);
            bool parseRest = int.TryParse(RestTimeTextBox.Text, out rest);

            if (!parseWork || !parseRest || work <= 0 || rest <= 0)
            {
                return;
            }

            WorkTime = work;
            RestTime = rest;

            DialogResult = true;

            Close();
        }

        public void ForceClose()
        {
            this.Close();
        }
    }
}
