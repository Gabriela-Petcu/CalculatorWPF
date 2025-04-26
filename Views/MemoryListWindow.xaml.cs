using System.Collections.Generic;
using System.Windows;

namespace CalculatorWPF.Views
{
    public partial class MemoryListWindow : Window
    {
        // val selectat din lista, transmisa inapoi catre ViewModel
        public double? SelectedValue { get; private set; }

        // primeste lista de valori din memorie
        public MemoryListWindow(List<double> values)
        {
            InitializeComponent();
            MemoryListBox.ItemsSource = values;
        }

        // cand se apasa butonul "Use Selected"
        private void UseButton_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedValue();
        }

        // dublu-click pe o valoare din lista
        private void MemoryListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetSelectedValue();
        }

        // seteaza valoarea aleasa si inchide fereastra
        private void SetSelectedValue()
        {
            if (MemoryListBox.SelectedItem is double value)
            {
                SelectedValue = value;
                DialogResult = true;
                Close();
            }
        }
    }
}
