using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CalculatorWPF.Converters
{
    //in functie de baza selectata controleaza vizibilitatea unor functii 
    public class HexBaseToVisibilityConverter : IValueConverter
    {
        //trans val din VM in Visibility pt interfata
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //daca val este 16 se afiseaza altfel se ascunde complet
            return (int)value == 16 ? Visibility.Visible : Visibility.Collapsed;
        }
        //nu e folosit, se arunca exceptie daca e apelat pt ca bindingul este doar din ViewModel spre View
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
