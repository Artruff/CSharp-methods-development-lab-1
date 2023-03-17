using System;
using System.IO;
using System.Linq;

namespace CSharp_methods_development_lab_1
{
    class Program
    {
        unsafe static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Введите адрес файла подкачки:");
                string path = Console.ReadLine();

                VirtualArray<int> va = new VirtualArray<int>(path, 10000);

                va[0] = 256;
                va[5000] = 2;
                va[9999] = 1;

                Console.WriteLine("Значение элемента с индексом 0 после записи: " + va[0].ToString());
                Console.WriteLine("Значение элемента с индексом 5000 после записи: " + va[5000].ToString());
                Console.WriteLine("Значение элемента с индексом 9999 после записи: " + va[9999].ToString());

                va.Save();
            }
            catch(Exception e)
            {
                Console.WriteLine("\nПрограмма завершила свою работу с ошибкой: " + e.Message);
            }
        }
    }
}
