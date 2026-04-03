using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using TMPro;

/// <summary>
/// Přiřaď na každý z 6 slotů dedukční desky.
/// Přijímá drop z knihovny i z jiného slotu. Umožňuje tahat indicii zpět.
/// clueItemPrefab = stejný prefab jako v BadatelnaUI (pro drag proxy).
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ClueSlotUI : MonoBehaviour,
    IDropHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler
{
    [SerializeField] public  int         slotIndex;
    [SerializeField] private GameObject  clueItemPrefab; // Stejný prefab jako v BadatelnaUI
    [SerializeField] private Image       iconImage;
    [SerializeField] private TMP_Text    nameText;
    [SerializeField] private GameObject  emptyState;
    [SerializeField] private GameObject  filledState;

    public ClueData CurrentClue  { get; private set; }
    public ClueData DraggingClue { get; private set; }

    private BadatelnaUI _badatelna;
    private Canvas      _rootCanvas;
    private CanvasGroup _canvasGroup;
    private GameObject  _dragProxy;

    // -------------------------------------------------------------------------

    public void Initialize(BadatelnaUI badatelna, Canvas rootCanvas)
    {
        _badatelna   = badatelna;
        _rootCanvas  = rootCanvas;
        _canvasGroup = GetComponent<CanvasGroup>();
        UpdateVisual();
    }

    public void SetClue(ClueData data)
    {
        CurrentClue = data;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        bool has = CurrentClue != null;
        if (emptyState  != null) emptyState .SetActive(!has);
        if (filledState != null) filledState.SetActive( has);
        if (has)
        {
            if (iconImage != null) iconImage.sprite = CurrentClue.icon;
            if (nameText  != null) nameText.text    = LocalizationSettings.StringDatabase.GetLocalizedString("CluesTable", CurrentClue.clueNameKey);
        }
    }

    // -------------------------------------------------------------------------
    // Drop — přijímá z knihovny nebo z jiného slotu
    // -------------------------------------------------------------------------

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // Z knihovny → do tohoto slotu
        if (eventData.pointerDrag.TryGetComponent<ClueItemUI>(out var libItem))
        {
            Debug.Log($"[ClueSlotUI] Drop přijat ze knihovny: {libItem.ClueData?.clueID} → slot {slotIndex}");
            _badatelna.HandleDropOnSlot(slotIndex, libItem.ClueData, -1, null);
            return;
        }

        // Z jiného slotu → swap
        if (eventData.pointerDrag.TryGetComponent<ClueSlotUI>(out var fromSlot))
        {
            if (fromSlot.DraggingClue == null) return;

            Debug.Log($"[ClueSlotUI] Drop přijat ze slotu {fromSlot.slotIndex}: {fromSlot.DraggingClue.clueID} → slot {slotIndex}");
            ClueData displaced = CurrentClue;
            _badatelna.HandleDropOnSlot(slotIndex, fromSlot.DraggingClue, fromSlot.slotIndex, displaced);
        }
    }

    // -------------------------------------------------------------------------
    // Drag — taháme indicii ze slotu
    // -------------------------------------------------------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CurrentClue == null) return;

        DraggingClue = CurrentClue;
        SetClue(null); // Slot vypadá prázdný

        if (_rootCanvas == null) { Debug.LogWarning("[ClueSlotUI] rootCanvas není přiřazen."); return; }
        if (clueItemPrefab == null) { Debug.LogWarning("[ClueSlotUI] clueItemPrefab není přiřazen."); return; }

        // Proxy = ClueItemPrefab, ne vizuál slotu
        _dragProxy = Instantiate(clueItemPrefab, _rootCanvas.transform);

        // Nastav vizuál proxy na taženou indicii
        var proxyItem = _dragProxy.GetComponent<ClueItemUI>();
        if (proxyItem != null)
        {
            proxyItem.SetVisuals(DraggingClue);
            proxyItem.enabled = false; // Deaktivuj logiku — jen vizuál
        }

        // Fixní velikost z prefabu
        var proxyRect  = _dragProxy.GetComponent<RectTransform>();
        var prefabRect = clueItemPrefab.GetComponent<RectTransform>();
        if (proxyRect != null && prefabRect != null)
            proxyRect.sizeDelta = prefabRect.sizeDelta;

        var pg = _dragProxy.GetComponent<CanvasGroup>();
        if (pg == null) pg = _dragProxy.AddComponent<CanvasGroup>();
        pg.blocksRaycasts = false;
        pg.alpha          = 1f;

        _badatelna.OnSlotDragBegan(slotIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragProxy == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _dragProxy.GetComponent<RectTransform>().localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dragProxy != null)
        {
            Destroy(_dragProxy);
            _dragProxy = null;
        }

        _badatelna.OnSlotDragEnded(slotIndex, DraggingClue);
        DraggingClue = null;
    }

    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && CurrentClue != null)
            _badatelna?.ShowClueDetail(CurrentClue);
    }
}
