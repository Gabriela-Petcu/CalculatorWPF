using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CalculatorWPF.ViewModels;
using CalculatorWPF.Views;

namespace CalculatorWPF
{
    public partial class MainWindow : Window
    {
        private CalculatorViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // dupa ce se incarca fereastra, se obtine datacontextul din xaml
            Loaded += (_, _) =>
            {
                viewModel = DataContext as CalculatorViewModel;

                if (viewModel == null)
                {
                    MessageBox.Show("ViewModel-ul nu a fost setat corect!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                //inregistreaza un eveniment din tastatura
                this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            };
        }


        //se apel cand un buton de cifra este apasat=>inputDigit pt a se adauga in Display
        private void OnDigit(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                viewModel.InputDigit(button.Content.ToString());
            }
        }

        //apel cand un operator este apasat, se convertesc simbolurile
        private void OnOperator(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string op = button.Content.ToString();
                if (op == "×") op = "*";
                if (op == "÷") op = "/";
                if (op == "−") op = "-";
                viewModel.ApplyOperator(op);
            }
        }

        //la apel = se trimite in Evaluate
        private void OnEquals(object sender, RoutedEventArgs e)
        {
            viewModel.Evaluate();
        }

        //cand se apasa C se apel ClearAll
        private void OnClear(object sender, RoutedEventArgs e)
        {
            viewModel.ClearAll();
        }

        //cand se apasa backspace se elim ultimul caracter
        private void OnBackspace(object sender, RoutedEventArgs e)
        {
            viewModel.Backspace();
        }

        //afiseaza fereastra de tip About
        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this
            };
            aboutWindow.ShowDialog();
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (viewModel == null)
                return;

            // cifre
            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                string digit = (e.Key - Key.D0).ToString();
                viewModel.InputDigit(digit);
                e.Handled = true;
            }
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                string digit = (e.Key - Key.NumPad0).ToString();
                viewModel.InputDigit(digit);
                e.Handled = true;
            }
            // virgula/punct
            else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                viewModel.InputDigit(".");
                e.Handled = true;
            }
            // operatorii de baza
            else if (e.Key == Key.Add || e.Key == Key.OemPlus && Keyboard.Modifiers == ModifierKeys.None)
            {
                viewModel.ApplyOperator("+");
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                viewModel.ApplyOperator("-");
                e.Handled = true;
            }
            else if (e.Key == Key.Multiply)
            {
                viewModel.ApplyOperator("*");
                e.Handled = true;
            }
            else if (e.Key == Key.Divide || e.Key == Key.Oem2) // slash
            {
                viewModel.ApplyOperator("/");
                e.Handled = true;
            }
            // enter sau egal
            else if (e.Key == Key.Enter || (e.Key == Key.OemPlus && Keyboard.Modifiers == ModifierKeys.Shift))
            {
                viewModel.Evaluate();
                e.Handled = true;
            }
            // backspace
            else if (e.Key == Key.Back)
            {
                viewModel.Backspace();
                e.Handled = true;
            }
            // ESC = Clear All
            else if (e.Key == Key.Escape)
            {
                viewModel.ClearAll();
                e.Handled = true;
            }
        }

        //handler pt evenimentul de click pe butoanele de baza, cand utilizatorul apasa un buton
        //pt schimbarea bazei numerice, metoda preia baza selectata si converteste valoarea afisata din display-ul curent
        //intr o alta baza numerica, afisand rezultatul
        private void BaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CalculatorViewModel viewModel &&
                sender is Button button &&
                button.Tag is string baseStr &&
                int.TryParse(baseStr, out int newBase))
            {
                if (viewModel.IsProgrammerMode)
                {
                    try
                    {
                        int valueInDecimal = Convert.ToInt32(viewModel.Display, viewModel.CurrentBase == 10 ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture);
                        viewModel.CurrentBase = newBase;
                        viewModel.Display = Convert.ToString(valueInDecimal, newBase).ToUpper();
                    }
                    catch
                    {
                        viewModel.Display = "Error";
                    }
                }
            }
        }


    }
}
