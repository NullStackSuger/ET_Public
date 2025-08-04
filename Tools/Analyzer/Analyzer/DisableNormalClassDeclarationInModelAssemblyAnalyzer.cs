using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DisableNormalClassDeclarationInModelAssemblyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DisableNormalClassDeclarationInModelAssemblyAnalyzerRule.Rule);

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
            if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, AnalyzerAssembly.AllModel))
            {
                analysisContext.RegisterSyntaxNodeAction(this.Analyze,SyntaxKind.ClassDeclaration);
            }
        });
            
    }
        
    private void Analyze(SyntaxNodeAnalysisContext analysisContext)
    {
        
    }
}