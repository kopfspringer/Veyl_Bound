using GamingIsLove.ORKFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zentrale Steuerung des Spiels: verwaltet Züge, Teams und den aktiven Spieler.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>Singleton-Instanz des GameManagers.</summary>
    public static GameManager instance;

    private void Awake()
    {
        // Instanz setzen, um einfachen Zugriff zu ermöglichen.
        instance = this;
    }

    /// <summary>Der Charakter, der aktuell am Zug ist.</summary>
    public CharacterController activePlayer;

    /// <summary>Liste aller Charaktere im Spiel.</summary>
    public List<CharacterController> allChars = new List<CharacterController>();
    /// <summary>Liste der Spielercharaktere.</summary>
    public List<CharacterController> playerTeam = new List<CharacterController>();
    /// <summary>Liste der Gegner.</summary>
    public List<CharacterController> enemyTeam = new List<CharacterController>();

    /// <summary>Zähler der bisherigen Züge des Spielers.</summary>
    public int turnCounter = 0;
    /// <summary>Index des aktuell aktiven Charakters.</summary>
    private int currentCharIndex;

    /// <summary>
    /// Sucht alle Charaktere, ordnet sie Teams zu und startet den ersten Zug.
    /// </summary>
    void Start()
    {
        allChars.AddRange(FindObjectsByType<CharacterController>(FindObjectsSortMode.None));

        foreach (CharacterController cc in allChars)
        {
            if (cc.isEnemy == false)
            {
                playerTeam.Add(cc);
            }
            else
            {
                enemyTeam.Add(cc);
            }
        }

        allChars.Clear();

        allChars.AddRange(playerTeam);
        allChars.AddRange(enemyTeam);

        currentCharIndex = 0;
        if (allChars.Count > 0)
        {
            activePlayer = allChars[currentCharIndex];
            CameraController.instance.SetMoveTarget(activePlayer.transform.position);

            if (activePlayer.isEnemy)
            {
                StartCoroutine(EnemyTurn());
            }
        }
        if (activePlayer == null && playerTeam.Count > 0)
        {
            activePlayer = playerTeam[0];
        }

        // Das Aktionsmenü erscheint erst, wenn der Spieler seine Bewegung beendet hat.
    }

    /// <summary>
    /// Wählt einen Charakter aus und zeigt seine Bewegungsreichweite an.
    /// </summary>
    public void SelectCharacter(CharacterController cc)
    {
        if (cc == activePlayer && !cc.isEnemy)
        {
            MoveGrid.instance.ShowMovePointsAround(cc.transform.position, 5);
        }
    }

    /// <summary>
    /// Bewegt den aktiven Spieler zu einem Zielpunkt.
    /// </summary>
    public void MoveActivePlayerToPoint(Vector3 point)
    {
        if (activePlayer != null && !activePlayer.isEnemy)
        {
            activePlayer.MoveToPoint(point);
            MoveGrid.instance.HideMovePoints();
        }
    }

    /// <summary>
    /// Wird aufgerufen, wenn ein Charakter seine Bewegung abgeschlossen hat.
    /// </summary>
    public void CharacterFinishedMove(CharacterController cc)
    {
        if (cc == activePlayer)
        {
            if (!cc.isEnemy)
            {
                if (BattleMenu.instance != null)
                {
                    BattleMenu.instance?.Show();
                }
                else
                {
                    EndTurn();
                }

            }
            else
            {
                EndTurn();
            }
        }
    }

    /// <summary>
    /// Beendet den aktuellen Zug und wählt den nächsten Charakter aus.
    /// </summary>
    public void EndTurn()
    {
        BattleMenu.instance?.Hide();
        ActionMenu.instance.HideMenu();

        bool playerFinishedTurn = !activePlayer.isEnemy;

        if (playerFinishedTurn)
        {
            turnCounter++;
        }

        currentCharIndex++;
        if (currentCharIndex >= allChars.Count)
        {
            currentCharIndex = 0;
        }

        activePlayer = allChars[currentCharIndex];

        CameraController.instance.SetMoveTarget(activePlayer.transform.position);

        if (activePlayer.isEnemy)
        {
            StartCoroutine(EnemyTurn());
        }
        // Das Aktionsmenü wird angezeigt, sobald der neue aktive Spieler sein Ziel erreicht hat.
    }

    /// <summary>
    /// Führt den Zug eines Gegners aus.
    /// </summary>
    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);

        if (playerTeam.Count > 0)
        {
            CharacterController player = playerTeam[0];
            CharacterController enemy = activePlayer;
            if (enemy != null)
            {
                float distance = Vector3.Distance(enemy.transform.position, player.transform.position);
                if (distance <= 2f)
                {
                    int damage = Random.Range(10, 21);
                    player.TakeDamage(damage);
                    Debug.Log($"Player {player.name} takes {damage} damage. HP left: {player.hitPoints}");
                }
                else
                {
                    Debug.Log("Player is too far away to be attacked.");
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        EndTurn();
    }
}
