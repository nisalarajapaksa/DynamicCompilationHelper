using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DynamicCompilationHelper
{
    public class Compiler
    {
        public byte[] CompileByPath(string filepath, List<string> classLibs, List<string> assemblies, string outputDllName = "DynamicallyCompiled")
        {
            filepath = ResolvePath(filepath);
            Console.WriteLine($"Starting compilation of: '{filepath}'");
            var sourceCode = File.ReadAllText(filepath);
            return Compile(sourceCode, classLibs, assemblies, outputDllName);
        }

        public byte[] Compile(string sourceCode, List<string> classLibs, List<string> assemblies, string outputDllName = "DynamicallyCompiled")
        {
            using (var peStream = new MemoryStream())
            {
                var result = GenerateCode(sourceCode, classLibs, assemblies, outputDllName).Emit(peStream);

                if (!result.Success)
                {
                    Console.WriteLine("Compilation done with error.");

                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (var diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    return null;
                }

                Console.WriteLine("Compilation done without any error.");

                peStream.Seek(0, SeekOrigin.Begin);

                return peStream.ToArray();
            }
        }

        private static CSharpCompilation GenerateCode(string sourceCode, List<string> classLibs, List<string> assemblies, string outputDllName)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);
            outputDllName = string.Concat(outputDllName, ".dll");

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location)
            };

            foreach (var classLib in classLibs)
            {
                references.Add(MetadataReference.CreateFromFile(ResolvePath(classLib)));
            }

            foreach (var assembly in assemblies)
            {
                references.Add(MetadataReference.CreateFromFile(System.Reflection.Assembly.Load(assembly).Location));
            }

            return CSharpCompilation.Create(outputDllName,
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        private static string ResolvePath(string pathName) {
            string pathResolved;
            if (System.IO.Path.IsPathRooted(pathName))
            {
                pathResolved = pathName.Replace('\\', Path.DirectorySeparatorChar);
            }
            else
            {
#if DEBUG
                string root = Path.GetFullPath(Path.Combine(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(
                                Path.GetDirectoryName(
                                    Path.GetDirectoryName(
                                        Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)))))));

                pathResolved = Path.GetFullPath(Path.Combine(root, pathName.Replace('\\', Path.DirectorySeparatorChar)));
#else
                pathResolved = Path.GetFullPath(pathName.Replace('\\', Path.DirectorySeparatorChar));
#endif
            }

            return pathResolved;
        }
    }
}
