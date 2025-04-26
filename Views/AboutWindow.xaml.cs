using System.Windows;

namespace CalculatorWPF.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        //evenimentul pt butonul Ok care inchide fereastra 
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            //inchide
            Close();
        }
    }
}
