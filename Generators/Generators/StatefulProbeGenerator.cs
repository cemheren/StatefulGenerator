//-----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------

namespace Hackathon21Poc.Generators
{
    using GeneratorDependencies;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    [Generator]
    public class StatefulProbeGenerator : ISourceGenerator
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

            // get the recorded user class
            var classes = syntaxReceiver.ClassesToAugment;

            foreach (var cls in classes)
            {
                GenerateStatefulMethods(cls, context);
            }
        }

        private void GenerateStatefulMethods(ClassDeclarationSyntax userClass, GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            var generatedMethodBody = "";

            var semanticModel = compilation.GetSemanticModel(userClass.SyntaxTree);
            var syntaxTreeRoot = userClass.SyntaxTree.GetRoot();

            var usingStatements = syntaxTreeRoot
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .ToArray();

            var methodBody = syntaxTreeRoot
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(x => x.Identifier.ValueText == "StatelessImplementation")
                .Single()
                .Body;

            var variableDictionary = methodBody
                .DescendantNodes()
                .OfType<VariableDeclarationSyntax>()
                .ToDictionary(keySelector: (variable) => { return variable.ChildNodes().OfType<VariableDeclaratorSyntax>().First().Identifier.ValueText; });

            var identifiers = methodBody
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .ToArray();

            var variableIdentifiers = identifiers.Where(identifier => variableDictionary.ContainsKey(identifier.Identifier.ValueText));

            methodBody = methodBody.ReplaceNodes(variableIdentifiers,
                (node, x) =>
                {
                    var memberAccessExpressionSyntax = SyntaxFactory
                        .MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, 
                            SyntaxFactory.IdentifierName("state"), 
                            SyntaxFactory.IdentifierName(node.Identifier.ValueText))
                        .WithTriviaFrom(node);
                    return memberAccessExpressionSyntax;
                });

            var usingStatementsText = string.Join("", usingStatements.Select(statement => statement.GetText().ToString()).ToArray());

            var statementsSplitByInterleaver = new List<List<StatementSyntax>>();
            statementsSplitByInterleaver.Add(new List<StatementSyntax>()); // add the initial one.

            var i = 0;
            var interleaverCount = 0;
            while (i < methodBody.Statements.Count)
            {
                for (; i < methodBody.Statements.Count; i++)
                {
                    var statement = methodBody.Statements[i];

                    if (statement is ExpressionStatementSyntax expressionStatement
                        && expressionStatement.Expression is InvocationExpressionSyntax invocationExpression)
                    {
                        if (invocationExpression.GetText().ToString().Contains("Interleaver.Pause"))
                        {
                            statementsSplitByInterleaver.Add(new List<StatementSyntax>()); // add the initial one.
                            interleaverCount++; i++;
                            break;
                        }

                        if (invocationExpression.GetText().ToString().Contains("Interleaver.Wait"))
                        {
                            statementsSplitByInterleaver.Add(new List<StatementSyntax>());
                            interleaverCount++; i++;

                            var waitTime = invocationExpression.ArgumentList.Arguments[0].Expression.GetText().ToString();

                            var waitString = $@"{{TimeSpan elapsedTime = DateTime.UtcNow - state.CurrentStateStartTime;

                            if (elapsedTime < {waitTime}) {{
                                Console.WriteLine($""elapsed time: {{elapsedTime}}"");
                                return;  
                            }}

                            Console.WriteLine($""Finished waiting {{{waitTime}}}"");
                            }}";

                            var block = SyntaxFactory.ParseStatement(waitString);

                            statementsSplitByInterleaver[interleaverCount].Add(block);

                            statementsSplitByInterleaver.Add(new List<StatementSyntax>()); // add the initial one.
                            interleaverCount++;
                            break;
                        }
                    }

                    statementsSplitByInterleaver[interleaverCount].Add(statement);
                }
            }

            for (i = 0; i < statementsSplitByInterleaver.Count; i++)
            {
                var stateSegment = statementsSplitByInterleaver[i];
                var stateSegmentLines = GetLines(stateSegment, variableDictionary.Values.ToList());

                generatedMethodBody = $@" {generatedMethodBody}
            if (state.ExecutionState == {i}) {{
{stateSegmentLines}
                {(i != statementsSplitByInterleaver.Count - 1 ? $"state.ExecutionState = {i + 1};" : "state.ExecutionState = -1;")}
                state.CurrentStateStartTime = DateTime.UtcNow;
                return;
            }}
                
";
            }


            // add the generated implementation to the compilation
            SourceText sourceText = SourceText.From($@"
namespace Hackathon21Poc.Probes {{
    {usingStatementsText}

    public partial class {userClass.Identifier}State
    {{
        {GetProperties(semanticModel, variableDictionary.Values.ToList())}
    }}

    public partial class {userClass.Identifier}
    {{
        public partial void GeneratedStatefulImplementation({userClass.Identifier}State state)
        {{
            {
                generatedMethodBody
            }
            System.Diagnostics.Debug.WriteLine(""test"");
        }}
    }}
}}", Encoding.UTF8);
            context.AddSource($"{userClass.Identifier.Text}.Generated.cs", sourceText);
        }

        private string GetLines(List<StatementSyntax> statementGroup, List<VariableDeclarationSyntax> variables, int indent = 1)
        {
            string leftIndent = String.Join("", Enumerable.Repeat("    ", indent));

            var sb = new StringBuilder();
            foreach (var statement in statementGroup)
            {
                // manually remove all the initializers. Couldn't figure out a quick way to do this with ReplaceNodes since these are not statements
                if (statement is LocalDeclarationStatementSyntax localDeclaration
                        && localDeclaration.Declaration is VariableDeclarationSyntax variableDeclaration)
                {
                    var declarator = variableDeclaration.ChildNodes().OfType<VariableDeclaratorSyntax>().First();
                    sb.AppendLine($"{leftIndent}{variableDeclaration.GetLeadingTrivia().ToFullString()}state.{declarator.Identifier.ValueText} = {declarator.Initializer.Value};");
                    continue;
                }

                sb.Append($"{statement.GetLeadingTrivia().ToString().Trim('\n')}");
                foreach (var line in statement.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    sb.AppendLine($"{leftIndent}{line}");
                }
                sb.Append($"{statement.GetTrailingTrivia().ToString().Trim('\n')}");
            }

            return sb.ToString();
        }

        private string GetProperties(SemanticModel sm, List<VariableDeclarationSyntax> variables)
        {
            var sb = new StringBuilder();
            foreach (var variable in variables)
            {
                var type = variable.Type.ToString();
                var declarator = variable.ChildNodes().OfType<VariableDeclaratorSyntax>().First();

                sb.AppendLine($"public {type} {declarator.Identifier.ValueText};");
                sb.Append("        "); // Indent
            }

            return sb.ToString();
        }

        class MySyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> ClassesToAugment = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Business logic to decide what we're interested in goes here
                if (syntaxNode is ClassDeclarationSyntax cds)
                {
                    if (cds.BaseList?.Types.Any(node => node.Type.ToString() == ("IGeneratorCapable")) == true)
                    {
                        ClassesToAugment.Add(cds);
                    }
                }
            }
        }
    }
}
