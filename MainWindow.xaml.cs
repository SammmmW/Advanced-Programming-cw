using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        bool helpActive = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Result.Text = Input.Text; //in the final program, what this needs to actually do is pass 'Input.Text' into the f# program, then display that result
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            helpActive = !helpActive;
            if (helpActive)
            {
                Info.Text = "Accepted tokens are as follows:\nAll digits (0-9), +, -, * (multiply), / (divide), % (remainder), ^ (to the power of), (), . (for decimal places), E (exponent), sqrt (Square Root), Sin, Cos, Tan, Pi, Log";
                Help.Content = "Close";
            }
            else
            {
                Info.Text = "";
                Help.Content = "Info";
            }
        }
    }
}