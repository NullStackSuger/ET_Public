using System.Collections;
using System.Numerics;
using System.Reflection;
using ImGuiNET;

namespace ET.Editor;

public static class Default_ComponentDetailHandler
{
    private static bool DrawFloat(string name, ref float value)
    {
        return ImGui.DragFloat(name, ref value);
    }
    private static bool DrawInt(string name, ref int value)
    {
        return ImGui.DragInt(name, ref value);
    }
    private static bool DrawBool(string name, ref bool value)
    {
        return ImGui.Checkbox(name, ref value);
    }
    private static bool DrawString(string name, ref string value)
    {
        byte[] rowValue = System.Text.Encoding.UTF8.GetBytes(value);
        byte[] newValue = new byte[64];
        int length = Math.Min(rowValue.Length, newValue.Length - 1);
        Array.Copy(rowValue, newValue, length);
        newValue[length] = 0; // null-terminated

        if (ImGui.InputText(name, newValue, (uint)newValue.Length))
        {
            int strLen = Array.IndexOf(newValue, (byte)0);
            if (strLen < 0) strLen = newValue.Length;
            
            value = System.Text.Encoding.UTF8.GetString(newValue, 0, strLen);
            return true;
        }
        
        return false;
    }
    
    private static bool DrawVector2(string name, ref Vector2 value)
    {
        return ImGui.DragFloat2(name, ref value);
    }
    private static bool DrawVector3(string name, ref Vector3 value)
    {
        return ImGui.DragFloat3(name, ref value);
    }
    private static bool DrawVector4(string name, ref Vector4 value)
    {
        return ImGui.DragFloat4(name, ref value);
    }
    private static bool DrawQuaternion(string name, ref Quaternion value)
    {
        Vector3 rot = value.ToVector3();
        if (ImGui.DragFloat3(name, ref rot))
        {
            value = rot.ToQuaternion();
            return true;
        }
        
        return false;
    }
    
    private static bool DrawEnum(string name, Type type, ref int value)
    {
        // enum所有名字
        string[] enumNames = Enum.GetNames(type);

        return ImGui.Combo(name, ref value, enumNames, enumNames.Length);
    }
    
    private static void DrawCustom(ref object obj)
    {
        // 反射遍历所有public属性，按类型渲染
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (FieldInfo field in fields)
        {
            // 跳过 static 和 const 字段
            if (field.IsStatic || field.IsLiteral) continue;

            object item = field.GetValue(obj);
            
            if (Handler(field.Name, ref item))
            {
                field.SetValue(obj, item);
            }
        }
    }
    
    public static void DrawEntity(Entity entity)
    {
        object obj = entity;
        DrawCustom(ref obj);
    }

    public static List<string> FindShader(string targetDir)
    {
        var result = new List<string>();
        if (!System.IO.Directory.Exists(targetDir))
            return result;
        var files = System.IO.Directory.GetFiles(targetDir, "*.shader", System.IO.SearchOption.TopDirectoryOnly);
        result.AddRange(files);
        return result;
    }

    static bool Handler(string name, ref object obj)
    {
        if (obj == null)
        {
            ImGui.Text($"{name}: null");
            return false;
        }
        else if (obj is float f)
        {
            if (DrawFloat(name, ref f))
            {
                obj = f;
                return true;
            }
        }
        else if (obj is int i)
        {
            if (DrawInt(name, ref i))
            {
                obj = i;
                return true;
            }
        }
        else if (obj is bool b)
        {
            if (DrawBool(name, ref b))
            {
                obj = b;
                return true;
            }
        }
        else if (obj is string s)
        {
            if (DrawString(name, ref s))
            {
                obj = s;
                return true;
            }
        }
        else if (obj is Vector2 v2)
        {
            if (DrawVector2(name, ref v2))
            {
                obj = v2;
                return true;
            }
        }
        else if (obj is Vector3 v3)
        {
            if (DrawVector3(name, ref v3))
            {
                obj = v3;
                return true;
            }
        }
        else if (obj is Vector4 v4)
        {
            if (DrawVector4(name, ref v4))
            {
                obj = v4;
                return true;
            }
        }
        else if (obj is Quaternion q)
        {
            if (DrawQuaternion(name, ref q))
            {
                obj = q;
                return true;
            }
        }
        else if (obj is Enum e)
        {
            int index = Convert.ToInt32(e);
            if (DrawEnum(name, obj.GetType(), ref index))
            {
                obj = Enum.ToObject(obj.GetType(), index);
                return true;
            }
        }
        else if (obj is Array arr)
        {
            Type elementType = arr.GetType().GetElementType();
            if (ImGui.TreeNode($"{name} [<{elementType}> {arr.Length}]"))
            {
                for (int j = 0; j < arr.Length; ++j)
                {
                    object item = arr.GetValue(j);
                    
                    if (Handler($"Item{j}", ref item))
                    {
                        arr.SetValue(item, j);
                    }
                }
                ImGui.TreePop();
            }
        }
        else if (obj is IList list)
        {
            Type elementType = list.GetType().GetGenericArguments()[0];

            bool needOpen = ImGui.TreeNode($"{name} [<{elementType}> {list.Count}]");
            
            ImGui.SameLine();
            
            ImGui.PushItemWidth(40);
            string countStr = list.Count.ToString();
            if (ImGui.InputText("##Count", ref countStr, 4, ImGuiInputTextFlags.CharsDecimal))
            {
                if (!int.TryParse(countStr, out int count) || count < 0)
                {
                    
                }
                else if (count > list.Count)
                {
                    for (int k = list.Count; k < count; ++k)
                    {
                        list.Add(elementType.IsValueType ? Activator.CreateInstance(elementType) : null);
                    }
                }
                else if (count < list.Count)
                {
                    for (int k = list.Count - 1; k >= count; --k)
                    {
                        list.RemoveAt(k);
                    }
                }
            }
            ImGui.PopItemWidth();
            
            if (needOpen)
            {
                for (int j = 0; j < list.Count; ++j)
                {
                    object item = list[j];
                    
                    if (Handler($"Item{j}", ref item))
                    {
                        list[j] = item;
                    }
                }
                ImGui.TreePop();
            }
        }
        // struct
        else if (obj.GetType().IsValueType /*&& !obj.GetType().IsPrimitive && !obj.GetType().IsEnum*/)
        {
            DrawCustom(ref obj);
        }
        // class
        else if (obj.GetType().IsClass /*&& obj.GetType() != typeof(string) && !obj.GetType().IsArray*/)
        {
            DrawCustom(ref obj);
        }
        
        return false;
    }
}