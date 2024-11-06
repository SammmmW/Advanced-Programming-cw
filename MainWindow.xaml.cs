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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            //Error.Text = "I am an error";
            Info.Text = "Accepted tokens are as follows:\nAll digits (1-9), +, -, * (multiply), /(divide), % (remainder), ^(to the power of), (), . (for decimal places), E (exponent), Sin, Cos, Tan, Pi, Log";
        }
    }
}