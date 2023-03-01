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
        static public unsafe byte[] GetBytes<T>(T obj) where T : unmanaged
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];

            fixed (void* pointer = buffer)
            {
                Marshal.StructureToPtr(obj, new IntPtr(pointer), false);
                return buffer;
            }
        }

        static public unsafe T CreateStruct<T>(byte[] buffer) where T : unmanaged
        {
            fixed (void* pointer = buffer)
            {
                return (T)Marshal.PtrToStructure(new IntPtr(pointer), typeof(T));
            }
        }
    }
}
