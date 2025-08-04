/*using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StaticClassCircularDependencyAnalyzer: DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [StaticClassCircularDependencyAnalyzerRule.Rule];

    public override void Initialize(AnalysisContext context)
    {
        if (!AnalyzerGlobalSetting.EnableAnalyzer)
        {
            return;
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(this.CompilationStartAnalysis);
    }

    private void CompilationStartAnalysis(CompilationStartAnalysisContext context)
    {
        var dependencyMap = new ConcurrentDictionary<string, Dictionary<string,List<InvocationExpressionSyntax>>>();
        var staticClassSet = new HashSet<string>();
            
        if (AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllHotfix))
        {
                
            context.RegisterSyntaxNodeAction(analysisContext => { this.StaticClassDependencyAnalyze(analysisContext, dependencyMap, staticClassSet); }, SyntaxKind.InvocationExpression);
            context.RegisterCompilationEndAction(analysisContext => { this.CircularDependencyAnalyze(analysisContext, dependencyMap, staticClassSet); });
        }
    }

        
    /// <summary>
    /// 静态类依赖分析 构建depedencyMap
    /// </summary>
    private void StaticClassDependencyAnalyze(SyntaxNodeAnalysisContext context, ConcurrentDictionary<string, Dictionary<string,List<InvocationExpressionSyntax>>> dependencyMap,
        HashSet<string> staticClassSet)
    {
           
    }

    /// <summary>
    /// 环形依赖分析
    /// </summary>
    private void CircularDependencyAnalyze(CompilationAnalysisContext context, ConcurrentDictionary<string, Dictionary<string,List<InvocationExpressionSyntax>>> dependencyMap,
        HashSet<string> staticClassSet)
    {
            
            
    }
        
}*/