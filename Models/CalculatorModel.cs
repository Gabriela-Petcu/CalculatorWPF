using System;

namespace CalculatorWPF.Models
{
    public class CalculatorModel
    {
        //val din stanga
        public double CurrentValue { get; set; } = 0;
        //stocheaza operatorul
        public string Operator { get; set; }

        //comanda Clear
        public void Clear()
        {
            CurrentValue = 0;
            Operator = null;
        }

        public double ApplyOperation(double input, string op)
        {
            return op switch
            {
                "+" => CurrentValue + input,
                "-" => CurrentValue - input,
                "*" => CurrentValue * input,
                "/" => input == 0 ? double.NaN : CurrentValue / input,
                "%" => CurrentValue % input,
                _ => input
            };
        }
    }
}
