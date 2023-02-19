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

namespace FileManagement
{
    public class Compilable : MonoBehaviour
    {

    }



    public static class CompilationManager
    {
        private static Assembly lastASM;
        private static string errors;

        public static void constructObject(ReferenceTypeS rTS, Vector3 spawnPoint)
        {
            if (lastASM == null)
                ObjectInstanceManager.createObjectInstance<ErrInstance>(errors, spawnPoint);

            try
            {
                object instance = lastASM.CreateInstance(rTS.name);
                ObjectInstanceManager.createObjectInstance(instance, spawnPoint);
            }
            catch (Exception e)
            {
                ObjectInstanceManager.createObjectInstance<ErrInstance>(e.ToString(), spawnPoint);
            }
        }

        public static void executeSnippet(string methodName, object[] args, Vector3 spawnPoint)
        {
            if (lastASM == null)
                ObjectInstanceManager.createObjectInstance<ErrInstance>(errors, spawnPoint);

            try
            {
                object instance = lastASM.CreateInstance("Snippets");
                MethodInfo methodInfo = instance.GetType().GetMethod(methodName);
                object result = methodInfo.Invoke(instance, args);

                ObjectInstanceManager.createObjectInstance(instance, spawnPoint);
            }
            catch (Exception e)
            {
                ObjectInstanceManager.createObjectInstance<ErrInstance>(e.ToString(), spawnPoint);
            }
        }

        public static void compileActiveWorkspace()
        {
            StringBuilder src = new StringBuilder();

            Dictionary<string, ReferenceTypeS>.ValueCollection values = FileManager.activeWorkspace._sourceFiles.Values;
            foreach (ReferenceTypeS rTS in values)
            {
                string typeCode = FileManager.loadSourceCode(rTS);
                if (typeCode != null)
                    src.AppendLine(typeCode);
            }

            string source = src.ToString();
            lastASM = compile(source);
        }

        public static Assembly compile(string source)
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
                errors = msg.ToString();

                return null;
            }



