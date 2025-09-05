using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Verwaltet das Aktionsmenü des Spielers und die Auswahl der verfügbaren Aktionen.
/// </summary>
public class ActionMenu : MonoBehaviour
{
    /// <summary>Globale Referenz auf dieses Menü zur einfachen Nutzung.</summary>
    public static ActionMenu instance;

    [Header("UI References")]
    /// <summary>Button, der das Aktionsmenü ein- oder ausblendet.</summary>
    public Button actionButton;
    /// <summary>Panel, das nach oben aufklappt und die Aktionsbuttons enthält.</summary>
    public GameObject dropUpPanel;

    [SerializeField] private Button attackButton; // Button für normale Angriffe
    [SerializeField] private Button magicButton;  // Button zum Anzeigen von Zaubern
    [SerializeField] private Button restButton;   // Button zum Ausruhen

    [SerializeField] private Button fireButton;   // Button für den Feuerzauber
    [SerializeField] private Button rainButton;   // Button für den Regenzauber

    /// <summary>Interner Zustand, welcher Angriff ausgeführt werden soll.</summary>
    private enum PendingAttack { None, Physical, Fire, Rain }
    /// <summary>Aktuell ausgewählter, aber noch nicht ausgeführter Angriff.</summary>
    private PendingAttack pendingAttack = PendingAttack.None;
    /// <summary>Reichweite des ausstehenden Angriffs.</summary>
    private int pendingRange;

    /// <summary>
    /// Initialisiert die Singleton-Instanz.
    /// </summary>
    private void Awake()
    {
        // Speichert die Instanz, damit andere Klassen leicht darauf zugreifen können.
        instance = this;
    }

    /// <summary>
    /// Registriert Event-Listener und versteckt das Menü zu Beginn.
    /// </summary>
    void Start()
    {
        // Das Menü wird zunächst unsichtbar.
        HideMenu();

        // Listener für den Hauptbutton, der das Panel auf- und zuklappt.
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(ToggleDropUp);
        }

