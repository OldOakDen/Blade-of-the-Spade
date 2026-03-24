using UnityEngine;

public class ObjectPreview : MonoBehaviour
{
    public enum PreviewMode
    {
        Discovery,   // M�d p�i objevov�n� nov�ho p�edm�tu
        Examination, // M�d pro prohl�en� ji� objeven�ch p�edm�t�
        Showcase     // M�d pro prezentaci (nap�. bez interakce)
    }

    public DetectManager detectManager;
    public GameObject grid;
    public GameObject findsScrollView;
    public Transform newParent; // Nov� rodi� pro nalezen� objekty
    public GameObject previewedTarget;
    public bool discoveryNow = true;
    public PreviewMode currentMode; // P�id�me aktu�ln� m�d

    public bool showByButton;

    private float rotationSpeed = 1000f; // Rychlost rotace
    private float scaleSpeed = 10f; // Rychlost zm�ny velikosti
    private Vector3 initialScale; // Po��te�n� velikost
    private Vector3 initialGridScale; // Po��te�n� velikost gridu

    void OnEnable()
    {
        showByButton = false;
    }

    public void InitializeMode(PreviewMode mode)
    {
        currentMode = mode;

        switch (mode)
        {
            case PreviewMode.Discovery:
                InitializeDiscoveryMode();
                break;
            case PreviewMode.Examination:
                InitializeExaminationMode();
                break;
            case PreviewMode.Showcase:
                InitializeShowcaseMode();
                break;
        }
    }

    private void InitializeDiscoveryMode()
    {
        // Specifick� nastaven� pro Discovery m�d
        detectManager.hintTextbox.DisplayLocalizedText("MetalDetectingSceneTable", "dtct_hint_itemFound", "dtct_hint_identify01", "dtct_hint_identify02", "dtct_hint_identify03", "dtct_hint_identify04", "dtct_hint_identify05", "dtct_hint_identify06", "dtct_hint_identify07");

        detectManager.identifyForm.SetActive(true);
        detectManager.gridButton.SetActive(true);
        GetComponent<Animator>().enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        previewedTarget = detectManager.actualSignal;
        newParent.transform.eulerAngles = Vector3.zero;
        newParent.transform.localRotation = Quaternion.identity;

        previewedTarget.transform.SetParent(newParent, false);
        previewedTarget.transform.localPosition = Vector3.zero;
        previewedTarget.transform.eulerAngles = Vector3.zero;
        previewedTarget.transform.localRotation = Quaternion.identity;

        previewedTarget.transform.localScale *= detectManager.actualSignal.GetComponent<DetectTarget>().previewResize;
        previewedTarget.GetComponent<Collider>().enabled = false;
        previewedTarget.GetComponent<ClayEffect>().enabled = true;

        grid.transform.localScale = Vector3.one;
        grid.transform.localScale *= detectManager.actualSignal.GetComponent<DetectTarget>().previewResize;

        AudioManager.Instance.PlaySFX("Discovery01");
        AudioManager.Instance.PlayLoopSound("Heartbeat");

        initialScale = previewedTarget.transform.localScale;
        initialGridScale = grid.transform.localScale;
    }

