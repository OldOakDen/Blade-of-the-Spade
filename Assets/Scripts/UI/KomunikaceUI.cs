using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KomunikaceUI : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Data")]
    [SerializeField] private MessageDatabase messageDatabase;

    [Header("Horní lišta")]
    [SerializeField] private Button   buttonClose;
    [SerializeField] private TMP_Text tabLabelNew;
    [SerializeField] private TMP_Text tabLabelPending;
    [SerializeField] private TMP_Text tabLabelDone;
    [SerializeField] private Button   buttonTabNew;
    [SerializeField] private Button   buttonTabPending;
    [SerializeField] private Button   buttonTabDone;

    [Header("Seznam zpráv (ScrollView content)")]
    [SerializeField] private Transform  listContent;
    [SerializeField] private GameObject messageRowPrefab;
    // Prefab musí mít:
    //   TMP_Text  "Sender"
    //   TMP_Text  "Subject"
    //   Image     "SelectionHighlight"  (defaultně neaktivní)
    //   Button    (root)

    [Header("Detail zprávy")]
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

    private enum Tab { New, Pending, Done }
    private Tab _activeTab = Tab.New;

    private List<MessageData> _allUnlocked = new();
    private List<GameObject>  _rows        = new();
    private MessageData       _selected;
    private GameObject        _selectedRow;

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------

    private void Start()
    {
        buttonClose     .onClick.AddListener(() => mainGameUI.GoBack());
        buttonTabNew    .onClick.AddListener(() => SwitchTab(Tab.New));
        buttonTabPending.onClick.AddListener(() => SwitchTab(Tab.Pending));
        buttonTabDone   .onClick.AddListener(() => SwitchTab(Tab.Done));
        buttonConfirm   .onClick.AddListener(OnConfirm);

        detailRoot.SetActive(false);
        Refresh();
    }

    private void OnEnable()
    {
        if (messageDatabase != null)
            Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            mainGameUI.GoBack();
    }

    // -------------------------------------------------------------------------
    // Záložky
    // -------------------------------------------------------------------------

    private void SwitchTab(Tab tab)
    {
        _activeTab   = tab;
        _selected    = null;
        _selectedRow = null;
        detailRoot.SetActive(false);
        BuildList();
    }

    private void UpdateTabLabels()
    {
        var gsm = GameStateManager.Instance;
        _allUnlocked = messageDatabase.GetUnlockedMessages(gsm);

        int countNew = 0, countPending = 0, countDone = 0;
        foreach (var msg in _allUnlocked)
        {
            switch (GetBucket(msg, gsm))
            {
                case Tab.New:     countNew++;     break;
                case Tab.Pending: countPending++; break;
                case Tab.Done:    countDone++;    break;
            }
        }

        tabLabelNew    .text = $"Nové zprávy [{countNew}]";
        tabLabelPending.text = $"Čeká na zpracování [{countPending}]";
        tabLabelDone   .text = $"Zpracováno [{countDone}]";
    }

    private Tab GetBucket(MessageData msg, GameStateManager gsm)
    {
        bool isRead = gsm != null && gsm.IsMessageRead(msg.messageID);
        if (!isRead) return Tab.New;
        if (msg.requiresAction && (gsm == null || !gsm.IsActionProcessed(msg.actionID)))
            return Tab.Pending;
        return Tab.Done;
    }

    // -------------------------------------------------------------------------
    // Seznam
    // -------------------------------------------------------------------------

    private void Refresh()
    {
        // Okamžitě skryj staré řádky ještě před přepočtem — Destroy() je odložený
        foreach (var row in _rows)
            row?.SetActive(false);

        UpdateTabLabels();
        BuildList();
    }

    private void BuildList()
    {
        foreach (var row in _rows)
            Destroy(row);
        _rows.Clear();

        var gsm = GameStateManager.Instance;

        foreach (var msg in _allUnlocked)
        {
            if (GetBucket(msg, gsm) != _activeTab) continue;

            var row = Instantiate(messageRowPrefab, listContent);
            _rows.Add(row);

            var senderText  = row.transform.Find("Text_Sender") ?.GetComponent<TMP_Text>();
            var subjectText = row.transform.Find("Text_Subject")?.GetComponent<TMP_Text>();
            var highlight   = row.transform.Find("SelectionHighlight")?.GetComponent<Image>();

            if (senderText  != null) senderText .text = msg.senderName.GetLocalizedString();
            if (subjectText != null) subjectText.text = msg.subject.GetLocalizedString();
            if (highlight   != null) highlight.gameObject.SetActive(false);

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
    // Detail
    // -------------------------------------------------------------------------

    private void SelectMessage(MessageData msg, GameObject row)
    {
        if (_selectedRow != null)
        {
            var prev = _selectedRow.transform.Find("SelectionHighlight")?.GetComponent<Image>();
            if (prev != null) prev.gameObject.SetActive(false);
        }

        _selected    = msg;
        _selectedRow = row;

        var hl = row.transform.Find("SelectionHighlight")?.GetComponent<Image>();
        if (hl != null) hl.gameObject.SetActive(true);

        detailSender .text = msg.senderName.GetLocalizedString();
        detailSubject.text = msg.subject.GetLocalizedString();
        detailBody   .text = msg.body.GetLocalizedString();

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

        if (gsm != null && !gsm.IsMessageRead(msg.messageID))
        {
            gsm.MarkMessageRead(msg.messageID);
            EventBus.MessageRead(msg.messageID);

            if (!msg.requiresAction)
                Refresh();
        }
    }

    private void OnConfirm()
    {
        if (_selected == null) return;

        MessageActionHandler.Execute(_selected.actionID);
        GameStateManager.Instance?.MarkActionProcessed(_selected.actionID);

        buttonConfirm.gameObject.SetActive(false);
        detailRoot.SetActive(false);

        _selected    = null;
        _selectedRow = null;

        Refresh();
    }
}
