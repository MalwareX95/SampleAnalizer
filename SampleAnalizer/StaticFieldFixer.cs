using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SampleAnalizer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StaticFieldFixer)), Shared]
    internal class StaticFieldFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(StaticFieldAnalizer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root?.FindNode(diagnosticSpan);

            if (node is null)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Create field",
                    c => CreateField(context.Document, node, c), 
                    "Create field"), 
                diagnostic);
        }

        private static async Task<Document> CreateField(Document document, SyntaxNode node, CancellationToken cancellationToken)
        {
            var services = document.Project.Solution.Workspace.Services;
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var editor = new SyntaxEditor(root, services);
            var type = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();

            TypeSyntax intType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));

            // Create the generic type 'Func<int>'
            TypeSyntax funcType = SyntaxFactory.QualifiedName(
                SyntaxFactory.IdentifierName("System"),
                SyntaxFactory.GenericName(SyntaxFactory.Identifier("Func")) 
                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(intType))));
            
            var fieldDeclaration = SyntaxFactory
                .VariableDeclaration(funcType)
                .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator("_myField")));

            editor.ReplaceNode(type, type.AddMembers(SyntaxFactory.FieldDeclaration(fieldDeclaration)));

            return document.WithSyntaxRoot(editor.GetChangedRoot());
        }
    }
}
