using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Steuert das Verhalten einer Spielfigur, inklusive Bewegung, HP-Anzeige und Angriff/Heilung.
/// </summary>
public class CharacterController : MonoBehaviour
{
    /// <summary>Geschwindigkeit, mit der sich die Figur bewegt.</summary>
    public float moveSpeed;
    /// <summary>Zielposition, die die Figur erreichen möchte.</summary>
    private Vector3 moveTarget;

    /// <summary>Optionaler NavMeshAgent, falls Navigation über Unitys NavMesh genutzt wird.</summary>
    public NavMeshAgent navAgent;
    /// <summary>Gibt an, ob die Figur sich aktuell bewegt.</summary>
    private bool isMoving;

    /// <summary>Markiert, ob die Figur ein Gegner ist.</summary>
    public bool isEnemy;

    /// <summary>Aktuelle Lebenspunkte der Figur.</summary>
    public int hitPoints = 100;
    /// <summary>Versatz der Lebensanzeigen-Anzeige über dem Charakter.</summary>
    public Vector3 hpOffset = new Vector3(0f, 2f, 0f);
    /// <summary>Wird true, während der Spieler eine Bewegung auswählt.</summary>
    private bool playerMovePending;
    /// <summary>Speichert, in welchem Zug der Gegner zuletzt gehandelt hat.</summary>
    private int lastTurnProcessed;
    /// <summary>True, wenn die Figur gestorben ist.</summary>
    private bool isDead;

    /// <summary>Transform des Elternobjekts der Lebensanzeige.</summary>
    private Transform hpBarParent;
    /// <summary>Transform des gefüllten Bereichs der Lebensanzeige.</summary>
    private Transform hpBarFill;
    /// <summary>Maximale Breite der Lebensanzeige.</summary>
    private float maxHpBarWidth = 1.5f;
    /// <summary>Höhe der Lebensanzeige.</summary>
    private float hpBarHeight = 0.2f;

    /// <summary>
    /// Wird einmal zu Beginn aufgerufen und initialisiert die Lebensanzeige.
    /// </summary>
    void Start()
    {
        moveTarget = transform.position;

        // Lebensanzeige über dem Charakter erzeugen.
        GameObject hpParentObj = new GameObject("HPBar");
        hpParentObj.transform.SetParent(transform);
        hpParentObj.transform.localPosition = hpOffset;
        hpBarParent = hpParentObj.transform;

        GameObject hpFillObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hpFillObj.name = "HPBarFill";
        hpFillObj.transform.SetParent(hpBarParent);
        hpFillObj.transform.localPosition = Vector3.zero;
        hpFillObj.transform.localScale = new Vector3(maxHpBarWidth, hpBarHeight, 0.1f);
        hpFillObj.GetComponent<Renderer>().material.color = Color.red;
        Destroy(hpFillObj.GetComponent<Collider>());
        hpBarFill = hpFillObj.transform;
        UpdateHPBar();
        if (isEnemy)
        {
            lastTurnProcessed = GameManager.instance.turnCounter;
        }
    }

    /// <summary>
    /// Wird jeden Frame aufgerufen und steuert Bewegung sowie Gegnerverhalten.
    /// </summary>
    void Update()
    {
        if (isDead)
        {
            return;
        }

        // Gegner bewegen sich automatisch, sobald sie an ihrem Ziel sind und ein neuer Zug beginnt.
        if (isEnemy && Vector3.Distance(transform.position, moveTarget) < 0.01f && lastTurnProcessed < GameManager.instance.turnCounter)
        {
            MoveTowardsPlayer();
            lastTurnProcessed = GameManager.instance.turnCounter;
        }

        // Bewegung zum gesetzten Ziel.
        if (transform.position != moveTarget)
        {
            isMoving = true;
            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);

            if (GameManager.instance.activePlayer == this)
            {
                CameraController.instance.SetMoveTarget(transform.position);
            }
        }

