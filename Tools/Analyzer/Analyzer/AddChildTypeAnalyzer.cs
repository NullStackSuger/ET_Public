using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

// TODO 分析器/具体实现没写

namespace ET.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AddChildTypeAnalyzer: DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AddChildTypeAnalyzerRule.Rule, DisableAccessEntityChildAnalyzerRule.Rule);

    public override void Initialize(AnalysisContext context)
    {
        if (!AnalyzerGlobalSetting.EnableAnalyzer)
        {
            return;
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(analysisContext =>
        {
            if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, AnalyzerAssembly.AllModelHotfix))
            {
                analysisContext.RegisterSyntaxNodeAction(this.Analyzer, SyntaxKind.SimpleMemberAccessExpression);
            }
        });
    }

    private void Analyzer(SyntaxNodeAnalysisContext analysisContext)
    {
       
    }
}