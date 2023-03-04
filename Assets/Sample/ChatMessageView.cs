using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageView : MonoBehaviour
{
    [SerializeField] Text role;
    [SerializeField] Text content;

    public string Role { set => role.text = value; }
    public string Content { set => content.text = value; }
}
