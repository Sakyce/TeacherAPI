using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TeacherAPI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NoBehaviorStateMachineInInitialize : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TeacherAPI_0001";

        private const string Title = "Calling ChangeState inside Initialize method";
        private const string MessageFormat = "Avoid calling ChangeState inside the 'Initialize' method of {0}.";
        private const string Category = "Usage";
        private const string Description = "Use abstract GetAngryState and GetHappyState for initializing your Teacher state machine instead.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.MethodDeclaration);
        }
        private static bool IsDerivedFromTeacher(INamedTypeSymbol typeSymbol)
        {
            while (typeSymbol != null)
            {
                if (typeSymbol.Name == "Teacher")
                    return true;
                typeSymbol = typeSymbol.BaseType;
            }
            return false;
        }
        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            if (methodDeclaration.Identifier.ValueText != "Initialize")
                return;

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null)
                return;

            var containingClass = methodSymbol.ContainingType;
            if (containingClass == null)
                return;

            if (!IsDerivedFromTeacher(containingClass))
                return;

            var methodCalls = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var call in methodCalls)
            {
                if (call.Expression is MemberAccessExpressionSyntax methodIdentifier
                    && methodIdentifier.Expression.ToString().EndsWith("behaviorStateMachine")
                    && methodIdentifier.Name.ToString().Equals("ChangeState")
                    )
                {
                    var diagnostic = Diagnostic.Create(Rule, call.GetLocation(), containingClass.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
