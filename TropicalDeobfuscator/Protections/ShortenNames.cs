using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TropicalDeobfuscator.Protections
{
    internal class ShortenNames
    {
        private static List<string> JunkAttributes = new List<string>
        {
            "ConfusedByAttribute",
            "Form",
            "DotfuscatorAttribute",
            "ObfuscatedByGoliath",
            "TropicalObf",
            "PoweredByAttribute"
        };


        public static void Fix()
        {
            var module = DeobfuscatorContext.Module;
            foreach(TypeDef Type in module.GetTypes().ToArray())
            {
                if (JunkAttributes.Contains(Type.Name))
                    module.Types.Remove(Type);  //Remove Junk Attributes

                foreach (MethodDef Method in Type.Methods)
                {
                    if (Method.HasParams())
                    {
                        foreach(var param in Method.Parameters)
                        {
                            param.Name = ""; /*param names are fucking long laggy as fuck*/
                        }
                    }
                }            
            }
        }
    }
}
