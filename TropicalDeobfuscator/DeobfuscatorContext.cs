using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TropicalDeobfuscator
{
    public class DeobfuscatorContext
    {

       
        public static MethodDef BoolDecryptionMethod
        {
            get;
            set;
        }

        public static ModuleDefMD Module
        {
            get;
            set;
        }
        public static Assembly ReflectionAssembly
        {
            get;
            set;
        }
    }
}
