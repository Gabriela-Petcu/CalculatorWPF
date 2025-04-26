using System.Collections.Generic;
using System.Linq;

namespace CalculatorWPF.Models
{
    public class MemoryManager
    {
        //stiva pt memorarea val salvate
        private readonly Stack<double> memoryStack = new();

        //comanda MC=sterge complet
        public void ClearMemory()
        {
            memoryStack.Clear();
        }

        //comanda MS=salveaza o noua val in vf stivei
        public void Store(double value)
        {
            memoryStack.Push(value);
        }

        //comanda M+=adauga val curenta la val salvata in memorie
        public void AddToMemory(double value)
        {
            if (memoryStack.Count > 0)
            {
                double top = memoryStack.Pop();
                memoryStack.Push(top + value);
            }
            else
            {
                memoryStack.Push(value);
            }
        }

        //comanda M-=scade val din memorie
        public void SubtractFromMemory(double value)
        {
            if (memoryStack.Count > 0)
            {
                double top = memoryStack.Pop();
                memoryStack.Push(top - value);
            }
            else
            {
                memoryStack.Push(-value);
            }
        }

        //comanda MR=afiseaza ultima val din memorie fara stergere
        public double Recall()
        {
            return memoryStack.Count > 0 ? memoryStack.Peek() : 0;
        }

        //comanda M>=lista completa a valorilor din memorie
        public List<double> GetMemoryValues()
        {
            return memoryStack.Reverse().ToList(); // pentru a le arăta în ordine de adăugare
        }

    }
}
