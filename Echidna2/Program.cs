using Echidna2.Serialization;


Console.WriteLine("Hello, World!");
Project project = Compilation.Compile().WaitForResult();
project.Assembly.GetType("Prefabs.Editor.Editor")!.GetMethod("OpenNewWindow")!.Invoke(null, []);