using GroqApiLibrary;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

class Program
{
    static List<JObject> myHistoryChat = new List<JObject>();

    static void Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        string groqAiKey = config.GetSection("GROQ_API_KEY").Value;
        GroqApiLibrary.GroqAPI groqApi = new GroqApiLibrary.GroqAPI(groqAiKey);

        while (true)
        {
            string userInput = Console.ReadLine();
            myHistoryChat.Add(new JObject
            {
                ["role"] = "user",
                ["content"] = userInput
            });

            
            int maxMessagesSize = 8;
            int messagesToRemoveCount = Math.Max(0, myHistoryChat.Count - maxMessagesSize);
            myHistoryChat.RemoveRange(0, messagesToRemoveCount);

            JObject response = GenerateAIResponce(groqApi).Result;
            string? aiResponse = response?["choices"]?[0]?["message"]?["content"]?.ToString();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(aiResponse);
            Console.ForegroundColor = ConsoleColor.White;

            myHistoryChat.Add(new JObject
            {
                ["role"] = "assistant",
                ["content"] = aiResponse
            });
        }
    }

    static async Task<JObject> GenerateAIResponce(GroqAPI anApi)
    {
        JArray totalChatJArray = new JArray();

        foreach (var chat in myHistoryChat)
        {
            totalChatJArray.Add(chat);
        }

        JObject request = new JObject
        {
            ["model"] = "llama-3.1-8b-instant",
            ["messages"] = totalChatJArray
        };

        var result = await anApi.CreateChatCompletionAsync(request);
        return result;
    }
}