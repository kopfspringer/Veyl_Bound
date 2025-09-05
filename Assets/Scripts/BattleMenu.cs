using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Erstellt und verwaltet das Kampfmenü, über das der Spieler Aktionen auswählt.
/// </summary>
public class BattleMenu : MonoBehaviour
{
    /// <summary>Singleton-Instanz des Kampfmenüs.</summary>
    public static BattleMenu instance;

    [Header("Menu Elements")]
    /// <summary>Wurzelobjekt, das alle UI-Elemente des Menüs enthält.</summary>
    public GameObject menuRoot;
    /// <summary>Button für Angriffe.</summary>
    public Button attackButton;
    /// <summary>Button für Magie.</summary>
    public Button magicButton;
    /// <summary>Button für Gegenstände.</summary>
    public Button itemsButton;
    /// <summary>Button zum Beenden des Zuges.</summary>
    public Button waitButton;

    /// <summary>
    /// Initialisiert das Menü und registriert die Events.
    /// </summary>
    private void Awake()
    {
        instance = this;

        // Falls kein Menü im Editor zugewiesen wurde, wird eines zur Laufzeit erzeugt.
        if (menuRoot == null)
        {
            CreateMenu();
        }

        // Menü ist zu Beginn unsichtbar.
        Hide();

        // "Warten"-Button beendet den Zug sofort.
        if (waitButton != null)
        {
            waitButton.onClick.AddListener(() =>
            {
                Hide();
                GameManager.instance.EndTurn();
            });
        }
    }

    /// <summary>
    /// Erstellt das Menü dynamisch im Canvas.
    /// </summary>
    private void CreateMenu()
    {
        Canvas canvas = new GameObject("BattleMenuCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        menuRoot = canvas.gameObject;

        GameObject panel = new GameObject("MenuPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(200, 300);
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 10f;

        // Buttons erzeugen und zuweisen.
        attackButton = CreateButton(panel.transform, "Attack");
        magicButton = CreateButton(panel.transform, "Magic");
        itemsButton = CreateButton(panel.transform, "Items");
        waitButton = CreateButton(panel.transform, "Wait");
    }

    /// <summary>
    /// Hilfsmethode zum Erstellen eines Buttons mit Text.
    /// </summary>
    private Button CreateButton(Transform parent, string text)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent, false);

        Image image = buttonObj.AddComponent<Image>();
        image.color = Color.white;

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 40);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    /// <summary>
    /// Zeigt das Menü an.
    /// </summary>
    public void Show()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(true);
        }
    }

    /// <summary>
    /// Versteckt das Menü.
    /// </summary>
    public void Hide()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
    }
}

