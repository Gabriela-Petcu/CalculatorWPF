using System;

namespace CalculatorWPF.Models
{
    public static class ProgrammerConverter
    {
        //convertire intre baze
        public static string ConvertNumber(string numberStr, int fromBase, int toBase)
        {
            try
            {
                int value = System.Convert.ToInt32(numberStr, fromBase);
                return System.Convert.ToString(value, toBase).ToUpper(); //pt afisarea literelor
            }
            catch (Exception)
            {
                return "Error";
            }
        }
    }
}
