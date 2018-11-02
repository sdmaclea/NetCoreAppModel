// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Runtime.Loader;
using System.Reflection;
using System.Resources;
using System.Globalization;
//using Console = Internal.Console;

namespace SatelliteALCIsolation
{
    class MyALC : AssemblyLoadContext
    {
        public string [] ProbePaths { get; set;} 
        
        public MyALC() { Resolving += ResolveTrace;}

        protected override Assembly Load(AssemblyName assemblyName)
        {
            Console.WriteLine($"MyALC.Load(AssemblyName {assemblyName})");
            foreach (string path in ProbePaths)
            {
                string assemblyPath = String.Format(path, assemblyName.Name, assemblyName.CultureName);
                try
                {
                    Assembly tryLoad = LoadFromAssemblyPath(assemblyPath);
                
                    if (tryLoad != null)
                    {
                        Console.WriteLine($"{assemblyPath} Found");
                        return tryLoad;
                    }
                }
                catch
                {
                    Console.WriteLine($"{assemblyPath} NOT Found");
                }
            }
            return null;
        }
        
        Assembly ResolveTrace(AssemblyLoadContext alc, AssemblyName assemblyName)
        {
            Console.WriteLine($"MyALC.ResolveTrace(AssemblyName {assemblyName})");
            
            return null;
        }
    }

    class Program
    {
        static String describeLib(String assemblyPath, String culture)
        {
            string result = "Oops";
            try
            {
                string [] probePaths = new string[] 
                {
                    System.IO.Path.GetDirectoryName(assemblyPath) + "/{1}/{0}.dll",
                };
                
                AssemblyLoadContext alc = new MyALC { ProbePaths=probePaths };

                Assembly classLibWithSatellite = alc.LoadFromAssemblyPath(assemblyPath);
                
                Type describeType = classLibWithSatellite.GetType("ClassLibWithSatellite.ClassLibWithSatellite");

                result = (String)describeType.InvokeMember("Describe", BindingFlags.InvokeMethod, null, null, new object[] { culture });

            }
            catch (Exception e)
            {
                String excepDesc = e.ToString();
                Console.WriteLine($"Unexpected exception: {excepDesc}");
            }

            Console.WriteLine($"result: {result}");

            return result;
        }

        static public string describeMain(string lang)
        {
            try
            {
                ResourceManager rm = new ResourceManager(typeof(MainStrings));
    
                CultureInfo ci = CultureInfo.CreateSpecificCulture(lang);
        
                return rm.GetString("Describe", ci);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        static int Main()
        {
            int result = 100;

            String myPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            String classLibV1Path = System.IO.Path.GetDirectoryName(myPath) + "/ClassLibVer1/ClassLibWithSatellite.dll";
            String classLibV2Path = System.IO.Path.GetDirectoryName(myPath) + "/ClassLibVer2/ClassLibWithSatellite.dll";

            String v1FrMainDesc = describeMain("fr-FR");

            if (!v1FrMainDesc.Equals("Neutral language Main description 1.0.0"))
            {
                Console.WriteLine($"Unexpected result v1FrMainDesc : {v1FrMainDesc}");
                result += 1;
            }

            String v1EnMainDesc = describeMain("en-US");

            if (!v1EnMainDesc.Equals("English language Main description 1.0.0"))
            {
                Console.WriteLine($"Unexpected result v1EnMainDesc : {v1EnMainDesc}");
                result += 1;
            }
            
            String refFrDesc = ReferencedClassLib.Program.Describe("fr-FR");
            
            if (!refFrDesc.Equals("Neutral language ReferencedClassLib description 1.0.0"))
            {
                Console.WriteLine($"Unexpected result refFrDesc : {refFrDesc}");
                result += 1;
            }

            String refEnDesc = ReferencedClassLib.Program.Describe("en-US");

            if (!refEnDesc.Equals("English language ReferencedClassLib description 1.0.0"))
            {
                Console.WriteLine($"Unexpected result refEnDesc : {refEnDesc}");
                result += 1;
            }
            
            String v1FrDesc = describeLib(classLibV1Path, "fr-FR");

            if (!v1FrDesc.Equals("Neutral language description 1.0.0"))
            {
                Console.WriteLine($"Unexpected result v1FrDesc : {v1FrDesc}");
                result += 1;
            }

            String v1EnDesc = describeLib(classLibV1Path, "en-US");

            if (!v1EnDesc.Equals("English language description 1.0.0"))
            {
                Console.WriteLine($"Unexpected result v1EnDesc : {v1EnDesc}");
                result += 1;
            }

            String v2Desc = describeLib(classLibV2Path, "en-US");
            if (!v2Desc.Equals("English language description 2.1.2"))
            {
                Console.WriteLine($"Unexpected result v2Desc : {v2Desc}");
                result += 1;
            }

            return result;
        }
    }
    
    class MainStrings
    {
    }
}
