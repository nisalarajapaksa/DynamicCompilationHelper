using DynamicCompilationHelper;
using System;
using System.Collections.Generic;

namespace SampleProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Sample");

            // Source code as a text input
            CompileAndRunTextInput();

            // Source code as a text input
            // CompileAndRunFileInput();
        }

        static void CompileAndRunTextInput() {
            var sourceCode = @"
using System;
using System.Data.SqlClient;

namespace Sample
{
    class DynamicProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine($""Dynamically Compiled Program --> { args[0]}!"");
        }
    }
}
";
            var logicLibs = new List<string> {
                @"SampleProgram\bin\Debug\netcoreapp3.1\System.Data.SqlClient.dll",
            };

            var assemblies = new List<string> {
                "netstandard, Version=2.0.0.0",
                "System.Runtime.Extensions, Version=4.2.2.0",
                "System.ComponentModel.Primitives, Version=4.2.2.0",
                "System.Data.Common, Version=4.2.1.0"
            };

            var dllName = "FromText";

            var compiler = new Compiler();
            var runner = new Runner();

            // Source code as a text input
            var compiled = compiler.Compile(sourceCode, logicLibs, assemblies, dllName);
            runner.Execute(compiled, new[] { "Sample Input Text" });
        }

        static void CompileAndRunFileInput()
        {
            var sourceFilePath = @"SampleProgram\bin\Debug\netcoreapp3.1\SourceCodeTextFile.txt";
            var logicLibs = new List<string> {
                @"SampleProgram\bin\Debug\netcoreapp3.1\System.Data.SqlClient.dll",
            };

            var assemblies = new List<string> {
                "netstandard, Version=2.0.0.0",
                "System.Runtime.Extensions, Version=4.2.2.0",
                "System.ComponentModel.Primitives, Version=4.2.2.0",
                "System.Data.Common, Version=4.2.1.0"
            };

            var dllName = "FromFile";

            var compiler = new Compiler();
            var runner = new Runner();

            // source code from a file
            var compiled = compiler.CompileByPath(sourceFilePath, logicLibs, assemblies, dllName);
            runner.Execute(compiled, new[] { "Sample Input Text" });
        }
    }
}
