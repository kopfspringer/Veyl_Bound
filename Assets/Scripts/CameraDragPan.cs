using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class CameraDragPan : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("0 = Links, 1 = Rechts, 2 = Mitte")]
    public int mouseButton = 0;          // Linke Maustaste fürs Drag-Panning
    public float panSpeed = 20f;         // Grundgeschwindigkeit (Pfeile/WASD + Drag)
    public float shiftMultiplier = 2f;   // Geschwindigkeit mit Shift gehalten
    public bool invertDragY = true;      // Maus hoch = vorwärts (RTS-Feeling)

    [Header("Bounds (optional)")]
    public bool useBounds = false;       // Weltgrenzen aktiv?
    public Vector2 minXZ = new Vector2(-50f, -50f);
    public Vector2 maxXZ = new Vector2(50f, 50f);

    [Header("Höhe/Y-Steuerung")]
    [Tooltip("Wenn true: Y auf Start-Höhe fixieren (kein Zoom über ORK). Wenn false: Y bleibt wie von ORK gesetzt (Zoom erlaubt).")]
    public bool useFixedY = false;

    private Vector3 lastMousePos;
    private bool dragging;
    private float fixedY;                // Start-Höhe merken, falls useFixedY = true

    void Start()
    {
        fixedY = transform.position.y;
    }

    void LateUpdate()
    {
        Vector3 move = Vector3.zero;

        // ---------- Drag-Panning (Maus halten & ziehen) ----------
        if (Input.GetMouseButtonDown(mouseButton))
        {
            // Optional: UI blockiert Panning
            if (IsPointerOverUI()) return;

            dragging = true;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(mouseButton))
            dragging = false;

        if (dragging && !IsPointerOverUI())
        {
            Vector3 cur = Input.mousePosition;
            Vector3 delta = cur - lastMousePos;

            // Kamera-relative Achsen (ohne Y-Anteil)
            Vector3 right = transform.right;
            Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            float dy = invertDragY ? -delta.y : delta.y;
            // Pixel -> Weltbewegung (Skalierung bewusst moderat)
            move += (-delta.x * right + dy * fwd) * (panSpeed / 1000f);

            lastMousePos = cur;
        }

        // ---------- Pfeiltasten / WASD (kamera-relativ) ----------
        float h = Input.GetAxisRaw("Horizontal"); // A/D, Pfeile links/rechts
        float v = Input.GetAxisRaw("Vertical");   // W/S, Pfeile hoch/runter
        Vector2 hv = new Vector2(h, v);
        if (hv.sqrMagnitude > 0.0f)
        {
            hv = Vector2.ClampMagnitude(hv, 1f); // kein Diagonal-Turbo
            Vector3 right = transform.right;
            Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            move += (right * hv.x + fwd * hv.y) * panSpeed * Time.deltaTime;
        }

        // ---------- Shift-Boost ----------
        if (move != Vector3.zero && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            move *= shiftMultiplier;

        // ---------- Anwenden + Grenzen + Y-Handling ----------
        if (move != Vector3.zero)
        {
            Vector3 newPos = transform.position + move;

            if (useBounds)
            {
                newPos.x = Mathf.Clamp(newPos.x, minXZ.x, maxXZ.x);
                newPos.z = Mathf.Clamp(newPos.z, minXZ.y, maxXZ.y);
            }

            if (useFixedY)
                newPos.y = fixedY; // Höhe hart fixieren (deaktiviere ORK-Zoom)
            // else: Y unverändert lassen -> ORK darf Höhe/Zoom setzen

            transform.position = newPos;
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
