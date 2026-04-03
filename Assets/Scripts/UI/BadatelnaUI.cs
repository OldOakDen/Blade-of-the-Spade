using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;

public class BadatelnaUI : MonoBehaviour
{
    // =========================================================================
    // Inspector fields
    // =========================================================================

    [Header("Dependencies")]
    [SerializeField] private ClueDatabase     clueDatabase;
    [SerializeField] private LocationDatabase locationDatabase;
    [SerializeField] private MainGameUI       mainGameUI;
    [SerializeField] private Canvas           rootCanvas;

    [Header("Mapa — záložky")]
    [SerializeField] private MapTabEntry[] mapTabs;
    [SerializeField] private string        currentMapID;

    [Header("Typ indicie — záložky")]
    [SerializeField] private Button  tabAll;
    [SerializeField] private Button  tabDocuments;
    [SerializeField] private Button  tabMaps;
    [SerializeField] private Button  tabNotes;
    [SerializeField] private Button  tabArtifacts;
    [Tooltip("Samostatný TMP_Text child objekt pouze pro číslo na záložce.")]
    [SerializeField] private TMP_Text tabCountAll;
    [SerializeField] private TMP_Text tabCountDocuments;
    [SerializeField] private TMP_Text tabCountMaps;
    [SerializeField] private TMP_Text tabCountNotes;
    [SerializeField] private TMP_Text tabCountArtifacts;

    [Header("Knihovna indicií")]
    [SerializeField] private Transform  clueListContent;
    [SerializeField] private GameObject clueItemPrefab;

    [Header("Dedukční deska — sloty")]
    [SerializeField] private ClueSlotUI[] clueSlots;   // 6 slotů
    [SerializeField] private GameObject   slotsGroup;  // Rodičovský GO všech slotů — skryje se po odeslání hypotézy

    [Header("Panel hypotézy")]
    [SerializeField] private GameObject hypothesisPanel;
    [SerializeField] private TMP_Text   hypothesisTitleText;
    [SerializeField] private TMP_Text   hypothesisDescText;
    [SerializeField] private TMP_Text   hypothesisClueListText;  // Použité indicie jako odřádkovaný text
    [SerializeField] private GameObject hypothesisPendingTextGO; // "Odesláno, zkontroluj příchozí poštu" — skryje se po potvrzení akce

    [Header("Animace návratu indicií")]
    [SerializeField] private float          clueReturnDuration    = 0.4f;
    [SerializeField] private AnimationCurve clueReturnCurve       = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float          returnScatterRadius   = 30f;
    [Tooltip("ScrollView nebo jiný RectTransform reprezentující oblast knihovny — cíl animace.")]
    [SerializeField] private RectTransform  clueLibraryScrollView;

    [Header("Progress bar")]
    [SerializeField] private GameObject progressBarGroup;
    [SerializeField] private Image      progressBarFill;

    [Header("Odeslání hypotézy")]
    [SerializeField] private Button   submitButton;
    [SerializeField] private TMP_Text submitResultText;

