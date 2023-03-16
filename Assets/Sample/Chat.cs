using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class Chat : MonoBehaviour
{
    [SerializeField] ChatMessageView messageViewTemplete;
    [SerializeField] InputField inputField;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Button sendButton;
    [SerializeField] bool stream = true;

    [SerializeField] string apiKey; // NOTE: 入力したままコミットやリポジトリの公開などをしないこと

    OpenAIChatCompletionAPI chatCompletionAPI;

    List<OpenAIChatCompletionAPI.Message> context = new List<OpenAIChatCompletionAPI.Message>()
    {
        new OpenAIChatCompletionAPI.Message(){role = "system", content = "あなたは優秀なAIアシスタントです。"},
    };

    void Awake()
    {
        messageViewTemplete.gameObject.SetActive(false);
        sendButton.onClick.AddListener(OnSendClick);
        chatCompletionAPI = new OpenAIChatCompletionAPI(apiKey);
    }

    void OnSendClick()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;
        var message = new OpenAIChatCompletionAPI.Message()
        {
            role = "user",
            content = inputField.text,
        };

        context.Add(message);
        var view = CreatedMessage();
        view.Role = message.role;
        view.Content = message.content;
        inputField.text = "";

        ChatCompletionRequest(stream).Forget();
    }

    async UniTask ChatCompletionRequest(bool stream)
    {
        sendButton.interactable = false;

        var cancellationToken = this.GetCancellationTokenOnDestroy();

        await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
        scrollRect.verticalNormalizedPosition = 0;

        var requestData = new OpenAIChatCompletionAPI.RequestData()
        {
            messages = context,
            stream = stream
        };

        if (stream)
        {
            ChatMessageView view = null;
            var message = new OpenAIChatCompletionAPI.Message();

            await foreach (var chunk in chatCompletionAPI.CreateCompletionRequestAsStream(requestData, cancellationToken))
            {
                if (view == null) view = CreatedMessage();

                string role = chunk.choices[0].delta.role;

                if (role != null)
                {
                    message.role = role;
                }
                message.content += chunk.choices[0].delta.content;

                view.Role = message.role;
                view.Content = message.content;
                scrollRect.verticalNormalizedPosition = 0;
            }

            context.Add(message);
        }
        else
        {
            var response = await chatCompletionAPI.CreateCompletionRequest(requestData, cancellationToken);
            var view = CreatedMessage();
            var message = response.choices[0].message;

            context.Add(message);

            view.Role = message.role;
            view.Content = message.content;
        }

        await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
        scrollRect.verticalNormalizedPosition = 0;

        sendButton.interactable = true;
    }

    ChatMessageView CreatedMessage()
    {
        var messageView = Instantiate(messageViewTemplete);
        messageView.gameObject.name = "message";
        messageView.gameObject.SetActive(true);
        messageView.transform.SetParent(messageViewTemplete.transform.parent, false);
        return messageView;
    }
}
