using UnityEngine;

/// <summary>
/// Repr�sentiert ein einzelnes Feld im Bewegungsraster.
/// </summary>
public class MovePoint : MonoBehaviour
{
    /// <summary>Renderer zum �ndern der Farbe des Feldes.</summary>
    private Renderer rend;
    /// <summary>Standardfarbe des Feldes.</summary>
    private Color defaultColor;
    /// <summary>Kollisionskomponente, um Klicks zu erm�glichen oder zu blockieren.</summary>
    private Collider col;

    private void Awake()
    {
        // Komponentenreferenzen holen.
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();

        if (rend != null)
        {
            defaultColor = rend.material.color;
        }
    }

    /// <summary>
    /// Setzt eine neue Farbe f�r das Feld.
    /// </summary>
    public void SetColor(Color color)
    {
        if (rend != null)
        {
            rend.material.color = color;
        }
    }

    /// <summary>
    /// Stellt die urspr�ngliche Farbe wieder her.
    /// </summary>
    public void ResetColor()
    {
        SetColor(defaultColor);
    }

    /// <summary>
    /// Aktiviert oder deaktiviert die Klickbarkeit des Feldes.
    /// </summary>
    public void SetClickable(bool clickable)
    {
        if (col != null)
        {
            col.enabled = clickable;
        }
    }

    /// <summary>
    /// Reagiert auf Mausklicks und bewegt den aktiven Spieler zu diesem Feld.
    /// </summary>
    private void OnMouseDown()
    {
        GameManager.instance.MoveActivePlayerToPoint(transform.position);
    }
}
