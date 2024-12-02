using ClassLibrary1;
using Microsoft.FSharp.Collections;
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
            var fsString = lexparser.str2lst(Input.Text);
            var oList = lexparser.lexer(fsString);
            try
            {
                double b = lexparser.parseNeval(oList).Item2;
                Result.Text = b.ToString();
            }
            catch (Exception v)
            {
                if (v == lexparser.divideByZero)
                {
                    Result.Text = lexparser.divideByZero.ToString();
                }
                else if (v == lexparser.lexError)
                {
                    Result.Text = lexparser.lexError.ToString();
                }
                else if (v == lexparser.parseError)
                {
                    Result.Text = lexparser.parseError.ToString();
                }
                else if (v == lexparser.undeclaredVariable)
                {
                    Result.Text = lexparser.undeclaredVariable.ToString();
                }
                else if (v == lexparser.typeMismatch)
                {
                    Result.Text = lexparser.typeMismatch.ToString();
                }
                else
                {
                    Result.Text = "Unkown Error";
                }
            }
            
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            helpActive = !helpActive;
            if (helpActive)
            {
                Info.Text = "Accepted tokens are as follows:\nAll digits (0-9), +, -, * (multiply), / (divide), % (remainder), ^ (to the power of), (), . (for decimal places), sqrt (Square Root), exp (Exponent) Sin, Cos, Tan, Pi, Log, = (for variable assignment), int, float, and ; (must append on any variable assignment)";
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
