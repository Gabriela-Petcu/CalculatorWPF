using System;
using System.Windows.Input;

namespace CalculatorWPF.ViewModels
{
 
    // Comanda generica pt legatura ViewModel - View
   
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;

        //va fi executata cand comanda este chemata, param T,decide daca coamnda poate fi executata in functie de param
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        //verif daca poate fi executata apel fct canE cu param T
        public bool CanExecute(object parameter) =>
            canExecute == null || canExecute((T)parameter);

        //executa actiunea
        public void Execute(object parameter) =>
            execute((T)parameter);

        //notifica aplicatia cand se schimba conditiile
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }


    //fix la fel ca mai sus doar ca este o comanda non-generica, folosita atunci cand nu este necesar un param de tip specific pt actiune
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter) =>
            canExecute == null || canExecute(parameter);

        public void Execute(object parameter) =>
            execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
