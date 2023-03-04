using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading;
using Cysharp.Threading.Tasks;

// https://platform.openai.com/docs/api-reference/chat/create
public class OpenAIChatCompletionAPI
{
    const string API_URL = "https://api.openai.com/v1/chat/completions";
    string apiKey;
    JsonSerializerSettings settings = new JsonSerializerSettings();

    public OpenAIChatCompletionAPI(string apiKey)
    {
        this.apiKey = apiKey;
        settings.NullValueHandling = NullValueHandling.Ignore;
    }

    public async UniTask<ResponseData> CreateCompletionRequest(RequestData requestData, CancellationToken cancellationToken)
    {
        var json = JsonConvert.SerializeObject(requestData, settings);

        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);

        using (var request = new UnityWebRequest(API_URL, "POST"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();

            await request.SendWebRequest().WithCancellation(cancellationToken);
            return JsonConvert.DeserializeObject<ResponseData>(request.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class RequestData
    {
        public string model = "gpt-3.5-turbo";
        public List<Message> messages;
        public float? temperature = null; // [0.0 - 2.0]
        public float? top_p = null;
        public int? n = null;
        public bool? stream = null;
        public List<string> stop = null;
        public int? max_tokens = null;
        public float? presence_penalty = null;
        public float? frequency_penalty = null;
        public Dictionary<int, int> logit_bias = null;
        public string user = null;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
        public string finish_reason;
        public int index;
    }

    [System.Serializable]
    public class ResponseData
    {
        public string id;
        public string @object;
        public int created;
        public string model;
        public Usage usage;
        public List<Choice> choices;
    }
}
