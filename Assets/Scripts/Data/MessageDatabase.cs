using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MessageDatabase", menuName = "Blade of the Spade/Message Database")]
public class MessageDatabase : ScriptableObject
{
    public List<MessageData> messages = new List<MessageData>();

    public MessageData GetMessage(string id) =>
        messages.Find(m => m.messageID == id);

    /// <summary>
    /// Vrátí zprávy, které jsou buď odemčené od začátku, nebo je GameStateManager
    /// explicitně zná (byly odemčeny za běhu hry).
    /// </summary>
    public List<MessageData> GetUnlockedMessages(GameStateManager gsm)
    {
        var result = new List<MessageData>();
        foreach (var msg in messages)
        {
            bool knownToSave = gsm != null && gsm.State.messages.ContainsKey(msg.messageID);
            if (msg.isUnlockedByDefault || knownToSave)
                result.Add(msg);
        }
        return result;
    }
}
