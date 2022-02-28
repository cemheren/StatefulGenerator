namespace Stateful.Generators
{
    using GeneratorDependencies;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    [Generator]
    public class ComponentAnalysisGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif 

            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // the generator infrastructure will create a receiver and populate it
            // we can retrieve the populated instance via the context
            MySyntaxReceiver syntaxReceiver = (MySyntaxReceiver)context.SyntaxReceiver;

            var classes = syntaxReceiver.ClassesToAugment;
            var dir = Assembly.GetExecutingAssembly().Location;
            var compilation = context.Compilation;

            var strlist = new List<string>();

            foreach (var classDeclarationSyntax in classes)
            {
                strlist.Add(classDeclarationSyntax.Identifier.ToString());

                var semantic = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                var classStrlist = new List<string>();

                foreach (var node in classDeclarationSyntax.DescendantNodes())
                {
                    var descendantType = semantic.GetTypeInfo(node).Type;

                    // todo: filter based on assembly here to reduce noise.
                    if (descendantType != null)
                    {
                        classStrlist.Add($"  {descendantType.Name}");
                    }
                }

                classStrlist.ForEach(x => strlist.Add(x));
            }

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(dir), "Analysis.txt"), string.Join("\n", strlist));
        }

        class MySyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> ClassesToAugment = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Business logic to decide what we're interested in goes here
                if (syntaxNode is ClassDeclarationSyntax cds)
                {
                    if (cds.AttributeLists.Any(a => a is AttributeListSyntax als && als.ToString() == "[ComponentAnalysis]"))
                    {
                        ClassesToAugment.Add(cds);
                    }
                }
            }
        }
    }
}
