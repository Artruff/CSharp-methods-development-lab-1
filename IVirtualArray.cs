using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_methods_development_lab_1
{
    //Структура страницы в буфере
    struct page<T> where T : unmanaged
    {
        public int fileIndex;
        public bool modify;
        public TimeSpan timeOfModify;
        public T[] elements;
        public bool[] bitMap;
    }
    //Интерфейс виртуального массива
    interface IVirtualArray<T> where T : unmanaged
    {
        //файл подкачки
        FileInfo swapFile { get; }
        //Размер массива
        public long size { get; }
        //Поиск страницы в буфере
        int FindIndexPage(long globalElementIndex);
        //Перегрузка оператора доступа
        T this[int index] { get; set; }
    }
}
