using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PomodoroApp
{
    public partial class MiniTimer : Window
    {
        public MiniTimer()
        {
            InitializeComponent();
            DrawClockFace();
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

        private void DrawClockFace()
        {
            // Centre et Rayon du cadran (basé sur la taille 200x200 définie dans le XAML)
            double centerX = 100;
            double centerY = 100;
            double radius = 100;

            // On boucle de 0 à 59 (les 60 secondes)
            for (int i = 0; i < 60; i++)
            {
                // Calcul de l'angle (6 degrés par seconde)
                // On retire 90 degrés pour commencer à midi (en haut) au lieu de 3h (à droite)
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
        // Permet de mettre à jour le chrono depuis MainWindow
        public void UpdateTimer(string time, double angle)
        {
            MiniFrontTimer.Content = time;
            MiniHandRotation.Angle = angle;
        }
    }
}
