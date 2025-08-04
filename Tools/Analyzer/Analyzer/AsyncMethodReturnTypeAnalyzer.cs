using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncMethodReturnTypeAnalyzer: DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AsyncMethodReturnTypeAnalyzerRule.Rule);
        
    public override void Initialize(AnalysisContext context)
    {
        if (!AnalyzerGlobalSetting.EnableAnalyzer)
        {
            return;
        }
        
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(this.Analyzer, SyntaxKind.MethodDeclaration, SyntaxKind.LocalFunctionStatement);
    }

    private void Analyzer(SyntaxNodeAnalysisContext analysisContext)
    {
        
    }
}