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
        var message = new OpenAIChatCompletionAPI.Message() { role = "user", content = inputField.text };
        AppendMessage(message);
        inputField.text = "";

        ChatCompletionRequest().Forget();
    }

    async UniTask ChatCompletionRequest()
    {
        sendButton.interactable = false;

        var cancellationToken = this.GetCancellationTokenOnDestroy();

        await UniTask.DelayFrame(1, cancellationToken:cancellationToken);
        scrollRect.verticalNormalizedPosition = 0;

        var response = await chatCompletionAPI.CreateCompletionRequest(
            new OpenAIChatCompletionAPI.RequestData() { messages = context },
            cancellationToken
        );

        var message = response.choices[0].message;
        AppendMessage(message);

        await UniTask.DelayFrame(1, cancellationToken:cancellationToken);
        scrollRect.verticalNormalizedPosition = 0;

        sendButton.interactable = true;
    }

    void AppendMessage(OpenAIChatCompletionAPI.Message message)
    {
        context.Add(message);

        var messageView = Instantiate(messageViewTemplete);
        messageView.gameObject.name = "message";
        messageView.gameObject.SetActive(true);
        messageView.transform.SetParent(messageViewTemplete.transform.parent, false);
        messageView.Role = message.role;
        messageView.Content = message.content;
    }
}