    private void InitializeExaminationMode()
    {
        findsScrollView.SetActive(true);
        //zde by 

        detectManager.gridButton.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AudioManager.Instance.PlaySFX("Backpack");

        GetComponent<Animator>().enabled = false;
        GetComponent<Animator>().enabled = true;

        //tady se musi zinicializovat prohlizeny predmet podle toho, na co hrac kliknul ve vyberu
        //jako prvni se ale automaticky objevuje posledne objeveny predmet
        //print("NAPOSLED NALEZENY PREDMET: " + detectManager.foundItemsManager.GetLastFoundItemName());

        //protoze tento mod funguje trosku jinak nez mod po objeveni predmetu, musi se asi urcite veci inicializovat znovu v nejake dalsi funkci
        //kdyz hrac klikne na button s jinym predmetem, ktery chce prohlizet
        if (!showByButton) //pokud neni tato inicializace prostrednictvim tlacitka ve scrollview, ukaz posledne nalezeny predmet
        {
            previewedTarget = detectManager.foundItemsManager.GetLastFoundItemInstance();
        }
       
        if (previewedTarget != null)
        {
            newParent.transform.eulerAngles = Vector3.zero;
            newParent.transform.localRotation = Quaternion.identity;

            previewedTarget.transform.SetParent(newParent, false);
            previewedTarget.transform.localPosition = Vector3.zero;
            previewedTarget.transform.eulerAngles = Vector3.zero;
            previewedTarget.transform.localRotation = Quaternion.identity;

            previewedTarget.transform.localScale *= previewedTarget.GetComponent<DetectTarget>().previewResize;
            previewedTarget.GetComponent<Collider>().enabled = false;
            previewedTarget.GetComponent<ClayEffect>().enabled = false; // Nebude zapnut� ClayEffect

            grid.transform.localScale = Vector3.one;
            grid.transform.localScale *= previewedTarget.GetComponent<DetectTarget>().previewResize;

            initialScale = previewedTarget.transform.localScale;
            initialGridScale = grid.transform.localScale;
        }

    }

    private void InitializeShowcaseMode()
    {
        // Nap��klad: statick� m�d bez interakce
        detectManager.identifyForm.SetActive(false);
        detectManager.gridButton.SetActive(false);
        GetComponent<Animator>().enabled = false;

        previewedTarget = detectManager.actualSignal;
        newParent.transform.eulerAngles = Vector3.zero;
        newParent.transform.localRotation = Quaternion.identity;

        previewedTarget.transform.SetParent(newParent, false);
        previewedTarget.transform.localPosition = Vector3.zero;
        previewedTarget.transform.eulerAngles = Vector3.zero;
        previewedTarget.transform.localRotation = Quaternion.identity;

        previewedTarget.transform.localScale *= detectManager.actualSignal.GetComponent<DetectTarget>().previewResize;
        previewedTarget.GetComponent<Collider>().enabled = false;
        previewedTarget.GetComponent<ClayEffect>().enabled = false;

        // Zde by mohl b�t tak� jin� styl zobrazen� (bez mo�nosti ot��en�, nap�.)
    }

    private void Update()
    {
        // Interakce a rotace jen pokud nejsme ve statickem modu
        if (currentMode != PreviewMode.Showcase)
        {
            HandleObjectRotation();
            HandleObjectScaling();
        }

        if (Input.GetKeyDown(KeyCode.Space) && currentMode == PreviewMode.Discovery)
        {
            detectManager.identifyForm.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleObjectRotation()
    {
        if (Input.GetMouseButton(1) && previewedTarget != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            previewedTarget.transform.Rotate(Vector3.up, -mouseX, Space.World);
            previewedTarget.transform.Rotate(Camera.main.transform.right, mouseY, Space.World);
        }
    }

    private void HandleObjectScaling()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0 && previewedTarget != null)
        {
            Vector3 newScale = previewedTarget.transform.localScale + Vector3.one * scrollInput * scaleSpeed;
            Vector3 newGridScale = grid.transform.localScale + Vector3.one * scrollInput * scaleSpeed;

            newScale = Vector3.Max(newScale, initialScale * 0.5f);
            newScale = Vector3.Min(newScale, initialScale * 2f);
            newGridScale = Vector3.Max(newGridScale, initialGridScale * 0.5f);
            newGridScale = Vector3.Min(newGridScale, initialGridScale * 2f);

            previewedTarget.transform.localScale = newScale;
            grid.transform.localScale = newGridScale;
        }
    }

public void ChangeDiscoveryNow()
    {
        GetComponent<Animator>().enabled = false;
    }

    public void ToggleGrid()
    {
        if (grid != null)
        {
            grid.SetActive(!grid.activeSelf); // Prepne aktivni stav GameObjectu
        }
    }
}
