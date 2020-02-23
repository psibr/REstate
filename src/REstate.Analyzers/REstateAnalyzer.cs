using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REstate.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class REstateAnalyzer : DiagnosticAnalyzer
    {
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.RESTATE001_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.RESTATE001_Format), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static DiagnosticDescriptor RESTATE001 = new DiagnosticDescriptor("RESTATE001", Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(RESTATE001); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleBaseType);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var baseDeclaration = (SimpleBaseTypeSyntax)context.Node;

            var baseItem = baseDeclaration.ChildNodes().OfType<GenericNameSyntax>().FirstOrDefault();

            // only consider IAcceptSignal
            if (baseItem?.Identifier.ValueText != "IAcceptSignal") return;

            var symbol = context.SemanticModel.GetSymbolInfo(baseItem);
            if (symbol.Symbol?.OriginalDefinition?.ToDisplayString() != "REstate.Natural.IAcceptSignal<TSignal>") return;

            var signalNode = baseItem.TypeArgumentList.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();

            if(signalNode is null) return;

            var signalType = context.SemanticModel.GetTypeInfo(signalNode);
            if(signalType.Type?.TypeKind != TypeKind.Interface) return;

            context.ReportDiagnostic(Diagnostic.Create(RESTATE001, context.Node.GetLocation(), signalType.Type.ToDisplayString()));
        }
    }
}