    [Header("Detail indicie")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TMP_Text   detailTitleText;
    [SerializeField] private TMP_Text   detailContentText;
    [SerializeField] private Button     detailCloseButton;

    [Header("Seznam lokalit")]
    [SerializeField] private Transform  locationListContent;
    [SerializeField] private GameObject locationButtonPrefab;
    [SerializeField] private TMP_Text   locationNameText;
    [SerializeField] private TMP_Text   locationHintText;
    [SerializeField] private Color      locationActiveColor = new Color(0.6f, 0.85f, 1f);

    [Header("Navigace")]
    [SerializeField] private Button buttonBack;

    // =========================================================================
    // Privátní stav
    // =========================================================================

    private ClueData.ClueType? _activeTypeFilter        = null;
    private LocationData       _currentLocation;
    private bool               _dropHandled;
    private List<ClueItemUI>   _libraryItems            = new List<ClueItemUI>();
    private ClueData           _draggingFromLibraryClue = null; // Indicie právě draggovaná z knihovny
    private Coroutine          _returnCoroutine         = null;
    private List<GameObject>   _returnProxies           = new List<GameObject>(); // Proxy objekty animace návratu

    // =========================================================================
    // Inicializace
    // =========================================================================

    private void Awake()
    {
        // Sloty
        for (int i = 0; i < clueSlots.Length; i++)
        {
            if (clueSlots[i] == null) continue;
            clueSlots[i].slotIndex = i;
            clueSlots[i].Initialize(this, rootCanvas);
        }

        // Záložky typů
        if (tabAll       != null) tabAll      .onClick.AddListener(() => SetTypeFilter(null));
        if (tabDocuments != null) tabDocuments.onClick.AddListener(() => SetTypeFilter(ClueData.ClueType.Document));
        if (tabMaps      != null) tabMaps     .onClick.AddListener(() => SetTypeFilter(ClueData.ClueType.Map));
        if (tabNotes     != null) tabNotes    .onClick.AddListener(() => SetTypeFilter(ClueData.ClueType.Note));
        if (tabArtifacts != null) tabArtifacts.onClick.AddListener(() => SetTypeFilter(ClueData.ClueType.Artifact));

        // Záložky map
        foreach (var entry in mapTabs)
        {
            if (entry.button == null || string.IsNullOrEmpty(entry.mapID)) continue;
            string id = entry.mapID;
            entry.button.onClick.AddListener(() => SwitchMap(id));
        }

        // Ostatní tlačítka
        if (submitButton    != null) submitButton   .onClick.AddListener(SubmitHypothesis);
        if (detailCloseButton != null) detailCloseButton.onClick.AddListener(() => detailPanel.SetActive(false));
        if (buttonBack      != null) buttonBack     .onClick.AddListener(GoBack);

        if (detailPanel       != null) detailPanel      .SetActive(false);
        if (hypothesisPanel   != null) hypothesisPanel  .SetActive(false);
    }

    private void OnEnable()
    {
        if (GameStateManager.Instance == null || locationDatabase == null) return;

        // Nastav výchozí mapID z první záložky pokud není nastaven
        if (string.IsNullOrEmpty(currentMapID) && mapTabs != null && mapTabs.Length > 0)
            currentMapID = mapTabs[0].mapID;

        LoadSlotsFromSave();

        // Obnov poslední aktivní lokalitu, nebo zvol první dostupnou
        LocationData locToSelect = null;

        string savedLocID = GameStateManager.Instance.GetActiveLocation();
        if (!string.IsNullOrEmpty(savedLocID))
            locToSelect = locationDatabase.GetLocation(savedLocID);

        if (locToSelect == null)
        {
            var available = locationDatabase.GetLocationsForMap(currentMapID);
            if (available.Count > 0) locToSelect = available[0];
        }

        if (locToSelect != null)
        {
            _currentLocation = locToSelect;
            currentMapID     = locToSelect.mapID;
            GameStateManager.Instance.SetActiveLocation(locToSelect.locationID);
            if (locationNameText != null) locationNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocationsTable", locToSelect.locationNameKey);
            if (locationHintText != null) locationHintText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocationsTable", locToSelect.hintTextKey);
        }

        RefreshLocationList();

        bool hypothesisSent = _currentLocation != null &&
            GameStateManager.Instance?.GetPolygonState(_currentLocation.locationID) == "hypothesis_sent";

        if (hypothesisSent)
        {
            if (slotsGroup       != null) slotsGroup     .SetActive(false);
            if (progressBarGroup != null) progressBarGroup.SetActive(false);
            if (submitButton     != null) submitButton.gameObject.SetActive(false);
            // Přepni na All a obnov knihovnu — indicie jsou volné, bez ohledu na průběh animace
            ResetToAllTab();
            RefreshLibrary();
            ShowHypothesisPanel(_currentLocation, _currentLocation.requiredClues);
        }
        else
        {
            if (hypothesisPanel != null) hypothesisPanel.SetActive(false);
            if (slotsGroup      != null) slotsGroup     .SetActive(true);
            RefreshLibrary();
            RefreshProgress();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (detailPanel != null && detailPanel.activeSelf)
                detailPanel.SetActive(false);
            else
                GoBack();
        }
    }

    private void OnDisable()
    {
        // Pokud animace návratu ještě běží — zastav ji a ukliď proxy objekty
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
            _returnCoroutine = null;
        }

        foreach (var proxy in _returnProxies)
            if (proxy != null) Destroy(proxy);
        _returnProxies.Clear();

        // Obnov knihovnu v záložce All — stav slotů je správně v GameState
        ResetToAllTab();
        RefreshLibrary();
    }

