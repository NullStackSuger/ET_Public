/*using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EntityHashCodeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [EntityHashCodeAnalyzerRule.Rule];
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
                analysisContext.RegisterSemanticModelAction((this.Analyzer));
            }
        });
        /*var entityHashCodeMap = new ConcurrentDictionary<long, string>();
        context.RegisterCompilationStartAction(analysisContext =>
        {
            CompilationStartAnalysis(analysisContext, entityHashCodeMap);
        });#1#
    }
    
    private void Analyzer(SemanticModelAnalysisContext analysisContext)
    {
        
    }

    private void CompilationStartAnalysis(CompilationStartAnalysisContext context,ConcurrentDictionary<long, string> entityHashCodeMap)
    {
            
        context.RegisterSemanticModelAction((analysisContext =>
        {
            if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.SemanticModel.Compilation.AssemblyName,AnalyzeAssembly.AllModel))
            {
                AnalyzeSemanticModel(analysisContext, entityHashCodeMap);
            }
        } ));
    }

    private void AnalyzeSemanticModel(SemanticModelAnalysisContext analysisContext, ConcurrentDictionary<long, string> entityHashCodeMap)
    {
        
    }
}*/