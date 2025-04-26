using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CalculatorWPF.Models;
using CalculatorWPF.Views;

namespace CalculatorWPF.ViewModels
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        //se actualizeaza interfata la fiecare miscare
        private string display;
        public string Display
        {
            get => display;
            set { display = value; OnPropertyChanged(); } //se trim semnal spre xaml sa actualizeze ce se vede
        }

        //model pt operatii matematice
        private readonly CalculatorModel model = new();

        //model pt formatare
        private readonly CultureInfo customCulture;

        //model pt gestionarea functiilor de memorie
        private readonly MemoryManager memory = new();

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CalculatorViewModel()
            //la pornirea app se seteaza valorile salvate anterior, se pregateste comanda pt operatii
        {
            Display = "0"; //output=0
            //se citesc optiunile salvate inainte
            IsDigitGroupingEnabled = Properties.Settings.Default.IsDigitGrouping;
            CurrentBase = Properties.Settings.Default.LastBase;
            Mode = Properties.Settings.Default.LastMode;
            RespectOrderOfOperations = Properties.Settings.Default.RespectOrderOfOperations;
            customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            customCulture.NumberFormat.NumberGroupSeparator = ",";
            customCulture.NumberFormat.NumberGroupSizes = new[] { 3 };

            //se leaga comenzile
            SingleOperationCommand = new RelayCommand<string>(ApplySingleOperation);
            
            //se formateaza afisarea
            FormatDisplay();

        }

        private string currentInput = "";
        private List<string> expression = new();
        private bool justEvaluated = false;
        private bool inputStarted = false;
        private bool lastInputWasOperator = false;

        //verif daca cifra este valida pt baza curenta
        public void InputDigit(string digit)
        {
            var separator = customCulture.NumberFormat.NumberDecimalSeparator;

            // verif daca cifra este valida in baza curenta
            if (!IsValidDigitForBase(digit) && digit != separator)
            {
                Display = "Caracter invalid";
                return;
            }

            //daca suntem in Programmer Mode
            if (CurrentBase != 10)
            {
                if (Display == "0" || justEvaluated || lastInputWasOperator)
                {
                    currentInput = digit.ToUpper();
                    Display = currentInput;
                }
                else
                {
                    currentInput += digit.ToUpper();
                    Display = currentInput;
                }

                justEvaluated = false;
                inputStarted = true;
                lastInputWasOperator = false;
                return;
            }


            //daca digitul e separatorul zecimal
            if (digit == separator)
            {
                if (currentInput.Contains(separator))
                    return;

                if (string.IsNullOrEmpty(currentInput))
                    currentInput = "0" + separator;
                else
                    currentInput += separator;

                Display = currentInput;
                FormatDisplay();
                return;
            }

            // daca e prima cifra dupa o eval sau dupa un operator
            if (Display == "0" || justEvaluated || lastInputWasOperator)
            {
                currentInput = digit;
                Display = digit;
            }
            else
            {
                currentInput += digit;
                Display += digit;
            }

            FormatDisplay();

            justEvaluated = false;
            inputStarted = true;
            lastInputWasOperator = false;
        }



        //transf cifra in litera mare
        private bool IsValidDigitForBase(string digit)
        {
            digit = digit.ToUpper();
            return CurrentBase switch
            {
                2 => digit is "0" or "1",
                8 => "01234567".Contains(digit),
                10 => "0123456789".Contains(digit),
                16 => "0123456789ABCDEF".Contains(digit),
                _ => false
            };
        }

        //sterge tot
        public void ClearAll()
        {
            model.Clear();
            Display = "0";
            currentInput = "";
            expression.Clear();
            justEvaluated = false;
            inputStarted = false;
        }

        //sterge doar ultima valoare introdusa
        public void ClearEntry()
        {
            Display = "0";
            currentInput = "";
            inputStarted = false;
        }

        //sterge ultimul caracter din currentInput
        public void Backspace()
        {
            // curatam orice separator de grupare din currentInput
            currentInput = currentInput.Replace(customCulture.NumberFormat.NumberGroupSeparator, "");

            if (currentInput.Length > 0)
                currentInput = currentInput[..^1];

            Display = currentInput.Length > 0 ? currentInput : "0";

            FormatDisplay();
        }


        //schimba semnul pt operatorul +/-
        public void ToggleSign()
        {
            try
            {
                if (CurrentBase == 10)
                {
                    string clean = Display.Replace(customCulture.NumberFormat.NumberGroupSeparator, "");
                    double value = double.Parse(clean, customCulture);
                    double negated = -value;

                    Display = FormatResult(negated);
                    currentInput = Display;
                }
                else
                {
                    int value = Convert.ToInt32(Display, CurrentBase);
                    int negated = -value;

                    Display = FormatResult(negated);
                    currentInput = Display;
                }
            }
            catch
            {
                Display = "Error";
            }
        }



        public void ApplyOperator(string op)
        {
            try
            {
                double input = CurrentBase == 10
            ? double.Parse(Display.Replace(customCulture.NumberFormat.NumberGroupSeparator, ""), customCulture)
            : Convert.ToInt32(Display, CurrentBase);

                if (RespectOrderOfOperations) //verif daca este activata opt de respectare a ordinii op
                {
                    expression.Add(Display); //adauga val curenta la expresia in curs
                    expression.Add(op); //adaug op
                }
                else
                {
                    if (model.Operator != null) //daca exista un op activ
                    { 
                        //aplic op curent pe valoarea curenta
                        double result = model.Operator switch
                        {
                            "+" => model.CurrentValue + input,
                            "-" => model.CurrentValue - input,
                            "*" => model.CurrentValue * input,
                            "/" => input != 0 ? model.CurrentValue / input : double.NaN, //verif impartirea la 0
                            _ => input
                        };
                        model.CurrentValue = result;
                        Display = FormatResult(result); //formateaza si actaulzieaza
                    }
                    else
                    {
                        model.CurrentValue = input; //daca nu exista un op, set valoarea ca valoare curenta
                    }
                    model.Operator = op; //seteaza op pt urm operatii
                }

                justEvaluated = true;
                inputStarted = false;
                lastInputWasOperator = true;
            }
            catch
            {
                Display = "Error";
            }
        }




        public void Evaluate()
        {
            try
            {
                double input = CurrentBase == 10
            ? double.Parse(Display.Replace(customCulture.NumberFormat.NumberGroupSeparator, ""), customCulture)
            : Convert.ToInt32(Display, CurrentBase); //val curenta

                if (RespectOrderOfOperations) //daca se respecta ordinea
                {
                    expression.Add(input.ToString()); //adauga valoarea curenta si incepe evaluarea expresiei
                    var postfix = ConvertToPostfix(expression); //transf expresia infix in postfix
                    double result = EvaluatePostfix(postfix); //evalueaza expresia postfix
                    Display = FormatResult(result); //afiseaza rez
                    expression.Clear(); //sterge expresia in curs
                }
                else if (model.Operator != null) //daca nu respecta ordinea, aplic op direct
                { 
                    double result = model.Operator switch
                    {
                        "+" => model.CurrentValue + input,
                        "-" => model.CurrentValue - input,
                        "*" => model.CurrentValue * input,
                        "/" => input != 0 ? model.CurrentValue / input : double.NaN,
                        _ => input
                    };
                    model.CurrentValue = result;
                    Display = FormatResult(result); //rezultatul
                    model.Operator = null;
                }

                justEvaluated = true;
                inputStarted = false;
                lastInputWasOperator = false;
            }
            catch
            {
                Display = "Error";
            }
        }




        private void ApplySingleOperation(string operation)
        {
            try
            {
                string clean = Display.Replace(customCulture.NumberFormat.NumberGroupSeparator, "");
                double value = double.Parse(clean, customCulture); // folosim  personalizrea
                double result;

                switch (operation)
                {
                    case "x²":
                        result = Math.Pow(value, 2); //ridicare la patrat
                        break;
                    case "√":
                        result = value >= 0 ? Math.Sqrt(value) : double.NaN; //radacina
                        break;
                    case "1/x":
                        result = value != 0 ? 1.0 / value : double.NaN; //inversul
                        break;
                    case "%":
                        if (model.Operator != null && operation!=null)
                        {
                            value = model.CurrentValue * value / 100.0; //procentuk
                            Display = FormatResult(value);
                            currentInput = Display;         // actualizeaz currentInput
                            inputStarted = true;            // marcheaza ca am început input nou
                            justEvaluated = false;
                            return;
                        }
                        else
                        {
                            result = 0 ;
                            break;
                        }


                    default:
                        result = value; //daca nu se pot aplica operatiile return val curenta
                        break;
                }

                Display = double.IsNaN(result) //daca nu e nr valid
                    ? "Error"
                    : FormatResult(result);
            }
            catch
            {
                Display = "Error";
            }
        }


        //converteste val curenta de pe display in double
        public void MemoryStore()
        {
            if (double.TryParse(Display, out double val))
                memory.Store(val);
        }

        //recupereaza val stocata in memoria interna 
        public void MemoryRecall()
        {
            Display = memory.Recall().ToString();
            currentInput = Display;
            inputStarted = false;
        }

        //adauga val curenta de pe display in memoria interna
        public void MemoryAdd()
        {
            if (double.TryParse(Display, out double val))
                memory.AddToMemory(val);
        }

        //scade val curenta de pe displya din valoarea stocata in mem interna
        public void MemorySubtract()
        {
            if (double.TryParse(Display, out double val))
                memory.SubtractFromMemory(val);
        }

        //goleste mem interna
        public void MemoryClear()
        {
            memory.ClearMemory();
        }

        //return valor stocate in memorie
        public List<double> GetMemoryList()
        {
            return memory.GetMemoryValues();
        }

        private void FormatDisplay()
        {
            // nu aplicam digit grouping in mod Programmer
            if (CurrentBase != 10)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(Display))
                {
                    Display = "0";
                    return;
                }

                string clean = Display.Replace(customCulture.NumberFormat.NumberGroupSeparator, "");
                bool endsWithDecimal = clean.EndsWith(customCulture.NumberFormat.NumberDecimalSeparator);

                if (double.TryParse(clean, NumberStyles.Any, customCulture, out double number))
                {
                    if (IsDigitGroupingEnabled)
                    {
                        Display = number % 1 == 0
                            ? number.ToString("#,0", customCulture)
                            : number.ToString("#,0.################", customCulture);
                    }
                    else
                    {
                        Display = number.ToString("0.################", customCulture);
                    }

                    // daca utilizatorul a introdus separatorul zecimal il pastram
                    if (endsWithDecimal)
                        Display += customCulture.NumberFormat.NumberDecimalSeparator;
                }
                else
                {
                    if (!endsWithDecimal)
                        Display = "Error";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare în FormatDisplay:\n" + ex.Message);
            }
        }



        private string FormatResult(double result)
        {
            if (CurrentBase == 10)
            {
                return IsDigitGroupingEnabled
                    ? (result % 1 == 0
                        ? result.ToString("#,0", customCulture)
                        : result.ToString("#,0.################", customCulture))
                    : result.ToString("0.################", customCulture);
            }
            else
            {
                int val = (int)result;
                if (val >= 0)
                    return Convert.ToString(val, CurrentBase).ToUpper();

                // convertim valoarea negativa in reprezentare pe 32 biti (complement fata de 2)
                uint uval = (uint)val;

                return CurrentBase switch
                {
                    2 => Convert.ToString(uval, 2),
                    8 => Convert.ToString(uval, 8),
                    16 => uval.ToString("X"),
                    _ => "Error"
                };
            }
        }




        private List<string> ConvertToPostfix(List<string> infix)
        {
            var output = new List<string>();
            var operators = new Stack<string>();

            Dictionary<string, int> precedence = new()
            {
                { "+", 1 }, { "-", 1 },
                { "*", 2 }, { "/", 2 }
            };

            foreach (var token in infix)
            {
                if (double.TryParse(token, out _))
                {
                    output.Add(token); //daca este nr il adaugam la lista de output
                }
                else
                {
                    while (operators.Count > 0 && precedence.ContainsKey(token) &&
                           precedence[operators.Peek()] >= precedence[token])
                    {
                        output.Add(operators.Pop()); //daca op are prioritate mai mica sau egala se muta in output
                    }
                    operators.Push(token);  //adaugam op pe stiva
                }
            }

            while (operators.Count > 0)
            {
                output.Add(operators.Pop()); //op ramasi se muta in output
            }

            return output;
        }

        private double EvaluatePostfix(List<string> postfix)
        {
            var stack = new Stack<double>();

            foreach (var token in postfix)
            {
                if (double.TryParse(token, out double number))
                {
                    stack.Push(number); //adauga nr pe stiva
                }
                else
                {
                    double b = stack.Pop();
                    double a = stack.Pop();

                    double result = token switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => b != 0 ? a / b : double.NaN, //imp la 0
                        _ => throw new InvalidOperationException("Unknown operator")
                    };

                    stack.Push(result); //push rez inapoi pe stiva
                }
            }

            return stack.Pop();
        }

        private int currentBase = 10;
        public int CurrentBase
        {
            get => currentBase;
            set
            {
                if (currentBase != value)
                {
                    currentBase = value;
                    OnPropertyChanged();
                }
            }
        }

        //permite activarea sau dezactivarea optiunii de grupare a cifrelor
        private bool isDigitGroupingEnabled = true;
        public bool IsDigitGroupingEnabled
        {
            get => isDigitGroupingEnabled;
            set
            {
                isDigitGroupingEnabled = value;
                OnPropertyChanged();

                // Doar dacă cultura e deja setată putem apela FormatDisplay
                if (customCulture != null)
                    FormatDisplay();

                Properties.Settings.Default.IsDigitGrouping = value;
                Properties.Settings.Default.Save(); //salv pt utilizari urm
            }
        }

        private bool respectOrderOfOperations = Properties.Settings.Default.RespectOrderOfOperations;
        public bool RespectOrderOfOperations
        {
            get => respectOrderOfOperations;
            set
            {
                respectOrderOfOperations = value;
                OnPropertyChanged();
                Properties.Settings.Default.RespectOrderOfOperations = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool IsStandardMode => Mode == "Standard";
        public bool IsProgrammerMode => Mode == "Programmer";

        private string mode;
        public string Mode
        {
            get => mode;
            set
            {
                if (mode != value)
                {
                    mode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsStandardMode));
                    OnPropertyChanged(nameof(IsProgrammerMode));

                    // cand intram in Standard, revenim la baza 10
                    if (mode == "Standard")
                    {
                        try
                        {
                            // convertim valoarea afisata la zecimal, apoi o reafisam in baza 10
                            int valueInDecimal = Convert.ToInt32(Display, CurrentBase);
                            CurrentBase = 10;
                            Display = valueInDecimal.ToString();
                        }
                        catch
                        {
                            Display = "Error";
                        }
                    }

                    Properties.Settings.Default.LastMode = value;
                    Properties.Settings.Default.Save();
                }
            }
        }


        //permite schimbarea bazei de numeratie in programmer
        public ICommand SetBaseCommand => new RelayCommand(param =>
        {
            if (!IsProgrammerMode) return;

            if (param is string baseStr && int.TryParse(baseStr, out int newBase))
            {
                try
                {
                    // scoatem separatorii pt a evita parse eronat
                    string clean = Display.Replace(customCulture.NumberFormat.NumberGroupSeparator, "");

                    // convertim din baza curenta în baza 10 (int implicit)
                    int decimalValue = Convert.ToInt32(clean, CurrentBase);

                    CurrentBase = newBase;
                    Properties.Settings.Default.LastBase = newBase;
                    Properties.Settings.Default.Save();

                    // Afișăm în noua bază
                    if (newBase == 10)
                    {
                        Display = decimalValue.ToString();
                    }
                    else if (decimalValue >= 0)
                    {
                        Display = Convert.ToString(decimalValue, newBase).ToUpper();
                    }
                    else
                    {
                        uint unsigned = (uint)decimalValue;
                        Display = newBase switch
                        {
                            2 => Convert.ToString(unsigned, 2),
                            8 => Convert.ToString(unsigned, 8),
                            16 => unsigned.ToString("X"),
                            _ => "Error"
                        };
                    }
                }
                catch
                {
                    Display = "Error";
                }
            }
        });


        //adauga o cifra la input ul curent
        public ICommand AddDigitCommand => new RelayCommand(param =>
        {
            if (param is string digit)
                InputDigit(digit);
        });

        //seteaza op matematic
        public ICommand SetOperationCommand => new RelayCommand(param =>
        {
            if (param is string op)
                ApplyOperator(op);
        });

        //calc rezul
        public ICommand CalculateCommand => new RelayCommand(_ => Evaluate());
        
        //sterge ultima val introdusa
        public ICommand ClearEntryCommand => new RelayCommand(_ => ClearEntry());
       
        //sterge tot istoricul calc
        public ICommand ClearAllCommand => new RelayCommand(_ => ClearAll());
        
        //sterge un caracter
        public ICommand BackspaceCommand => new RelayCommand(_ => Backspace());
        
        //schimba semnul
        public ICommand ToggleSignCommand => new RelayCommand(_ => ToggleSign());
        
        //aplica operatiuni
        public ICommand SingleOperationCommand { get; }

        //copierea continutului in clipboard, goleste display
        public ICommand CutCommand => new RelayCommand(_ =>
        {
            Clipboard.SetText(Display);
            Display = "0";
            currentInput = "";
            inputStarted = false;
        });

        //copierea continutului in clipboard
        public ICommand CopyCommand => new RelayCommand(_ => Clipboard.SetText(Display));

        //lipeste textul din clipboard in display
        public ICommand PasteCommand => new RelayCommand(_ =>
        {
            if (Clipboard.ContainsText())
            {
                string pasted = Clipboard.GetText().Trim().ToUpper();

                try
                {
                    int value = Convert.ToInt32(pasted, CurrentBase);
                    Display = Convert.ToString(value, CurrentBase).ToUpper();
                    currentInput = Display;
                    inputStarted = true;
                }
                catch
                {
                    Display = "Error";
                }
            }

        });

        //schimba modurile
        public ICommand SetModeCommand => new RelayCommand(param =>
        {
            if (param is string newMode)
            {
                Mode = newMode;
                Properties.Settings.Default.LastMode = newMode;
                Properties.Settings.Default.Save();
            }
        });

        //fereastra cu mesajele
        public ICommand ShowAboutCommand => new RelayCommand(_ =>
        {
            string message = "MyCalc - Calculator WPF\nRealizat de: Petcu Gabriela\nGrupa: 10LF233";
            MessageBox.Show(message, "Despre aplicație", MessageBoxButton.OK, MessageBoxImage.Information);
        });

        //comenzi pt memorie
        public ICommand MemoryClearCommand => new RelayCommand(_ => MemoryClear());
        public ICommand MemoryRecallCommand => new RelayCommand(_ => MemoryRecall());
        public ICommand MemoryAddCommand => new RelayCommand(_ => MemoryAdd());
        public ICommand MemorySubtractCommand => new RelayCommand(_ => MemorySubtract());
        public ICommand MemoryStoreCommand => new RelayCommand(_ => MemoryStore());

        //lista cu valorile din memorie
        public ICommand ShowMemoryListCommand => new RelayCommand(_ =>
        {
            var memoryValues = memory.GetMemoryValues();
            var window = new MemoryListWindow(memoryValues);
            if (window.ShowDialog() == true && window.SelectedValue.HasValue)
            {
                Display = window.SelectedValue.Value.ToString();
                currentInput = Display;
                inputStarted = false;
            }
        });
    }
}
