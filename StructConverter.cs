using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_methods_development_lab_1
{
    //Конвертирует структуры в байты и наоборот
    static class StructConverter
    {
        /// <summary>
        /// Конвертирует структуры в массив байтов
        /// </summary>
        /// <typeparam name="T">Тип структуры конвертирования</typeparam>
        /// <param name="obj">Переменная для конвертирования</param>
        /// <returns>Массив байтов/returns>
        static public unsafe byte[] s_getBytes<T>(T obj) where T : unmanaged
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];

            fixed (void* pointer = buffer)
            {
                Marshal.StructureToPtr(obj, new IntPtr(pointer), false);
                return buffer;
            }
        }

        /// <summary>
        /// Создаёт структуру из массива байт
        /// </summary>
        /// <typeparam name="T">Тип структуры конвертирования</typeparam>
        /// <param name="buffer">Массив байт</param>
        /// <returns></returns>
        static public unsafe T s_createStruct<T>(byte[] buffer) where T : unmanaged
        {
            fixed (void* pointer = buffer)
            {
                return (T)Marshal.PtrToStructure(new IntPtr(pointer), typeof(T));
            }
        }
    }
}
