using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Přiřaď na GameObject pokrývající oblast knihovny indicií.
/// Přijímá drag ze slotů a vrátí indicii zpět do knihovny.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class LibraryDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private BadatelnaUI badatelna;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || badatelna == null) return;

        if (eventData.pointerDrag.TryGetComponent<ClueSlotUI>(out var slot))
            badatelna.HandleDropOnLibrary(slot.slotIndex, slot.DraggingClue);
    }
}