            // Return the assembly
            return result.CompiledAssembly;
        }
    }



    public static class FileManager
    {
        public static string[] workspaceNames
        {
            get
            {
                string[] paths = Directory.GetDirectories(PathManager.workspacesPath);
                string[] names = new string[paths.Length];
                for (int i = 0; i < paths.Length; i++)
                    names[i] = Path.GetFileName(paths[i]);
                return names;
            }
        }

        public static string[] sourceFileNames
        {
            get
            {
                Dictionary<string, ReferenceTypeS>.KeyCollection keys = activeWorkspace._sourceFiles.Keys;

                string[] names = new string[keys.Count];
                keys.CopyTo(names, 0);
                return names;
            }
        }



        private static bool prettyPrint = true;

        public static Workspace activeWorkspace = null;

        public static void loadWorkspace(string name)
        {
            string workspacePath = PathManager.makeDirectory(PathManager.workspacesPath + '/' + name);

            // vv allows people to exit and re-enter workspace without reloading
            if (activeWorkspace == null || activeWorkspace.path != workspacePath)
            {
                //WindowManager.destroyFileWindows();
                activeWorkspace = new Workspace(workspacePath);
            }
        }



        public static ReferenceTypeS getSourceFile(string name)
        {
            return activeWorkspace._sourceFiles[name];
        }

        public static bool saveSourceFile(string name)
        {
            return activeWorkspace.saveSourceFile(name);
        }

        public static ReferenceTypeS createSourceFile(string name, bool isClass)
        {
            return activeWorkspace.createSourceFile(name);
        }

        public static bool saveAllFiles()
        {
            return activeWorkspace.saveAllFiles();
        }



        public static List<BlockVariantS> loadCustomBlockVariants()
        {
            return activeWorkspace.loadCustomBlockVariants();
        }

        public static void saveCustomBlockVariants()
        {
            activeWorkspace.saveCustomBlockVariants();
        }



        public static ReferenceTypeS loadSourceFile(string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("Path does not exist " + path);
                return null;
            }

            try
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    ReferenceTypeS sourceFile = JsonUtility.FromJson<ReferenceTypeS>(json);
                    return sourceFile;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Issue with the formatting of text file " + path);
                return null;
            }
        }

        public static string loadSourceCode(ReferenceTypeS rTS)
        {
            string path = rTS.path + '/' + rTS.name + ".cs";
            if (!File.Exists(path))
            {
                Debug.Log("Code file " + rTS.name + ".cs does not exist");
                return null;
            }

            try
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string code = r.ReadToEnd();
                    return code;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Issue reading code " + rTS.name + ".cs");
                return null;
            }
        }





        public class Workspace
        {
            public string path;
            public Dictionary<string, ReferenceTypeS> _sourceFiles = null;

            public Workspace(string path)
            {
                this.path = path;
                findSourceFiles();
            }

            public void findSourceFiles()
            {
                _sourceFiles = new Dictionary<string, ReferenceTypeS>();

                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    string[] files = Directory.GetFiles(directory);
                    foreach (string file in files)
                    {
                        if (file.Contains(".json"))
                        {
                            ReferenceTypeS f = loadSourceFile(file);
                            // dictionary key is file name
                            if (f != null)
                            {
                                _sourceFiles.Add(f.name, f);
                                //Debug.Log("Loaded source file " + Path.GetFileName(file));
                            }
                            break;
                        }
                    }
                }
            }



            public bool saveSourceFile(string name)
            {
                try
                {
                    if (!_sourceFiles.ContainsKey(name))
                    {
                        Debug.Log("Source file " + name + " does not exist");
                        return false;
                    }
                    // get ref to source file object
                    ReferenceTypeS sourceFile = _sourceFiles[name];
                    sourceFile.save();

                    // check there is a directory
                    PathManager.makeDirectory(sourceFile.path);

                    // save the source code
                    using (StreamWriter w = new StreamWriter(sourceFile.path + '/' + sourceFile.name + ".cs"))
                        w.WriteLine(sourceFile.getCode());
                    // save the block structure
                    using (StreamWriter w = new StreamWriter(sourceFile.path + '/' + sourceFile.name + ".json"))
                        w.WriteLine(JsonUtility.ToJson(sourceFile, prettyPrint));

                    return true;
                }
                catch (Exception e)
                {
                    Debug.Log(e.StackTrace);
                    Debug.Log("Issue saving source file " + name + '.');
                    return false;
                }
            }

            public bool saveAllFiles()
            {
                foreach (string file in _sourceFiles.Keys)
                    if (!saveSourceFile(file)) return false;
                return true;
            }

            public ReferenceTypeS createSourceFile(string name)
            {
                if (_sourceFiles.ContainsKey(name))
                {
                    Debug.Log("File already exists");
                    return null;
                }

                ReferenceTypeS rTS = new ReferenceTypeS(path + '/' + name, name);
                _sourceFiles.Add(name, rTS);
                return rTS;
            }



            public List<BlockVariantS> loadCustomBlockVariants()
            {
                string path = this.path + "/Workspace.json";
                if (!File.Exists(path)) return null;

                try
                {
                    using (StreamReader r = new StreamReader(path))
                    {
                        string json = r.ReadToEnd();
                        WorkspaceS workspaceFile = JsonUtility.FromJson<WorkspaceS>(json);
                        return workspaceFile.customBlockVariants;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Issue with the formatting of text file " + path);
                    return null;
                }
            }

            public void saveCustomBlockVariants()
            {
                try
                {
                    // create a workspace save json
                    WorkspaceS wS = new WorkspaceS();
                    string json = JsonUtility.ToJson(wS, prettyPrint);

                    // save the source code
                    using (StreamWriter w = new StreamWriter(path + "/Workspace.json"))
                        w.WriteLine(json);
                }
                catch (Exception e)
                {
                    Debug.Log(e.StackTrace);
                    Debug.Log("Issue converting Workspace to json");
                }
            }
        }
    }



    public static class PathManager
    {
        public static string workspacesPath
        {
            get { return makeDirectory(Application.persistentDataPath + "/Workspaces"); }
        }

        public static string makeDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
            catch
            {
                Debug.Log("Err making directory " + path);
                return null;
            }
        }
    }
}
