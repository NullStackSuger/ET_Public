using ImGuiNET;

namespace ET.Editor;

[AFileDetail(".txt")]
[AFileDetail(".json")]
[AFileDetail(".xml")]
[AFileDetail(".cs")]
[AFileDetail(".md")]
[AFileDetail(".log")]
// 可以自己添
// 文本类型, 会读取并显示
public class TxtType_FileDetailHandler : AFileDetailHandler
{
    public override void Draw(DetailEditorComponent self, string filePath)
    {
        // 重新读取
        if (self.needRebuild)
        {
            var fileInfo = new FileInfo(filePath);
            // 文件太大不读取
            if (fileInfo.Length > MaxSize)
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[MaxSize];
                int read = fs.Read(buffer, 0, MaxSize);
                string content = System.Text.Encoding.UTF8.GetString(buffer, 0, read);
                self.txtContentTmp = content;
                ImGui.TextWrapped(content + "\n...(Content is too large)");
            }
            else
            {
                string content = File.ReadAllText(filePath);
                self.txtContentTmp = content;
                ImGui.TextWrapped(content);
            }
        }
        // 用缓存
        else
        {
            ImGui.TextWrapped(self.txtContentTmp);
        }
    }

    private const int MaxSize = 10 * 1024;
}