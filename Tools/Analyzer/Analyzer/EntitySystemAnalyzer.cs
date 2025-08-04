/*using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EntitySystemAnalyzer: DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [EntitySystemAnalyzerRule.Rule, EntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule];

    public override void Initialize(AnalysisContext context)
    {
        if (!AnalyzerGlobalSetting.EnableAnalyzer)
        {
            return;
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(this.Analyzer, SymbolKind.NamedType);
        context.RegisterSymbolAction(this.AnalyzeIsSystemMethodValid,SymbolKind.NamedType);
    }
}*/