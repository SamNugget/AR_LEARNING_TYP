using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

using System.IO;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ObjectInstances;

namespace Compilation
{
    public class CompilationUnit : MonoBehaviour
    {
        private void CompileType(string source)
        {
            Assembly assembly = Compile(source);

            if (assembly == null) return;

            object instance = assembly.CreateInstance("HowdyYall");
            ObjectInstanceManager.createObjectInstance<CustomTypeInstance>(instance, transform.position);

            var method = instance.GetType().GetMethod("Foo");
            method.Invoke(instance, null);

            object newInstance = assembly.CreateInstance("Test");
            var newMethod = newInstance.GetType().GetMethod("Foo");
            newMethod.Invoke(newInstance, null);

            //var method = assembly.GetType("Test").GetMethod("Foo");
            //var del = (Action)Delegate.CreateDelegate(typeof(Action), method);
            //del.Invoke();
        }


        public Assembly Compile(string source)
        {
            var provider = new CSharpCodeProvider();
            var param = new CompilerParameters();



            // Add ALL of the assembly references
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = Path.GetFileName(assembly.Location);
                try
                {
                    if (!assembly.Location.Contains("PlasticSCM"))
                    {
                        param.ReferencedAssemblies.Add(assembly.Location);
                        continue;
                    }
                }
                catch { }
                Debug.Log("Failed to add: " + assemblyName);
            }



            // Generate a dll in memory
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;



            // Compile the source
            var result = provider.CompileAssemblyFromSource(param, source);



            if (result.Errors.Count > 0)
            {
                var msg = new StringBuilder();
                foreach (CompilerError error in result.Errors)
                {
                    msg.AppendFormat("Error ({0}): {1}\n",
                    error.ErrorNumber, error.ErrorText);
                }
                //throw new Exception(msg.ToString());

                ObjectInstanceManager.createObjectInstance<ErrInstance>(msg.ToString(), transform.position);
                return null;
            }



            // Return the assembly
            return result.CompiledAssembly;
        }


        void Start()
        {
            CompileType(@"
            using UnityEngine;

            [System.Serializable]
            public class HowdyYall
            {
                public string greeting = ""howdy"";

                public void Foo()
                {
                    Debug.Log(""Hello, World!"");
                    GameObject.CreatePrimitive(PrimitiveType.Cube);
                }
            }

            public class Test
            {
                public void Foo()
                
                    Debug.Log(""Attempt"");

                    HowdyYall hY = new HowdyYall();
                    hY.Foo();
                }
            }");
        }
    }

    public static class CompilationManager
    {
        
    }
}