    // =========================================================================
    // Drag & Drop — veřejné API pro ClueSlotUI
    // =========================================================================

    /// <summary>
    /// Zavolá ClueSlotUI.OnDrop po rozhodnutí co kam patří.
    /// fromSlotIndex == -1 znamená drop z knihovny (ne z jiného slotu).
    /// swapClue == obsah cílového slotu před dropem (pro swap).
    /// </summary>
    public void HandleDropOnSlot(int toIndex, ClueData dropped, int fromSlotIndex, ClueData swapClue)
    {
        _dropHandled = true;

        if (toIndex >= 0 && toIndex < clueSlots.Length)
            clueSlots[toIndex]?.SetClue(dropped);

        if (fromSlotIndex >= 0 && fromSlotIndex < clueSlots.Length)
            clueSlots[fromSlotIndex]?.SetClue(swapClue);

        SaveCurrentSlots();
        RefreshProgress();
        RefreshLibraryVisuals();
    }

    /// <summary>
    /// Přetažení indicie ze slotu zpět do knihovny — zavolá LibraryDropZone.
    /// </summary>
    public void HandleDropOnLibrary(int fromSlotIndex, ClueData clue)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= clueSlots.Length) return;
        _dropHandled = true;
        clueSlots[fromSlotIndex]?.SetClue(null);
        SaveCurrentSlots();
        RefreshProgress();
        RefreshLibraryVisuals();
    }

    public void OnLibraryDragBegan(ClueData clue)
    {
        _draggingFromLibraryClue = clue;
        RefreshLibraryCounts();
    }

    public void OnLibraryDragEnded()
    {
        _draggingFromLibraryClue = null;
        // Finální stav počtů se přepočítá přes RefreshLibraryVisuals
    }

    public void OnSlotDragBegan(int slotIndex)
    {
        _dropHandled = false;
        // Slot je v OnBeginDrag již prázdný (SetClue(null)) → počty se přepočítají
        RefreshLibraryCounts();
    }

    public void OnSlotDragEnded(int slotIndex, ClueData draggingClue)
    {
        if (!_dropHandled && draggingClue != null)
        {
            // Drop nespadl na žádný cíl → obnov indicii v původním slotu
            if (slotIndex >= 0 && slotIndex < clueSlots.Length)
                clueSlots[slotIndex]?.SetClue(draggingClue);
        }

        _dropHandled = false;
        RefreshProgress();
        RefreshLibraryVisuals();
    }

    // =========================================================================
    // Knihovna indicií
    // =========================================================================

    private void RefreshLibrary()
    {
        if (clueListContent == null || clueItemPrefab == null || clueDatabase == null) return;

        foreach (Transform child in clueListContent)
            Destroy(child.gameObject);
        _libraryItems.Clear();

        var gsm   = GameStateManager.Instance;
        var clues = clueDatabase.GetCluesForMap(currentMapID)
                                .Where(c => c.isAvailableByDefault || (gsm != null && gsm.IsClueUnlocked(c.clueID)))
                                .ToList();

        if (_activeTypeFilter.HasValue)
            clues = clues.Where(c => c.clueType == _activeTypeFilter.Value).ToList();

        foreach (var clue in clues)
        {
            var go   = Instantiate(clueItemPrefab, clueListContent);
            var item = go.GetComponent<ClueItemUI>();
            if (item == null) continue;
            item.Initialize(clue, this, rootCanvas);
            _libraryItems.Add(item);
        }

        RefreshLibraryVisuals();
    }

    public void RefreshLibraryVisuals()
    {
        var usedIDs = new HashSet<string>(
            clueSlots.Where(s => s?.CurrentClue != null).Select(s => s.CurrentClue.clueID)
        );

        foreach (var item in _libraryItems)
            item.SetUsedVisual(usedIDs.Contains(item.ClueData.clueID));

        RefreshLibraryCounts();
    }

    private void RefreshLibraryCounts()
    {
        if (clueDatabase == null || GameStateManager.Instance == null) return;

        var gsm     = GameStateManager.Instance;
        var usedIDs = new HashSet<string>(
            clueSlots.Where(s => s?.CurrentClue != null).Select(s => s.CurrentClue.clueID)
        );

        // Dostupné indicie pro aktuální mapu, minus ty v slotech
        var available = clueDatabase.GetCluesForMap(currentMapID)
            .Where(c => c.isAvailableByDefault || gsm.IsClueUnlocked(c.clueID))
            .Where(c => !usedIDs.Contains(c.clueID))
            .ToList();

        // Odečti indicii právě draggovanou z knihovny (vizuálně opouští knihovnu)
        if (_draggingFromLibraryClue != null)
            available = available.Where(c => c.clueID != _draggingFromLibraryClue.clueID).ToList();

        int countAll       = available.Count;
        int countDocuments = available.Count(c => c.clueType == ClueData.ClueType.Document);
        int countMaps      = available.Count(c => c.clueType == ClueData.ClueType.Map);
        int countNotes     = available.Count(c => c.clueType == ClueData.ClueType.Note);
        int countArtifacts = available.Count(c => c.clueType == ClueData.ClueType.Artifact);

        if (tabCountAll       != null) tabCountAll      .text = $"[{countAll}]";
        if (tabCountDocuments != null) tabCountDocuments.text = $"[{countDocuments}]";
        if (tabCountMaps      != null) tabCountMaps     .text = $"[{countMaps}]";
        if (tabCountNotes     != null) tabCountNotes    .text = $"[{countNotes}]";
        if (tabCountArtifacts != null) tabCountArtifacts.text = $"[{countArtifacts}]";
    }

    private void SetTypeFilter(ClueData.ClueType? type)
    {
        _activeTypeFilter = type;
        RefreshLibrary();
    }

    /// <summary>
    /// Přepne záložku knihovny na "Vše" bez obnovy seznamu — volej před RefreshLibrary().
    /// </summary>
    private void ResetToAllTab()
    {
        _activeTypeFilter = null;
    }

    // =========================================================================
    // Sloty — ukládání a načítání
    // =========================================================================

    private void LoadSlotsFromSave()
    {
        if (clueDatabase == null || GameStateManager.Instance == null) return;

        string[] saved = GameStateManager.Instance.LoadClueSlots();
        for (int i = 0; i < clueSlots.Length && i < saved.Length; i++)
        {
            ClueData clue = string.IsNullOrEmpty(saved[i]) ? null : clueDatabase.GetClue(saved[i]);
            clueSlots[i]?.SetClue(clue);
        }
    }

    private void SaveCurrentSlots()
    {
        if (GameStateManager.Instance == null) return;

        var ids = new string[6];
        for (int i = 0; i < clueSlots.Length && i < 6; i++)
            ids[i] = clueSlots[i]?.CurrentClue?.clueID;

        GameStateManager.Instance.SaveClueSlots(ids);
    }

    // =========================================================================
    // Progress bar & submit
    // =========================================================================

    private void RefreshProgress()
    {
        // Hypotéza již odeslána — sloty jsou skryté, nic nepřepočítávat
        if (_currentLocation != null &&
            GameStateManager.Instance?.GetPolygonState(_currentLocation.locationID) == "hypothesis_sent")
            return;

        if (_currentLocation == null || _currentLocation.requiredClues.Count == 0)
        {
            if (progressBarGroup != null) progressBarGroup.SetActive(false);
            UpdateSubmitButton(0f);
            return;
        }

        if (progressBarGroup != null) progressBarGroup.SetActive(true);

        var slottedIDs  = GetSlottedClueIDs();
        var requiredIDs = new HashSet<string>(_currentLocation.requiredClues.Select(c => c.clueID));

        // Pokud je na desce jakákoliv indicie mimo requiredClues → progress = 0
        bool hasExtra = slottedIDs.Any(id => !requiredIDs.Contains(id));
        if (hasExtra)
        {
            if (progressBarFill != null) progressBarFill.fillAmount = 0f;
            UpdateSubmitButton(0f);
            return;
        }

        // Progress = kolik z požadovaných je správně na desce
        int matched  = requiredIDs.Count(id => slottedIDs.Contains(id));
        float progress = (float)matched / requiredIDs.Count;

        if (progressBarFill != null) progressBarFill.fillAmount = progress;

        UpdateSubmitButton(progress);
    }

    private void UpdateSubmitButton(float progress)
    {
        if (submitButton == null) return;

        bool sent = _currentLocation != null &&
                    GameStateManager.Instance?.GetPolygonState(_currentLocation.locationID) == "hypothesis_sent";

        // Přesná shoda = progress 1.0 a žádná navíc (hasExtra = 0 → progress byl předán jako 1f)
        submitButton.gameObject.SetActive(!sent && progress >= 1f);

        if (submitResultText != null)
            submitResultText.gameObject.SetActive(sent);
    }

    private HashSet<string> GetSlottedClueIDs() =>
        new HashSet<string>(
            clueSlots.Where(s => s?.CurrentClue != null).Select(s => s.CurrentClue.clueID)
        );

    // =========================================================================
    // Lokalita
    // =========================================================================

    public void LoadLocation(LocationData location)
    {
        if (location == null) return;

        SaveCurrentSlots();
        _currentLocation = location;
        GameStateManager.Instance?.SetActiveLocation(location.locationID);

        if (locationNameText != null) locationNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocationsTable", location.locationNameKey);
        if (locationHintText != null) locationHintText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocationsTable", location.hintTextKey);

        currentMapID = location.mapID;

        bool hypothesisSent = GameStateManager.Instance?.GetPolygonState(location.locationID) == "hypothesis_sent";

        if (hypothesisSent)
        {
            if (slotsGroup       != null) slotsGroup     .SetActive(false);
            if (progressBarGroup != null) progressBarGroup.SetActive(false);
            if (submitButton     != null) submitButton.gameObject.SetActive(false);
            ShowHypothesisPanel(location, location.requiredClues);
        }
        else
        {
            if (hypothesisPanel != null) hypothesisPanel.SetActive(false);
            if (slotsGroup      != null) slotsGroup     .SetActive(true);
            LoadSlotsFromSave();
            RefreshLibrary();
            RefreshProgress();
        }

        RefreshLocationList();
    }

    private void RefreshLocationList()
    {
        if (locationListContent == null)
        {
            Debug.LogError("[BadatelnaUI] locationListContent není přiřazen.");
            return;
        }
        if (locationButtonPrefab == null)
        {
            Debug.LogError("[BadatelnaUI] locationButtonPrefab není přiřazen.");
            return;
        }
        if (locationDatabase == null)
        {
            Debug.LogError("[BadatelnaUI] locationDatabase není přiřazen.");
            return;
        }

        foreach (Transform child in locationListContent)
            Destroy(child.gameObject);

        var locations = locationDatabase.GetLocationsForMap(currentMapID);
        Debug.Log($"[BadatelnaUI] RefreshLocationList — currentMapID='{currentMapID}', lokalit: {locations.Count}");

        foreach (var loc in locations)
        {
            Debug.Log($"[BadatelnaUI] Instantiate lokace: {loc.locationID} (mapID='{loc.mapID}')");
            var go  = Instantiate(locationButtonPrefab, locationListContent);
            var txt = go.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocationsTable", loc.locationNameKey);

            var btn = go.GetComponent<Button>() ?? go.GetComponentInChildren<Button>();
            if (btn == null) { Debug.LogWarning($"[BadatelnaUI] locationButtonPrefab nemá Button komponentu: {loc.locationID}"); continue; }

            btn.interactable = true;

            bool isActive      = _currentLocation != null && loc.locationID == _currentLocation.locationID;
            var  colors        = btn.colors;
            colors.normalColor = isActive ? locationActiveColor : Color.white;
            btn.colors         = colors;

            var locCopy = loc;
            btn.onClick.AddListener(() => LoadLocation(locCopy));
        }
    }

    private string GetLocationStatus(LocationData loc)
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) return "available";
        if (gsm.GetPrestigePoints() < loc.requiredPrestigeLevel) return "locked";

        return gsm.GetPolygonState(loc.locationID) switch
        {
            "unlocked"        => "unlocked",
            "hypothesis_sent" => "hypothesis_sent",
            _                 => "available",
        };
    }

    private void SwitchMap(string mapID)
    {
        if (string.IsNullOrEmpty(mapID)) return;
        currentMapID = mapID;

        // Pokud aktuální lokace nepatří do nové mapy, zvol první dostupnou
        if (_currentLocation == null || _currentLocation.mapID != mapID)
        {
            var available = locationDatabase?.GetLocationsForMap(mapID);
            if (available != null && available.Count > 0)
            {
                _currentLocation = available[0];
                GameStateManager.Instance?.SetActiveLocation(_currentLocation.locationID);
                if (locationNameText != null) locationNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocationsTable", _currentLocation.locationNameKey);
                if (locationHintText != null) locationHintText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocationsTable", _currentLocation.hintTextKey);
            }
        }

        RefreshLibrary();
        RefreshLocationList();
        RefreshProgress();
    }

    // =========================================================================
    // Odeslání hypotézy
    // =========================================================================

    private void SubmitHypothesis()
    {
        if (_currentLocation == null) return;
        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        // Ulož seznam použitých indicií před vymazáním slotů
        var usedClues = clueSlots
            .Where(s => s?.CurrentClue != null)
            .Select(s => s.CurrentClue)
            .ToList();

        EventBus.HypothesisBuilt(_currentLocation.locationID);
        gsm.SetPolygonState(_currentLocation.locationID, "hypothesis_sent");
        gsm.MarkHypothesisBuilt(_currentLocation.locationID);

        NotificationManager.Instance?.Show(NotificationManager.SectionKomunikace);
        RefreshLocationList();

        // Animace návratu indicií → pak skrytí slotů a zobrazení panelu hypotézy
        _returnCoroutine = StartCoroutine(ReturnCluesCoroutine(_currentLocation, usedClues));
    }

    private IEnumerator ReturnCluesCoroutine(LocationData location, List<ClueData> usedClues)
    {
        if (clueReturnDuration > 0f && clueItemPrefab != null && rootCanvas != null)
        {
            // Střed cílového ScrollView v canvas-local souřadnicích (GetWorldCorners = world space, nezávislé na pivotu)
            Vector2 libraryCenter = GetLibraryCenterCanvasLocal();

            var proxies = new List<(GameObject go, Vector2 from, Vector2 target)>();

            foreach (var slot in clueSlots)
            {
                if (slot?.CurrentClue == null) continue;

                var proxy     = Instantiate(clueItemPrefab, rootCanvas.transform);
                var proxyItem = proxy.GetComponent<ClueItemUI>();
                if (proxyItem != null) { proxyItem.SetVisuals(slot.CurrentClue); proxyItem.enabled = false; }

                var pg = proxy.GetComponent<CanvasGroup>() ?? proxy.AddComponent<CanvasGroup>();
                pg.blocksRaycasts = false;

                Vector2 fromPos   = GetCanvasLocalPosition(slot.GetComponent<RectTransform>());
                Vector2 scattered = libraryCenter + new Vector2(
                    Random.Range(-returnScatterRadius, returnScatterRadius),
                    Random.Range(-returnScatterRadius, returnScatterRadius));

                proxy.GetComponent<RectTransform>().localPosition = fromPos;
                proxies.Add((proxy, fromPos, scattered));
                _returnProxies.Add(proxy); // Registruj pro cleanup při předčasném ukončení
            }

            float elapsed = 0f;
            while (elapsed < clueReturnDuration)
            {
                elapsed += Time.deltaTime;
                float t = clueReturnCurve.Evaluate(Mathf.Clamp01(elapsed / clueReturnDuration));
                foreach (var (go, from, target) in proxies)
                {
                    if (go != null)
                        go.GetComponent<RectTransform>().localPosition = Vector2.Lerp(from, target, t);
                }
                yield return null;
            }

            foreach (var (go, _, _) in proxies)
                if (go != null) Destroy(go);
            _returnProxies.Clear();
        }

        // Vymaž sloty a ulož stav
        foreach (var slot in clueSlots)
            slot?.SetClue(null);
        SaveCurrentSlots();
        GameStateManager.Instance?.SaveGame();

        // Aktualizuj počty záložek ihned po logickém vymazání slotů
        RefreshLibraryCounts();

        _returnCoroutine = null;

        // Skryj oblast slotů, progress bar a submit tlačítko
        if (slotsGroup       != null) slotsGroup     .SetActive(false);
        if (progressBarGroup != null) progressBarGroup.SetActive(false);
        if (submitButton     != null) submitButton.gameObject.SetActive(false);

        // Přepni knihovnu na záložku All a obnov zobrazení indicií
        ResetToAllTab();
        RefreshLibrary();

        // Zobraz panel hypotézy
        ShowHypothesisPanel(location, usedClues);
    }

    private void ShowHypothesisPanel(LocationData location, List<ClueData> clues)
    {
        if (hypothesisPanel == null || location == null) return;
        hypothesisPanel.SetActive(true);

        if (hypothesisTitleText != null)
            hypothesisTitleText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocationsTable", location.hypothesisNameKey);

        if (hypothesisDescText != null)
            hypothesisDescText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocationsTable", location.hypothesisDescKey);

        if (hypothesisClueListText != null)
        {
            hypothesisClueListText.text = string.Join("\n", clues.Select(c =>
                LocalizationSettings.StringDatabase.GetLocalizedString("CluesTable", c.clueNameKey)));
        }

        // Zobraz "Odesláno, zkontroluj příchozí poštu" dokud akce není potvrzena
        if (hypothesisPendingTextGO != null)
        {
            bool actionProcessed = !string.IsNullOrEmpty(location.hypothesisActionID)
                && (GameStateManager.Instance?.IsActionProcessed(location.hypothesisActionID) ?? false);
            hypothesisPendingTextGO.SetActive(!actionProcessed);
        }
    }

    /// <summary>
    /// Vrátí canvas-local pozici středu daného RectTransformu (nezávislé na pivotu).
    /// </summary>
    private Vector2 GetCanvasLocalPosition(RectTransform target)
    {
        if (target == null || rootCanvas == null) return Vector2.zero;

        // Střed = průměr world-space rohů (nezávisí na pivotu)
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(), screenPoint, null, out Vector2 local);
        return local;
    }

    /// <summary>
    /// Vrátí canvas-local střed oblasti knihovny (ScrollView). Fallback = clueListContent.
    /// </summary>
    private Vector2 GetLibraryCenterCanvasLocal()
    {
        var target = clueLibraryScrollView != null
            ? clueLibraryScrollView
            : clueListContent as RectTransform ?? clueListContent?.GetComponent<RectTransform>();
        return GetCanvasLocalPosition(target);
    }

    // =========================================================================
    // Detail indicie
    // =========================================================================

    public void ShowClueDetail(ClueData data)
    {
        if (detailPanel == null || data == null) return;
        detailPanel.SetActive(true);
        if (detailTitleText   != null) detailTitleText  .text = LocalizationSettings.StringDatabase.GetLocalizedString("CluesTable", data.clueNameKey);
        if (detailContentText != null) detailContentText.text = LocalizationSettings.StringDatabase.GetLocalizedString("CluesTable", data.clueContentKey);
    }

    // =========================================================================
    // Navigace
    // =========================================================================

    private void GoBack()
    {
        SaveCurrentSlots();
        GameStateManager.Instance?.SaveGame();
        mainGameUI?.GoBack();
    }
}

// ─────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public class MapTabEntry
{
    public Button button;
    public string mapID;
}
