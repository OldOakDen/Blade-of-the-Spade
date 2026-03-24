using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewMessage", menuName = "Blade of the Spade/Message")]
public class MessageData : ScriptableObject
{
    [Tooltip("Unikátní ID zprávy – používá se pro ukládání stavu (přečteno/nepřečteno).")]
    public string messageID;

    [Tooltip("Odesílatel – vyber tabulku a klíč v Inspektoru.")]
    public LocalizedString senderName;

    [Tooltip("Předmět zprávy.")]
    public LocalizedString subject;

    [Tooltip("Tělo zprávy.")]
    public LocalizedString body;

    public bool isUnlockedByDefault;

    [Space]
    public bool          requiresAction;

    [Tooltip("Text potvrzovacího tlačítka. Pokud prázdné, použije se 'Potvrzuji'.")]
    public LocalizedString actionButtonLabel;

    [Tooltip("ID akce spuštěné při potvrzení – musí mít záznam v MessageActionHandler.")]
    public string actionID;
}
