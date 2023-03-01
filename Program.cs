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
                Random r = new Random();

                VirtualArray<byte> va = new VirtualArray<byte>(path, 10000);

                Console.Write("Значение элемента с индексом 0 до записи: ");
                Console.WriteLine(va[0]);
                va[0] = (byte)r.Next(1, 20);
                Console.Write("Новое значение элемента с индексом 0: ");
                Console.WriteLine(va[0]);

                Console.Write("\nЗначение элемента с индексом 1 до записи: ");
                Console.WriteLine(va[1]);
                va[1] = (byte)r.Next(1, 20);
                Console.Write("Новое значение элемента с индексом 1: ");
                Console.WriteLine(va[1]);

                Console.Write("\nЗначение элемента с индексом 100 до записи: ");
                Console.WriteLine(va[100]);
                va[100] = (byte)r.Next(1, 20);
                Console.Write("Новое значение элемента с индексом 100: ");
                Console.WriteLine(va[100]);

                Console.Write("\nЗначение элемента с индексом 5000 до записи: ");
                Console.WriteLine(va[5000]);
                va[5000] = (byte)r.Next(1, 20);
                Console.Write("Новое значение элемента с индексом 5000: ");
                Console.WriteLine(va[5000]);

                Console.Write("\nЗначение элемента с индексом 9999 до записи: ");
                Console.WriteLine(va[9999]);
                va[9999] = (byte)r.Next(1, 20);
                Console.Write("Новое значение элемента с индексом 9999: ");
                Console.WriteLine(va[9999]);

                va.Save();
            }
            catch(Exception e)
            {
                Console.WriteLine("Программа завершила свою работу с ошибкой: " + e.Message);
            }
        }
    }
}
