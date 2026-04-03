using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using TMPro;

/// <summary>
/// Přiřaď na prefab indicie v knihovně.
/// Obsluhuje vizuál, tažení do slotů a right-click pro detail.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ClueItemUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler
{
    [SerializeField] private Image    iconImage;
    [SerializeField] private TMP_Text nameText;

    public ClueData ClueData { get; private set; }

    private BadatelnaUI _badatelna;
    private Canvas      _rootCanvas;
    private CanvasGroup _canvasGroup;
    private GameObject  _dragProxy;

    // -------------------------------------------------------------------------

    public void Initialize(ClueData data, BadatelnaUI badatelna, Canvas rootCanvas)
    {
        ClueData     = data;
        _badatelna   = badatelna;
        _rootCanvas  = rootCanvas;
        _canvasGroup = GetComponent<CanvasGroup>();

        SetVisuals(data);
    }

    /// <summary>
    /// Nastaví ikonu a název podle ClueData. Volitelně z proxy bez plné inicializace.
    /// </summary>
    public void SetVisuals(ClueData data)
    {
        if (data == null) return;
        if (iconImage != null) iconImage.sprite = data.icon;
        if (nameText  != null) nameText.text    = LocalizationSettings.StringDatabase.GetLocalizedString("CluesTable", data.clueNameKey);
    }

    /// <summary>
    /// Alpha 0 + blocksRaycasts false = indicie je v slotu (neviditelná v knihovně).
    /// Alpha 1 + blocksRaycasts true  = indicie je volná.
    /// </summary>
    public void SetUsedVisual(bool isUsed)
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha          = isUsed ? 0f : 1f;
        _canvasGroup.blocksRaycasts = !isUsed;
    }

    // -------------------------------------------------------------------------
    // Drag
    // -------------------------------------------------------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_rootCanvas == null) { Debug.LogWarning("[ClueItemUI] rootCanvas není přiřazen."); return; }

        _dragProxy = Instantiate(gameObject, _rootCanvas.transform);

        // Fixní velikost stejná jako originál
        var proxyRect    = _dragProxy.GetComponent<RectTransform>();
        var originalRect = GetComponent<RectTransform>();
        if (proxyRect != null && originalRect != null)
            proxyRect.sizeDelta = originalRect.sizeDelta;

        // Proxy nesmí zachytávat raycasty
        var pg = _dragProxy.GetComponent<CanvasGroup>();
        if (pg == null) pg = _dragProxy.AddComponent<CanvasGroup>();
        pg.blocksRaycasts = false;
        pg.alpha          = 1f;

        // Deaktivuj logiku na proxy — jen vizuál
        var proxyItem = _dragProxy.GetComponent<ClueItemUI>();
        if (proxyItem != null) proxyItem.enabled = false;

        // Skryj originál v knihovně
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha          = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        // Okamžitě sniž počet na záložce — indicie vizuálně opouští knihovnu
        _badatelna?.OnLibraryDragBegan(ClueData);
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

        // Obnov stav originálu — RefreshLibraryVisuals rozhodne finální viditelnost
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha          = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        // Zruš drag flag před finálním přepočtem
        _badatelna?.OnLibraryDragEnded();
        _badatelna?.RefreshLibraryVisuals();
    }

    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            _badatelna?.ShowClueDetail(ClueData);
    }
}
