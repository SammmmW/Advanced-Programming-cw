using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool helpActive = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Result.Text = "Hello!";
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            helpActive = !helpActive;
            if (helpActive)
            {
                Info.Text = "Accepted tokens are as follows:\nAll digits (1-9), +, -, * (multiply), /(divide), % (remainder), ^(to the power of), (), . (for decimal places), E (exponent), Sin, Cos, Tan, Pi, Log";
            }
            else
            {
                Info.Text = "";
            }
        }
    }
}