        // Wenn der Spieler sein Ziel erreicht hat, wird das Aktionsmenü angezeigt.
        if (!isEnemy && playerMovePending && Vector3.Distance(transform.position, moveTarget) < 0.01f)
        {
            playerMovePending = false;
            isMoving = false;

            if (GameManager.instance.activePlayer == this)
            {
                ActionMenu.instance.ShowMenu();
            }
        }

        // Lebensanzeige immer zur Kamera drehen.
        if (hpBarParent != null && Camera.main != null)
        {
            hpBarParent.LookAt(Camera.main.transform);
        }
    }

    /// <summary>
    /// Bewegt den Gegner schrittweise in Richtung Spieler.
    /// </summary>
    private void MoveTowardsPlayer()
    {
        if (GameManager.instance.playerTeam.Count == 0)
        {
            return;
        }

        Vector3 playerPos = GameManager.instance.playerTeam[0].transform.position;
        Vector3 diff = playerPos - transform.position;
        Vector3 step = Vector3.zero;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z))
        {
            step = new Vector3(Mathf.Sign(diff.x), 0f, 0f);
        }
        else if (diff.z != 0f)
        {
            step = new Vector3(0f, 0f, Mathf.Sign(diff.z));
        }

        Vector3 newTarget = transform.position + step;

        if (newTarget == playerPos)
        {
            // Alternativrichtung wählen, falls das Feld vom Spieler belegt ist.
            if (step.x != 0f && diff.z != 0f)
            {
                step = new Vector3(0f, 0f, Mathf.Sign(diff.z));
                newTarget = transform.position + step;
            }
            else if (step.z != 0f && diff.x != 0f)
            {
                step = new Vector3(Mathf.Sign(diff.x), 0f, 0f);
                newTarget = transform.position + step;
            }
        }

        if (newTarget != playerPos && step != Vector3.zero)
        {
            moveTarget = newTarget;
        }
    }

    /// <summary>
    /// Setzt ein neues Bewegungsziel für den Charakter.
    /// </summary>
    public void MoveToPoint(Vector3 pointToMoveTo)
    {
        if (isDead)
        {
            return;
        }

        moveTarget = pointToMoveTo;
        if (!isEnemy)
        {
            playerMovePending = true;
            isMoving = true;
        }
    }

    /// <summary>
    /// Reagiert auf Mausklicks auf den Charakter.
    /// </summary>
    private void OnMouseDown()
    {
        if (isDead)
        {
            return;
        }
        if (ActionMenu.instance != null && ActionMenu.instance.TryExecuteAttackOn(this))
        {
            return;
        }
        if (!isEnemy)
        {
            GameManager.instance.SelectCharacter(this);
            isMoving = true;
            ActionMenu.instance.HideMenu();
        }
    }

    /// <summary>
    /// Verarbeitet erlittenen Schaden.
    /// </summary>
    public void TakeDamage(int amount)
    {
        hitPoints -= amount;
        if (hitPoints < 0)
        {
            hitPoints = 0;
        }
        UpdateHPBar();
        if (hitPoints == 0 && !isDead)
        {
            Die();
        }
    }

    /// <summary>
    /// Heilt den Charakter um den angegebenen Betrag.
    /// </summary>
    public void Heal(int amount)
    {
        hitPoints += amount;
        if (hitPoints > 100)
        {
            hitPoints = 100;
        }
        UpdateHPBar();
    }

    /// <summary>
    /// Aktualisiert die grafische Lebensanzeige.
    /// </summary>
    private void UpdateHPBar()
    {
        if (hpBarFill != null)
        {
            float width = maxHpBarWidth * hitPoints / 100f;
            hpBarFill.localScale = new Vector3(width, hpBarHeight, 0.1f);
            hpBarFill.localPosition = new Vector3((width - maxHpBarWidth) / 2f, 0f, 0f);
        }
    }

    /// <summary>
    /// Markiert den Charakter als tot und legt ihn um.
    /// </summary>
    private void Die()
    {
        isDead = true;
        moveTarget = transform.position;
        playerMovePending = false;
        if (hpBarParent != null)
        {
            hpBarParent.gameObject.SetActive(false);
        }
        transform.rotation = Quaternion.Euler(90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
}
