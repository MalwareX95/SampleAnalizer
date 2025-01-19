using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace SampleAnalizer
{ 
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticFieldAnalizer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MY0002";

        private const string Title = "Blocks should use braces";
        private const string MessageFormat = "Passed parameter should be a static field";
        private const string Description = "When possible, use curly braces on code blocks.";
        private const string Category = "CodeStyle";

        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Do, SyntaxKind.InvocationExpression);
        }

        private void Do(SyntaxNodeAnalysisContext context)
        {
            var node = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
            
            if (methodSymbol != null)
            {
                var staticFieldSymbol = context.Compilation.GetTypeByMetadataName(typeof(StaticField).FullName);

                var arguments = methodSymbol.Parameters
                    .Zip(node.ArgumentList.Arguments, (Param, Arg) => (Param, Arg))
                    .Where(x => x.Param.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, staticFieldSymbol)))
                    .Where(x => !IsStaticFieldSymbol(context.SemanticModel.GetSymbolInfo(x.Arg.Expression).Symbol))
                    .Select(x => x.Arg);

                foreach (var arg in arguments)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, arg.GetLocation()));
                }
            }
        }

        static bool IsStaticFieldSymbol(ISymbol symbol) => symbol is IFieldSymbol fieldSymbol && fieldSymbol.IsStatic && fieldSymbol.IsReadOnly;
    }
}
