using System.Globalization;
using System.Xml.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ET.Luban;

internal static class NumericGenerator
{
    public static void Create()
    {
        // TODO 数值系统/正常会有多张Numeric表分布在各处, 需要找到所有表合并再生成, 这里懒得做了
        var numericTypes = LoadNumericType();
        CreateNumericTypeXml(numericTypes);
        CreateNumericComponent(numericTypes);
        
        LoadNumericAffect();
        CreateNumericAffectXlsx();
        CreateNumericAffectHandler();
    }
    
    static string GetAsString(this ICell cell)
    {
        if (cell == null) return "";
        
        switch (cell.CellType)
        {
            case CellType.String:
                return cell.StringCellValue ?? "";
            case CellType.Numeric:
                return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
            case CellType.Boolean:
                return cell.BooleanCellValue.ToString();
            default:
                return "";
        }
    }
    static bool GetAsBool(this ICell cell)
    {
        if (cell == null) return false;
        
        switch (cell.CellType)
        {
            case CellType.Boolean:
                return cell.BooleanCellValue;
            case CellType.String:
                string str = cell.StringCellValue?.ToLower();
                return str == "1" || str == "true" || str == "True" || str == "TRUE";
            case CellType.Numeric:
                return cell.NumericCellValue != 0;
            default:
                return false;
        }
    }

    static readonly DoubleMap<int, string> types = new();
    static readonly MultiMap<string, NumericAffectData> affects = new();

