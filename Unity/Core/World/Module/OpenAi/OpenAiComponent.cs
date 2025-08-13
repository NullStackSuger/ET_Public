using System.Text;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Images;

namespace ET;

[EntitySystemOf(typeof(OpenAiComponent))]
public static partial class OpenAiComponentSystem
{
    [EntitySystem]
    private static void Awake(this OpenAiComponent self)
    {
        self.Awake([]);
    }

    [EntitySystem]
    private static void Awake(this OpenAiComponent self, ChatTool[] tools)
    {
        OpenAIClient client = new(OpenAiComponent.key);
        self.chatClient = client.GetChatClient("gpt-4o-mini");
        self.audioClient = client.GetAudioClient("whisper-1");
        self.imageClient = client.GetImageClient("dall-e-3");
        
        foreach (ChatTool tool in tools)
        {
            self.tools.Add(tool.FunctionName, tool);
        }
    }

    [EntitySystem]
    private static void Update(this OpenAiComponent self)
    {
        self.Handler().NoContext();
    }

    private static async ETTask Handler(this OpenAiComponent self)
    {
        if (!self.queue.TryDequeue(out var item)) return;
        ChatMessage message = item.Item1;
        string[] toolNames = item.Item2;
        ETTask task = item.Item3;
        
        self.requestHistory.Add(message);
        ChatCompletionOptions options = new();
        foreach (string toolName in toolNames)
        {
            if (self.tools.TryGetValue(toolName, out ChatTool tool))
                options.Tools.Add(tool);
        }
        
        StringBuilder sb = new();
        var updates = self.chatClient.CompleteChatStreamingAsync(self.requestHistory);
        await foreach (var update in updates)
        {
            // 更新内容
            if (update.ContentUpdate.Count > 0)
            {
                string append = update.ContentUpdate[0].Text;
                sb.Append(append);
                await EventSystem.Instance.PublishAsync(self.Scene(), new AiResponseUpdate() { append = append });
            }
            
            // 检查完成状态
            if (update.FinishReason.HasValue)
            {
                switch (update.FinishReason.Value)
                {
                    case ChatFinishReason.Stop:
                        break;
                    case ChatFinishReason.ToolCalls:
                        sb.Append("[需要Tools]");
                        break;
                    case ChatFinishReason.Length:
                        sb.Append("[长度达到限制]");
                        break;
                    case ChatFinishReason.ContentFilter:
                        Log.Instance.Error(new Exception($"[内容被过滤]: {sb}"));
                        break;
                }
            }
        }

        string result = sb.ToString();
        self.responseHistory.Add(result);
        await EventSystem.Instance.PublishAsync(self.Scene(), new AiResponseFinish() { message = result });
        task.SetResult();
    }
    
    private static async ETTask<string> Chat(this OpenAiComponent self, ChatMessage input, params string[] toolNames)
    {
        ETTask task = ETTask.Create(true);
        
        self.queue.Enqueue((input, toolNames, task));

        await task;
        
        int index = self.requestHistory.IndexOf(input);
        return index != -1 ? self.responseHistory[index] : "OpenAi No Response!";
    }
    public static async ETTask<string> Chat(this OpenAiComponent self, string message, params string[] toolNames)
    {
        return await self.Chat(new UserChatMessage(ChatMessageContentPart.CreateTextPart(message)), toolNames);
    }
}

public class OpenAiComponent : Entity, IAwake, IAwake<ChatTool[]>, IUpdate
{
    public const string key = "This is a key";
    
    public ChatClient chatClient;
    public AudioClient audioClient;
    public ImageClient imageClient;
    
    public List<ChatMessage> requestHistory = new();
    public List<string> responseHistory = new();
    
    public Dictionary<string, ChatTool> tools = new();
    
    public Queue<(ChatMessage, string[], ETTask)> queue = new();
}