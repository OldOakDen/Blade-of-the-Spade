using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KomunikaceUI : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Data")]
    [SerializeField] private MessageDatabase  messageDatabase;
    [SerializeField] private LocationDatabase locationDatabase;

    [Header("Horní lišta")]
    [SerializeField] private Button   buttonClose;
    [SerializeField] private Button   buttonTabInbox;
    [SerializeField] private Button   buttonTabPending;
    [SerializeField] private Button   buttonTabDone;
    [Tooltip("Badge — počet nepřečtených zpráv v Inbox.")]
    [SerializeField] private TMP_Text tabCountInbox;
    [Tooltip("Badge — počet čekajících akcí v Pending.")]
    [SerializeField] private TMP_Text tabCountPending;
    [Tooltip("Badge na Done záložce — skryj GameObject nebo nech prázdné.")]
    [SerializeField] private TMP_Text tabCountDone;

    [Header("Seznam zpráv (ScrollView content)")]
    [SerializeField] private Transform  listContent;
    [SerializeField] private GameObject messageRowPrefab;
    // Prefab musí mít:
    //   TMP_Text  "Text_Sender"
    //   TMP_Text  "Text_Subject"
    //   Image     "SelectionHighlight"  (defaultně neaktivní)
    //   Button    (root)

    [Header("Zvýraznění nepřečtené zprávy")]
    [SerializeField] private Color unreadColor = Color.white;
    [SerializeField] private Color readColor   = new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("Detail zprávy")]
    [SerializeField] private ScrollRect detailScrollRect; // ScrollView obsahující tělo zprávy
    [SerializeField] private GameObject detailRoot;
    [SerializeField] private TMP_Text   detailSender;
    [SerializeField] private TMP_Text   detailSubject;
    [SerializeField] private TMP_Text   detailBody;
    [SerializeField] private Button     buttonConfirm;
    [SerializeField] private TMP_Text   buttonConfirmLabel;

    [Header("Dependencies")]
    [SerializeField] private MainGameUI mainGameUI;

    // -------------------------------------------------------------------------
    // Stav
    // -------------------------------------------------------------------------

    private enum Tab { Inbox, Pending, Done }
    private Tab _activeTab = Tab.Inbox;

    private List<MessageData>              _allUnlocked = new();
    private List<GameObject>              _rows        = new();
    private Dictionary<string, GameObject> _rowByID    = new();
    private MessageData                   _selected;
    private GameObject                    _selectedRow;

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        EventBus.OnHypothesisBuilt += OnHypothesisBuilt;
    }

    private void OnDestroy()
    {
        EventBus.OnHypothesisBuilt -= OnHypothesisBuilt;
    }

    private void Start()
    {
        buttonClose      .onClick.AddListener(HideAndGoBack);
        buttonTabInbox   .onClick.AddListener(() => SwitchTab(Tab.Inbox));
        buttonTabPending .onClick.AddListener(() => SwitchTab(Tab.Pending));
        buttonTabDone    .onClick.AddListener(() => SwitchTab(Tab.Done));
        buttonConfirm    .onClick.AddListener(OnConfirm);

        detailRoot.SetActive(false);
        Refresh();
    }

    private void OnEnable()
    {
        if (messageDatabase == null) return;
        // Vždy otevírej na Inbox
        _activeTab = Tab.Inbox;
        _selected  = null;
        _selectedRow = null;
        Refresh();
        SelectOnOpen();
    }

    // -------------------------------------------------------------------------
    // EventBus
    // -------------------------------------------------------------------------

    private void OnHypothesisBuilt(string locationID)
    {
        if (locationDatabase == null) return;

        var location = locationDatabase.GetLocation(locationID);
        if (location == null || string.IsNullOrEmpty(location.hypothesisMessageID)) return;

        var gsm = GameStateManager.Instance;
        if (gsm == null) return;

        // Odemkni zprávu jako nepřečtenou
        gsm.SetMessageState(location.hypothesisMessageID, false);
        gsm.SaveGame();

        if (messageDatabase != null)
            Refresh();

        NotificationManager.Instance?.Show(NotificationManager.SectionKomunikace);
    }

    // -------------------------------------------------------------------------
    // Záložky
    // -------------------------------------------------------------------------

    private void SwitchTab(Tab tab)
    {
        _activeTab = tab;
        // Detail zůstane viditelný — neskrýváme ho při přepnutí záložky
        BuildList();
        UpdateTabLabels();
    }

    private void UpdateTabLabels()
    {
        var gsm = GameStateManager.Instance;

        int countInbox   = _allUnlocked.Count;
        int countPending = _allUnlocked.Count(m => IsInPending(m, gsm));
        int countDone    = _allUnlocked.Count(m => IsInDone(m, gsm));

        if (tabCountInbox   != null) tabCountInbox  .text = countInbox  .ToString();
        if (tabCountPending != null) tabCountPending.text = countPending .ToString();
        if (tabCountDone    != null) tabCountDone   .text = countDone    .ToString();
    }

    // Inbox = všechny odemčené zprávy
    private bool IsInInbox(MessageData msg) => true;

    // Pending = requiresAction == true AND akce není zpracována
    private bool IsInPending(MessageData msg, GameStateManager gsm) =>
        msg.requiresAction && (gsm == null || !gsm.IsActionProcessed(msg.actionID));

    // Done = requiresAction == true AND akce je zpracována
    private bool IsInDone(MessageData msg, GameStateManager gsm) =>
        msg.requiresAction && gsm != null && gsm.IsActionProcessed(msg.actionID);

    // -------------------------------------------------------------------------
    // Seznam
    // -------------------------------------------------------------------------

    private void Refresh()
    {
        foreach (var row in _rows)
            row?.SetActive(false);

        // BuildList musí být PŘED UpdateTabLabels:
        // vytváření řádků spouští GetLocalizedString(), které může
        // re-firovat LocalizeStringEvent na tab labelech a přepsat je.
        _allUnlocked = messageDatabase.GetUnlockedMessages(GameStateManager.Instance);
        BuildList();
        UpdateTabLabels();
    }

    private void BuildList()
    {
        foreach (var row in _rows)
            Destroy(row);
        _rows.Clear();
        _rowByID.Clear();
        _selectedRow = null;

        var gsm = GameStateManager.Instance;

        // Filtr podle aktivní záložky
        IEnumerable<MessageData> visible = _activeTab switch
        {
            Tab.Inbox   => _allUnlocked,
            Tab.Pending => _allUnlocked.Where(m => IsInPending(m, gsm)),
            Tab.Done    => _allUnlocked.Where(m => IsInDone(m, gsm)),
            _           => _allUnlocked
        };

        // Seřaď od nejnovější — MessageDatabase zachovává pořadí ze SO; reverzneme ho
        var ordered = visible.Reverse().ToList();

        foreach (var msg in ordered)
        {
            var row = Instantiate(messageRowPrefab, listContent);
            _rows.Add(row);
            _rowByID[msg.messageID] = row;

            var senderText  = row.transform.Find("Text_Sender") ?.GetComponent<TMP_Text>();
            var subjectText = row.transform.Find("Text_Subject")?.GetComponent<TMP_Text>();
            var highlight   = row.transform.Find("SelectionHighlight")?.GetComponent<Image>();

            if (senderText  != null) senderText .text = msg.senderName.GetLocalizedString();
            if (subjectText != null) subjectText.text = msg.subject   .GetLocalizedString();

            // Vizuální rozlišení přečtená / nepřečtená
            bool isRead = gsm != null && gsm.IsMessageRead(msg.messageID);
            Color textColor = isRead ? readColor : unreadColor;
            if (senderText  != null) senderText .color = textColor;
            if (subjectText != null) subjectText.color = textColor;

            // Obnov highlight pokud jde o aktuálně vybranou zprávu
            bool isSelected = _selected != null && msg.messageID == _selected.messageID;
            if (highlight != null) highlight.gameObject.SetActive(isSelected);
            if (isSelected) _selectedRow = row;

            var captured    = msg;
            var capturedRow = row;
            var btn = row.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => SelectMessage(captured, capturedRow));
            else
                Debug.LogWarning("[KomunikaceUI] messageRowPrefab nemá Button na rootu!");
        }
    }

    // -------------------------------------------------------------------------
    // Výběr zprávy při otevření
    // -------------------------------------------------------------------------

    private void SelectOnOpen()
    {
        var gsm = GameStateManager.Instance;

        // 1. První nepřečtená v Inbox
        var toSelect = _allUnlocked.LastOrDefault(m => gsm == null || !gsm.IsMessageRead(m.messageID));

        // 2. Fallback: lastSelectedMessageID pokud existuje v Inbox
        if (toSelect == null)
        {
            string lastID = gsm?.GetLastSelectedMessage();
            if (!string.IsNullOrEmpty(lastID))
                toSelect = _allUnlocked.Find(m => m.messageID == lastID);
        }

        // 3. Fallback: první zpráva v Inbox
        if (toSelect == null && _allUnlocked.Count > 0)
            toSelect = _allUnlocked[_allUnlocked.Count - 1]; // nejnovější = poslední v SO, reverznuto

        if (toSelect == null) return;

        if (_rowByID.TryGetValue(toSelect.messageID, out var targetRow))
            SelectMessage(toSelect, targetRow);
    }

    // -------------------------------------------------------------------------
    // Detail
    // -------------------------------------------------------------------------

    private void SelectMessage(MessageData msg, GameObject row)
    {
        // Zruš highlight předchozího řádku
        if (_selectedRow != null)
        {
            var prev = _selectedRow.transform.Find("SelectionHighlight")?.GetComponent<Image>();
            if (prev != null) prev.gameObject.SetActive(false);
        }

        _selected    = msg;
        _selectedRow = row;

        // Highlight nového řádku
        var hl = row.transform.Find("SelectionHighlight")?.GetComponent<Image>();
        if (hl != null) hl.gameObject.SetActive(true);

        // Ulož poslední vybranou zprávu
        GameStateManager.Instance?.SetLastSelectedMessage(msg.messageID);

        // Naplň detail
        detailSender .text = msg.senderName.GetLocalizedString();
        detailSubject.text = msg.subject   .GetLocalizedString();
        detailBody   .text = msg.body      .GetLocalizedString();

        var gsm = GameStateManager.Instance;
        bool showConfirm = msg.requiresAction && (gsm == null || !gsm.IsActionProcessed(msg.actionID));
        buttonConfirm.gameObject.SetActive(showConfirm);

        if (showConfirm && buttonConfirmLabel != null)
        {
            string btnLabel = msg.actionButtonLabel.IsEmpty
                ? "Potvrzuji"
                : msg.actionButtonLabel.GetLocalizedString();
            buttonConfirmLabel.text = btnLabel;
        }

        detailRoot.SetActive(true);

        // Vyroluj na začátek zprávy
        if (detailScrollRect != null)
            detailScrollRect.verticalNormalizedPosition = 1f;

        // Označ jako přečtenou — obnov barvu řádku a badge počty
        if (gsm != null && !gsm.IsMessageRead(msg.messageID))
        {
            gsm.MarkMessageRead(msg.messageID);
            EventBus.MessageRead(msg.messageID);

            // Aktualizuj barvu textu v řádku
            var senderText  = row.transform.Find("Text_Sender") ?.GetComponent<TMP_Text>();
            var subjectText = row.transform.Find("Text_Subject")?.GetComponent<TMP_Text>();
            if (senderText  != null) senderText .color = readColor;
            if (subjectText != null) subjectText.color = readColor;

            UpdateTabLabels();
        }
    }

    private void HideDetail()
    {
        detailRoot.SetActive(false);
        _selected    = null;
        _selectedRow = null;
    }

    private void HideAndGoBack()
    {
        HideDetail();
        mainGameUI?.GoBack();
    }

    private void OnConfirm()
    {
        if (_selected == null) return;

        MessageActionHandler.Execute(_selected.actionID);
        GameStateManager.Instance?.MarkActionProcessed(_selected.actionID);

        // Skryj potvrzovací tlačítko — detail zůstane viditelný
        buttonConfirm.gameObject.SetActive(false);

        // Zpráva přejde do Done — obnov počty záložek
        Refresh();
    }
}
