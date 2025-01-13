using ClassLibrary1;
using Microsoft.FSharp.Collections;
using System.Windows;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
using System.Windows.Forms;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        bool helpActive = false;
        public MainWindow()
        {
            
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fsString = lexparser.str2lst(Input.Text);
                var oList = lexparser.lexer(fsString);
                double b = lexparser.parseNeval(oList).Item2;
                Result.Text = b.ToString();
            }
            catch (Exception v)
            {
                if (v == lexparser.divideByZero)
                {
                    Result.Text = "Division by zero error detected";
                }
                else if (v == lexparser.lexError)
                {
                    Result.Text = "Lex error detected";
                }
                else if (v == lexparser.parseError)
                {
                    Result.Text = "Parse error detected";
                }
                else if (v == lexparser.undeclaredVariable)
                {
                    Result.Text = "Undeclared variable error detected";
                }
                else if (v == lexparser.typeMismatch)
                {
                    Result.Text = "Type mismatch error detected";
                }
                else
                {
                    Result.Text = "Unknown Error";
                }
            }

        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            helpActive = !helpActive;
            if (helpActive)
            {
                Info.Text = "Accepted tokens are as follows:\nAll digits (0-9), +, -, * (multiply), / (divide), % (remainder), ^ (to the power of), (), . (for decimal places), sqrt (Square Root), Sin, Cos, Tan, Pi, Log, exp, = (for variable assignment), int, float, and ; (must append on any variable assignment)";
                Help.Content = "Close";
            }
            else
            {
                Info.Text = "";
                Help.Content = "Info";
            }
        }
        private void Graph_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                View = new PlotModel { Title = "Output" };
                var cString = lexparser.str2lst(cInput.Text);
                var cList = lexparser.lexer(cString);
                double yIntercept = lexparser.parseNeval(cList).Item2;
                var gString = lexparser.str2lst(gInput.Text);
                var gList = lexparser.lexer(gString);
                double gradient = lexparser.parseNeval(gList).Item2;
                var pString = lexparser.str2lst(pInput.Text);
                var pList = lexparser.lexer(pString);
                double power = lexparser.parseNeval(pList).Item2;
                Func<double, double> graphFunction = (x) => Math.Pow(x, power)*gradient + yIntercept;
                View.Series.Add(new FunctionSeries(graphFunction, 0, 10, 0.1));
                this.DataContext = this;
            }
            catch (Exception v)
            {
                if (v == lexparser.divideByZero)
                {
                    Result.Text = "Division by zero error detected";
                }
                else if (v == lexparser.lexError)
                {
                    Result.Text = "Lex error detected";
                }
                else if (v == lexparser.parseError)
                {
                    Result.Text = "Parse error detected";
                }
                else if (v == lexparser.undeclaredVariable)
                {
                    Result.Text = "Undeclared variable error detected";
                }
                else if (v == lexparser.typeMismatch)
                {
                    Result.Text = "Type mismatch error detected";
                }
                else
                {
                    Result.Text = "Unknown Error";
                }
            }
        }
        public PlotModel View { get; set; }
    }
}