    static List<NumericTypeData> LoadNumericType()
    {
        var numericTypes = new List<NumericTypeData>();
        var allId = new HashSet<int>();
        var allName = new HashSet<string>();
        
        using var book = new XSSFWorkbook(new FileStream(PathHelper.NumericTypeInPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        var sheet = book.GetSheetAt(0);

        for (int i = 1; i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            if (row == null) continue;
            
            // Id
            string idCell = row.GetCell(0).GetAsString();
            if ("" == idCell) continue;
            if (!int.TryParse(idCell, out int id))
            {
                Console.WriteLine($"ID{id}错误");
                continue;
            }
            if (allId.Contains(id))
            {
                Console.WriteLine($"ID{id}重复");
                continue;
            }
            
            // Name
            string name = row.GetCell(1).GetAsString();
            if (allName.Contains(name))
            {
                Console.WriteLine($"名字{name}重复");
                continue;
            }
            
            // Desc
            string desc = row.GetCell(2).GetAsString();
            
            // IsGrow
            bool isGrow = row.GetCell(3).GetAsBool();
            
            allId.Add(id);
            allName.Add(name);
            
            var data = new NumericTypeData
            {
                id = id,
                name = name,
                desc = desc,
                isGrow = isGrow,
            };
            numericTypes.Add(data);
        }
        
        return numericTypes;
    }

    static void CreateNumericTypeXml(List<NumericTypeData> numericTypes)
    {
        var root = new XElement("module", new XAttribute("name", ""));
        var enumElement = new XElement("enum", new XAttribute("name", "NumericType"), new XAttribute("group", "cs"));

        foreach (var numericType in numericTypes)
        {
            // 添加注释
            if (!string.IsNullOrEmpty(numericType.desc) && numericType.desc != "")
            {
                enumElement.Add(new XComment($" {numericType.desc} "));
            }
            
            // 生成主值
            var mainVar = new XElement("var", new XAttribute("name", numericType.name), new XAttribute("value", numericType.id.ToString()));
            enumElement.Add(mainVar);
            types.Add(numericType.id, numericType.name);
            
            // 生成成长值
            if (numericType.isGrow)
            {
                for (int i = 1; i <= 6; i++)
                {
                    var growName = numericType.name + i;
                    var growValue = numericType.id * 10 + i;

                    var growVar = new XElement("var", new XAttribute("name", growName), new XAttribute("value", growValue.ToString()));
                    enumElement.Add(growVar);
                    types.Add(growValue, growName);
                }
            }
        }
        
        root.Add(enumElement);
        var xmlDoc = new XDocument(root);
        xmlDoc.Save(PathHelper.NumericTypeOutPath);
    }

    static void CreateNumericComponent(List<NumericTypeData> numericTypes)
    {
        var sb = new System.Text.StringBuilder();
        
        // 收集所有isGrow为true的数值类型ID
        var growIds = new HashSet<int>();
        foreach (var numericType in numericTypes)
        {
            if (numericType.isGrow)
            {
                growIds.Add(numericType.id);
            }
        }
        
        // 生成代码模板
        const string TemplateNumericComponent = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ET;

public partial class NumericComponent
{
    public static List<int> IsGrow = [${GrowIds}];
}
";
        
        // 替换模板中的占位符
        string growIdsStr = string.Join(", ", growIds);
        string finalCode = TemplateNumericComponent.Replace("${GrowIds}", growIdsStr);
        
        // 写入文件
        File.WriteAllText(PathHelper.NumericComponentOutPath, finalCode);
    }
    
    static List<NumericAffectData> LoadNumericAffect()
    {
        var numericAffects = new List<NumericAffectData>();

        using var book = new XSSFWorkbook(new FileStream(PathHelper.NumericAffectInPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        var sheet = book.GetSheetAt(0);
            
        for (int i = 1; i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            if (row == null) continue;

            // Name
            string name = row.GetCell(0).GetAsString();
            if (string.IsNullOrEmpty(name)) continue;

            // Affect
            string affect = row.GetCell(1).GetAsString();
            if (string.IsNullOrEmpty(affect)) continue;

            // Formula
            string formula = row.GetCell(2).GetAsString();
                
            // Desc
            string desc = row.GetCell(3).GetAsString();

            var data = new NumericAffectData
            {
                name = name,
                affect = affect,
                formula = formula,
                desc = desc
            };
            numericAffects.Add(data);
            affects.Add(name, data);
        }

        return numericAffects;
    }

    static void CreateNumericAffectXlsx()
    {
        using var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("NumericAffect");
        
        // 创建标题行
        var headerRow = sheet.CreateRow(0);
        headerRow.CreateCell(0).SetCellValue("##var");
        headerRow.CreateCell(1).SetCellValue("Id");
        headerRow.CreateCell(2).SetCellValue("Affects");
        
        // 创建类型行
        var typeRow = sheet.CreateRow(1);
        typeRow.CreateCell(0).SetCellValue("##type");
        typeRow.CreateCell(1).SetCellValue("int");
        typeRow.CreateCell(2).SetCellValue("(array#sep=,),int");
        
        // 创建分组行
        var groupRow = sheet.CreateRow(2);
        groupRow.CreateCell(0).SetCellValue("##group");
        groupRow.CreateCell(1).SetCellValue("cs");
        groupRow.CreateCell(2).SetCellValue("cs");
        
        // 设置列宽
        sheet.SetColumnWidth(0, 15 * 256);
        sheet.SetColumnWidth(1, 15 * 256);
        sheet.SetColumnWidth(2, 15 * 256);
        
        int rowIndex = 3;
        List<int> affectIds = new List<int>();
        foreach ((string name, List<NumericAffectData> datas) in affects)
        {
            // 获取影响者的ID
            int id = types.GetKeyByValue(name);
            if (id == default)
            {
                Console.WriteLine($"'{name}'不存在");
                continue;
            }
            
            // 收集所有被影响者的ID
            affectIds.Clear();
            foreach (NumericAffectData data in datas)
            {
                int affectId = types.GetKeyByValue(data.affect);
                if (affectId == default)
                {
                    Console.WriteLine($"'{data.affect}'不存在");
                    continue;
                }
                affectIds.Add(affectId);
            }
            
            if (affectIds.Count > 0)
            {
                var row = sheet.CreateRow(rowIndex++);
                
                // Id列 (B列)
                row.CreateCell(1).SetCellValue(id);
                
                // Affects列 (C列) - 使用数组格式
                row.CreateCell(2).SetCellValue(string.Join(",", affectIds));
            }
        }
        
        using var fs = new FileStream(PathHelper.NumericAffectOutPath, FileMode.Create);
        workbook.Write(fs);
    }

    static void CreateNumericAffectHandler()
    {
        var uniqueIds = new HashSet<long>();
        var sb = new System.Text.StringBuilder();

        // 模板
        const string TemplateAffectInvoke = @"
[Invoke(${InvokeType})]
public class NumericAffectHandler_${InvokeType} : AInvokeHandler<NumericAffect, long>
{
    /*
    ${Desc}
    */
    public override long Handle(NumericAffect A)
    {
        ${Formula}
    }
}
";
        const string TemplateAffectCS = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;

namespace ET;

${AffectBody}
";
        
        foreach ((string name, List<NumericAffectData> affects) in affects)
        {
            int id = types.GetKeyByValue(name);
            if (id == default)
            {
                Console.WriteLine($"类型'{name}'不存在");
                continue;
            }
            
            foreach (NumericAffectData affect in affects)
            {
                int affectId = types.GetKeyByValue(affect.affect);
                if (affectId == default)
                {
                    Console.WriteLine($"类型'{affect.name}'不存在");
                    continue;
                }
                
                // 生成唯一ID
                long uniqueId = GenerateUniqueId(id, affectId);
                if (uniqueId == 0) continue;
                if (!uniqueIds.Add(uniqueId))
                {
                    Console.WriteLine($"UniqueId重复 请检查 [{name}],[{affect.name}] ");
                    continue;
                }
                
                // 处理描述格式
                string desc = affect.desc?.Replace("\n", "\n    ") ?? "";
                // 处理公式缩进
                string formattedFormula = affect.formula.Replace("\n", "\n        ");
                
                string templateStr = TemplateAffectInvoke
                    .Replace("${InvokeType}", uniqueId.ToString())
                    .Replace("${NumericType}", $"{name}({id})")
                    .Replace("${AffectType}", $"{affect.affect}({affectId})")
                    .Replace("${Desc}", desc)
                    .Replace("${Formula}", formattedFormula);
                sb.Append(templateStr);
            }
        }
        // 写入文件
        string finalCode = TemplateAffectCS.Replace("${AffectBody}", sb.ToString());
        File.WriteAllText(PathHelper.NumericHandlerOutPath, finalCode);
    }

    private static long GenerateUniqueId(int value1, int value2)
    {
        if (value1 <= 0 || value2 <= 0)
        {
            Console.WriteLine($"生成唯一ID失败: 值必须大于0 请检查: {value1}, {value2}");
            return 0;
        }
        return ((long)value1 << 32) | (value2 & 0xFFFFFFFFL);
    }

    class NumericTypeData
    {
        public int id;
        public string name;
        public string desc;
        public bool isGrow;
    }

    class NumericAffectData
    {
        public string name;
        public string affect;
        public string formula;
        public string desc;
    }
}