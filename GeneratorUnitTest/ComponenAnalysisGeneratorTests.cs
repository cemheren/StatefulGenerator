using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using GeneratorDependencies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stateful.Generators;

namespace GeneratorUnitTest
{
    [TestClass]
    public class ComponenAnalysisGeneratorTests
    {
        private static string Filler =>
@"
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
"
;
        
        [TestMethod]
        public void SimpleGeneratorTest()
        {
            string userSource = @$"
namespace Program.Test
{{
    using System;
    using GeneratorDependencies;

    {Filler}

    
    public class DependencyLevelOne
    {{ }}

    [ComponentAnalysis]
    public class TopLevelClassOne
    {{
        private DependencyLevelOne dependencyLevelOne;
    }}

    [ComponentAnalysis]
    public class TopLevelClassTwo
    {{
        private DependencyLevelOne dependencyLevelOne;
    }}
}}
";

            Compilation comp = CreateCompilation(userSource);
            var errors = comp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

            var newComp = RunGenerators(comp, out var generatorDiags, new ComponentAnalysisGenerator());
            //var newFile = newComp.SyntaxTrees.Single(x => Path.GetFileName(x.FilePath).EndsWith("Generated.cs"));

            //Assert.IsNotNull(newFile);
            //var generatedfile = newFile.GetText().ToString();

            //Assert.IsTrue(generatedfile.Contains("state.x = 6"), message: "state.x = 6");

            //Assert.AreEqual(0, generatorDiags.Length);
            //errors = newComp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

            //Assert.AreEqual(0, errors.Count, 
            //    message: string.Join("\n ", errors));
        }

        private static Compilation CreateCompilation(string source)
        {
            var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);

            // need to manually add .netstandard since the GeneratorDependencies is on .netstandard2.0 
            var references = new List<PortableExecutableReference>{
                    MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IGeneratorCapable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "netstandard.dll")
                };

            Assembly.GetEntryAssembly().GetReferencedAssemblies()
                .ToList()
                .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

            return CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions()) },
                references,
                new CSharpCompilationOptions(OutputKind.WindowsApplication));
        }
        
        private static Compilation RunGenerators(Compilation c, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CSharpGeneratorDriver.Create(generators).RunGeneratorsAndUpdateCompilation(c, out var outputCompilation, out diagnostics);
            return outputCompilation;
        }
    }
 }

