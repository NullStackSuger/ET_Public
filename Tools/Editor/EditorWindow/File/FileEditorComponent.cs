using System.Diagnostics;
using ImGuiNET;

namespace ET.Editor;

[EntitySystemOf(typeof(FileEditorComponent))]
public static partial class FileEditorComponentSystem
{
    [EntitySystem]
    private static void Awake(this FileEditorComponent self, string[] a)
    {
        self.ignores = a ?? [];
            
        self.Build();
    }

    [EntitySystem]
    private static void Ui(this FileEditorComponent self)
    {
        // 打开ImGui窗口
        ImGui.Begin(nameof(FileEditorComponent), ImGuiWindowFlags.MenuBar);

        if (ImGui.BeginPopupContextWindow("FilePanelContextMenu", ImGuiPopupFlags.MouseButtonRight))
        {
            if (ImGui.MenuItem("Build"))
            {
                self.Build();
            }
            ImGui.EndPopup();
        }
            
        self.Draw(PathHelper.Asset);

        ImGui.End();
    }
    
    private static void Draw(this FileEditorComponent self, string dirPath)
    {
        try
        {
            // 获取当前目录下所有子目录
            string[] dirs = Directory.GetDirectories(dirPath);
            foreach (string dir in dirs)
            {
                
                string dirName = Path.GetFileName(dir);

                bool needOpen = ImGui.TreeNode($"{dirName}##{dir}"); // 文件夹可以同名

                if (ImGui.BeginPopupContextItem(dirName))
                {
                    if (ImGui.MenuItem("Open"))
                    {
                        Process.Start(new ProcessStartInfo() { FileName = dir, UseShellExecute = true });
                    }
                    ImGui.EndPopup();
                }
                
                if (needOpen)
                {
                    self.Draw(dir);
                    ImGui.TreePop();
                }
            }

            // 获取当前目录下所有文件
            string[] files = Directory.GetFiles(dirPath);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string ext = Path.GetExtension(fileName);
                
                // 判断文件后缀是否在ignores中，若在则跳过
                if (Array.Exists(self.ignores, ig => ig.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                ImGui.TreeNodeEx(fileName, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);

                if (ImGui.IsItemClicked())
                {
                    EventSystem.Instance.Publish(self.Scene(), new ClickFileItem() { path = file, name = fileName, extension = ext });
                }

                if (ImGui.BeginPopupContextItem(fileName))
                {
                    if (ImGui.MenuItem("Open"))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo() { FileName = file, UseShellExecute = true });
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "notepad.exe",
                                Arguments = $"\"{Path.GetFullPath(file)}\"",
                                UseShellExecute = false
                            });
                        }
                    }
                    ImGui.EndPopup();
                }
            }
        }
        catch (Exception e)
        {
            ImGui.Text($"[错误] 访问目录失败: {dirPath} ({e.Message})");
            Log.Instance.Error(e);
        }
    }

    // 和Draw很像, 一个带Ui渲染, 一个不带
    private static void Build(this FileEditorComponent self, string dirPath = PathHelper.Asset)
    {
        self.files ??= new ();
        self.files.Clear();
        BuildInner(self, dirPath);
        
        static void BuildInner(FileEditorComponent self, string dirPath)
        {
            try
            {
                // 获取当前目录下所有子目录
                string[] dirs = Directory.GetDirectories(dirPath);
                foreach (string dir in dirs)
                {
                    // 递归子目录
                    BuildInner(self, dir);
                }
            
                // 获取当前目录下所有文件
                string[] files = Directory.GetFiles(dirPath);
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string ext = Path.GetExtension(fileName);
                
                    // 判断文件后缀是否在ignores中，若在则跳过
                    if (Array.Exists(self.ignores, ig => ig.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    if (self.files.TryGetValue(ext, fileName, out string oldFilePath))
                    {
                        Log.Instance.Error($"存在同名的{ext}文件: {oldFilePath} And {Path.GetRelativePath(PathHelper.Asset, file)}");
                        continue;
                    }
                    else
                    {
                        self.files.Add(ext, fileName, Path.GetRelativePath(PathHelper.Asset, file));   
                    }
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }
}

public class FileEditorComponent : Entity, IAwake<string[]>, IUi
{
    public string[] ignores;
    // 后缀, 名字, 路径
    public MultiDictionary<string, string, string> files = new();
}