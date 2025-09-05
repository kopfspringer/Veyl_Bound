using UnityEngine;

/// <summary>
/// Steuert die Kamerabewegung und erlaubt das Ziehen mit der Maus.
/// </summary>
public class CameraController : MonoBehaviour
{

    /// <summary>Globale Referenz auf den Kamera-Controller.</summary>
    public static CameraController instance;

    private void Awake()
    {
        // Singleton-Instanz setzen.
        instance = this;
    }

    /// <summary>Geschwindigkeit, mit der sich die Kamera zu ihrem Ziel bewegt.</summary>
    public float moveSpeed;
    /// <summary>Zielposition, zu der die Kamera gleitet.</summary>
    private Vector3 moveTarget;
    /// <summary>Faktor, wie stark die Kamera bei Mausbewegungen verschoben wird.</summary>
    public float dragSpeed = 0.1f;
    /// <summary>Letzte Mausposition zur Berechnung der Bewegung.</summary>
    private Vector3 lastMousePosition;

    /// <summary>
    /// Start wird einmalig nach der Initialisierung aufgerufen.
    /// </summary>
    void Start()
    {
        // Anfangsziel ist die aktuelle Position der Kamera.
        moveTarget = transform.position;
    }

    /// <summary>
    /// Aktualisiert die Kamera jede Frame.
    /// </summary>
    void Update()
    {
        HandleMouseDrag();

        // Bewege die Kamera sanft zum Ziel.
        if (moveTarget != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Ermöglicht das Verschieben der Kamera durch Ziehen mit der linken Maustaste.
    /// </summary>
    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Drag beginnt: aktuelle Maus- und Kameraposition merken.
            lastMousePosition = Input.mousePosition;
            moveTarget = transform.position;
        }
        else if (Input.GetMouseButton(0))
        {
            // Delta der Mausbewegung berechnen.
            Vector3 delta = Input.mousePosition - lastMousePosition;
            // Kamera in entgegengesetzter Richtung verschieben.
            Vector3 move = new Vector3(-delta.x, 0f, -delta.y) * dragSpeed;
            transform.position += move;

            lastMousePosition = Input.mousePosition;
            moveTarget = transform.position;
        }
    }

    /// <summary>
    /// Setzt ein neues Ziel für die Kamera.
    /// </summary>
    public void SetMoveTarget(Vector3 newTarget)
    {
        moveTarget = newTarget;
    }
}
