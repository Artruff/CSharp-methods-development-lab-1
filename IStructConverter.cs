using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_methods_development_lab_1
{
    interface IStructConverter
    {
        unsafe byte[] GetBytes<T>(T obj) where T : struct;
        unsafe T CreateStruct<T>(byte[] buffer) where T : struct;
    }
}
