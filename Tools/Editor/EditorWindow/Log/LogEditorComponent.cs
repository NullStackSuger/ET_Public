using ImGuiNET;
using System.Numerics;

namespace ET.Editor;

[EntitySystemOf(typeof(LogEditorComponent))]
public static partial class LogEditorComponentSystem
{
    [EntitySystem]
    private static void Ui(this LogEditorComponent self)
    {
        ImGui.Begin(nameof(LogEditorComponent), ImGuiWindowFlags.MenuBar);

        string levelName = "";
        List<LogInfo> logs = null;

        if (ImGui.BeginMenuBar())
        {
            // Log过滤和数量
            switch (self.editorLogLevel)
            {
                case 0:
                    levelName = "All";
                    break;
                case Log.TraceLevel:
                    levelName = "Trace";
                    break;
                case Log.DebugLevel:
                    levelName = "Debug";
                    break;
                case Log.InfoLevel:
                    levelName = "Info";
                    break;
                case Log.WarningLevel:
                    levelName = "Warning";
                    break;
                case Log.ErrorLevel:
                    levelName = "Error";
                    break;
            }

            if (ImGui.Button(levelName))
            {
                ++self.editorLogLevel;
                self.editorLogLevel %= (Log.ErrorLevel + 1);
                self.selectLog = -1;
            }

            logs = Log.Instance[self.editorLogLevel];
            ImGui.Text($"Count: {logs.Count}");

            // Clear
            // Clear是会下一帧才会清除
            if (ImGui.Button("Clear"))
            {
                Log.Instance.Clear();
            }

            // Input
            ImGui.SetNextItemWidth(120);
            ImGui.InputText("##Rule", ref self.selectRule, 64);

            ImGui.EndMenuBar();
        }

        // Show Logs List
        if (logs != null && logs.Count > 0)
        {
            for (int i = 0; i < logs.Count; ++i)
            {
                LogInfo log = logs[i];

                // 筛选文字
                if (self.selectRule != "" && !log.message.StartsWith(self.selectRule))
                {
                    continue;
                }

                // 渲染Log
                Vector4 color = log.level switch
                {
                    Log.WarningLevel => new Vector4(1f, 1f, 0.2f, 1.0f),
                    Log.ErrorLevel => new Vector4(1f, 0.3f, 0.3f, 1.0f),
                    _ => new Vector4(0.6f, 0.6f, 0.6f, 1.0f)
                };
                ImGui.PushStyleColor(ImGuiCol.Text, color);
                bool isSelected = self.selectLog != -1 && self.selectLog == i;
                if (ImGui.Selectable($"{log.message}##{i}", isSelected))
                {
                    self.selectLog = i;
                }

                ImGui.PopStyleColor();

                // 悬停且被选中时显示堆栈
                if (isSelected && ImGui.IsItemHovered() && log.st != null)
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(log.st.ToString());
                    ImGui.EndTooltip();
                }
                
                // 右键菜单
                if (ImGui.BeginPopupContextItem($"{log.message}##{i}"))
                {
                    if (ImGui.MenuItem("Open"))
                    {
                        if (log.st != null)
                        {
                            var frame = log.st.GetFrame(0)!;
                            string file = frame.GetFileName();
                            int line = frame.GetFileLineNumber();
                            // 注意必须Path里注册了Rider才行
                            System.Diagnostics.Process.Start("rider64.exe", $"--line {line} \"{file}\"");
                        }
                    }
                    else if (ImGui.MenuItem("Copy"))
                    {
                        ImGui.SetClipboardText(log.message);   
                    }
                    else if (ImGui.MenuItem("Copy Stack"))
                    {
                        ImGui.SetClipboardText(log.st?.ToString());
                    }
                    ImGui.EndPopup();
                }
            }
        }

        ImGui.End();
    }
}

public class LogEditorComponent : Entity, IAwake, IUi
{
    public int editorLogLevel;
    public string selectRule = "";
    public int selectLog = -1;
}