        // Listener für die verschiedenen Aktions-Buttons.
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(Attack);
        }
        if (magicButton != null)
        {
            magicButton.onClick.AddListener(Magic);
        }
        if (restButton != null)
        {
            restButton.onClick.AddListener(Rest);
        }

        // Zusätzliche Zauber werden zunächst versteckt.
        if (fireButton != null)
        {
            fireButton.onClick.AddListener(CastFire);
            fireButton.gameObject.SetActive(false);
        }
        if (rainButton != null)
        {
            rainButton.onClick.AddListener(CastRain);
            rainButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Macht den Hauptbutton sichtbar und aktualisiert den Zustand der Aktionsbuttons.
    /// </summary>
    public void ShowMenu()
    {
        // Hauptbutton einblenden.
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(true);
        }

        // Panel zunächst geschlossen lassen.
        if (dropUpPanel != null)
        {
            dropUpPanel.SetActive(false);
        }

        UpdateButtonStates();
    }

    /// <summary>
    /// Versteckt das gesamte Aktionsmenü.
    /// </summary>
    public void HideMenu()
    {
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
        }

        if (dropUpPanel != null)
        {
            dropUpPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Öffnet oder schließt das aufklappbare Panel.
    /// </summary>
    private void ToggleDropUp()
    {
        if (dropUpPanel != null)
        {
            // Sichtbarkeit umschalten.
            dropUpPanel.SetActive(!dropUpPanel.activeSelf);
            if (dropUpPanel.activeSelf)
            {
                // Buttons nur aktualisieren, wenn das Panel sichtbar ist.
                UpdateButtonStates();
            }
        }
    }

    /// <summary>
    /// Bereitet einen normalen Angriff vor und zeigt die Reichweite an.
    /// </summary>
    private void Attack()
    {
        if (GameManager.instance.activePlayer != null)
        {
            // Angriff vormerken und Reichweite festlegen.
            pendingAttack = PendingAttack.Physical;
            pendingRange = 2;
            MoveGrid.instance.ShowAttackRange(GameManager.instance.activePlayer.transform.position, pendingRange);
            if (dropUpPanel != null)
            {
                dropUpPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Blendet die verfügbaren Zauber ein oder aus.
    /// </summary>
    private void Magic()
    {
        bool show = !fireButton.gameObject.activeSelf;
        fireButton.gameObject.SetActive(show);
        rainButton.gameObject.SetActive(show);
        if (show)
        {
            UpdateButtonStates();
        }
    }

    /// <summary>
    /// Führt die Ruhe-Aktion aus und heilt den Spieler.
    /// </summary>
    private void Rest()
    {
        CharacterController player = GameManager.instance.activePlayer;
        if (player != null && !player.isEnemy)
        {
            int healAmount = 30;
            player.Heal(healAmount);
            Debug.Log($"Player {player.name} rests and recovers {healAmount} HP. HP now: {player.hitPoints}");
        }
        MoveGrid.instance.HideMovePoints();
        pendingAttack = PendingAttack.None;
        CompletePlayerAction();
    }

    /// <summary>
    /// Bereitet den Feuerzauber vor und zeigt die Reichweite an.
    /// </summary>
    private void CastFire()
    {
        if (GameManager.instance.activePlayer != null)
        {
            pendingAttack = PendingAttack.Fire;
            pendingRange = 4;
            MoveGrid.instance.ShowAttackRange(GameManager.instance.activePlayer.transform.position, pendingRange);
            fireButton.gameObject.SetActive(false);
            rainButton.gameObject.SetActive(false);
            if (dropUpPanel != null)
            {
                dropUpPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Bereitet den Regenzauber vor und zeigt die Reichweite an.
    /// </summary>
    private void CastRain()
    {
        if (GameManager.instance.activePlayer != null)
        {
            pendingAttack = PendingAttack.Rain;
            pendingRange = 4;
            MoveGrid.instance.ShowAttackRange(GameManager.instance.activePlayer.transform.position, pendingRange);
            fireButton.gameObject.SetActive(false);
            rainButton.gameObject.SetActive(false);
            if (dropUpPanel != null)
            {
                dropUpPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Blendet das Menü aus und beendet den Zug des Spielers.
    /// </summary>
    private void CompletePlayerAction()
    {
        HideMenu();
        GameManager.instance.EndTurn();
    }

    /// <summary>
    /// Versucht, einen vorbereiteten Angriff auf ein Ziel auszuführen.
    /// </summary>
    /// <param name="target">Das angegriffene Ziel.</param>
    /// <returns>True, wenn ein Angriff ausgeführt wurde.</returns>
    public bool TryExecuteAttackOn(CharacterController target)
    {
        if (pendingAttack == PendingAttack.None || GameManager.instance.activePlayer == null)
        {
            return false;
        }

        CharacterController player = GameManager.instance.activePlayer;
        float distance = Vector3.Distance(player.transform.position, target.transform.position);
        if (distance > pendingRange || !target.isEnemy)
        {
            // Außerhalb der Reichweite oder kein gültiges Ziel.
            return true;
        }

        int damage = 0;
        switch (pendingAttack)
        {
            case PendingAttack.Physical:
                damage = Random.Range(10, 21);
                Debug.Log($"Enemy {target.name} takes {damage} damage. HP left: {target.hitPoints - damage}");
                break;
            case PendingAttack.Fire:
                damage = Random.Range(15, 31);
                Debug.Log($"Enemy {target.name} takes {damage} fire damage. HP left: {target.hitPoints - damage}");
                break;
            case PendingAttack.Rain:
                damage = Random.Range(12, 36);
                Debug.Log($"Enemy {target.name} takes {damage} rain damage. HP left: {target.hitPoints - damage}");
                break;
        }

        // Schaden anwenden und Angriffsmodus zurücksetzen.
        target.TakeDamage(damage);
        MoveGrid.instance.HideMovePoints();
        pendingAttack = PendingAttack.None;
        CompletePlayerAction();
        return true;
    }

    /// <summary>
    /// Aktiviert oder deaktiviert die Aktionsbuttons abhängig vom Spielzustand.
    /// </summary>
    private void UpdateButtonStates()
    {
        if (attackButton == null || fireButton == null || rainButton == null)
        {
            return;
        }

        bool playerExists = GameManager.instance != null && GameManager.instance.activePlayer != null;
        bool enemyExists = GameManager.instance != null && GameManager.instance.enemyTeam.Count > 0;

        if (!playerExists || !enemyExists)
        {
            attackButton.interactable = false;
            fireButton.interactable = false;
            rainButton.interactable = false;
            return;
        }

        // Buttons werden aktiv gelassen, damit der Spieler Angriffe auswählen kann,
        // auch wenn das Ziel möglicherweise zu weit entfernt ist.
        attackButton.interactable = true;
        fireButton.interactable = true;
        rainButton.interactable = true;
    }
}
