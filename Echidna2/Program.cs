using System.Runtime.Loader;
using Echidna2.Serialization;


Console.WriteLine("Hello, World!");
Compilation.Compile().Wait();

AssemblyLoadContext projectAssemblyLoadContext = new("EchidnaProject");
using FileStream projectAssemblyFileStream = new(Compilation.CompilationDllPath, FileMode.Open, FileAccess.Read);
TomlDeserializer.ProjectAssembly = projectAssemblyLoadContext.LoadFromStream(projectAssemblyFileStream);

TomlDeserializer.ProjectAssembly.GetType("Prefabs.Editor.Editor").GetMethod("OpenNewWindow").Invoke(null, []);