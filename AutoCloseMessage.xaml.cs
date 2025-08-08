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
using System.Windows.Threading;


namespace PomodoroApp
{
    /// <summary>
    /// Interaction logic for AutoCloseMessage.xaml
    /// </summary>
    public partial class AutoCloseMessage : Window
    {
        // peut rajouté arguments comme le message et la durée
        public AutoCloseMessage(string message, int duration)
        {
            InitializeComponent();
            //referencce a mon front Label
            MessageText.Text = message;
            //crée le timer avec la classe DispatcherTimer de Windows.Threading apres reglé l'intervalle
            DispatcherTimer closeTimer = new DispatcherTimer();
            closeTimer.Interval = TimeSpan.FromSeconds(duration);

            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                this.Close();
            };
            closeTimer.Start();
        }
    }
}
