using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class TacticsCameraController : MonoBehaviour
{
    [Header("Rotate (Right Mouse)")]
    public int rotateMouseButton = 1;        // 1 = rechte Maustaste
    public float rotateSpeed = 2000f;         // °/Sek. pro Maus-Delta (gefühlt)
    public float minPitch = 20f;
    public float maxPitch = 70f;

    [Header("Pan (Left Mouse + WASD/Arrows)")]
    public int panMouseButton = 0;           // 0 = linke Maustaste
    public float panSpeed = 10f;             // Welt-Einh./Sek. (Tasten)
    public float dragPanFactor = 0.03f;      // Welt-Einh./Pixel (Maus-Drag)
    public float shiftMultiplier = 2f;       // Shift = schneller

    [Header("Zoom (Mouse Wheel)")]
    public bool zoomAsDolly = true;          // true = vor/zurück entlang Blickrichtung
    public float zoomSpeed = 40f;            // Dolly-Geschw./Sek. (bei Rad=1)
    public float minDistanceFromGround = 3f; // einfache Boden-Sicherheit (Y-Min)
    public float maxDistance = 100f;         // Sicherheitslimit

    [Header("Bounds (optional)")]
    public bool useBounds = false;
    public Vector2 minXZ = new Vector2(-50, -50);
    public Vector2 maxXZ = new Vector2(50, 50);

    [Header("Misc")]
    public bool invertDragY = true;          // Maus hoch = vorwärts (RTS-Feeling)
    public bool blockOverUI = true;          // Drag/Rotate ignorieren, wenn Cursor über UI

    float yaw;   // aktuelle Yaw in °
    float pitch; // aktuelle Pitch in °
    Vector3 lastMousePos;

    void Start()
    {
        // Startwinkel aus aktueller Kamera übernehmen
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = NormalizePitch(e.x);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        ApplyRotation();
        lastMousePos = Input.mousePosition;
    }

    void LateUpdate()
    {
        Vector3 move = Vector3.zero;

        // --- ROTATE (rechte Maustaste ziehen) ---
        if (Input.GetMouseButton(rotateMouseButton) && !IsPointerOverUI())
        {
            Vector2 delta = (Vector2)(Input.mousePosition - lastMousePos);
            yaw += delta.x * rotateSpeed * Time.deltaTime * 0.01f;
            pitch -= delta.y * rotateSpeed * Time.deltaTime * 0.01f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            ApplyRotation();
        }

        // Kamera-relative Achsen (ohne Y)
        Vector3 right = transform.right;
        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        // --- PAN (linke Maustaste ziehen) ---
        if (Input.GetMouseButton(panMouseButton) && !IsPointerOverUI())
        {
            Vector2 delta = (Vector2)(Input.mousePosition - lastMousePos);
            float dy = invertDragY ? -delta.y : delta.y;
            move += (-delta.x * right + dy * fwd) * (dragPanFactor);
        }

        // --- PAN (WASD / Pfeiltasten) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(h) > 0.001f || Mathf.Abs(v) > 0.001f)
        {
            Vector2 hv = Vector2.ClampMagnitude(new Vector2(h, v), 1f);
            move += (right * hv.x + fwd * hv.y) * panSpeed * Time.deltaTime;
        }

        // --- Shift-Boost ---
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            move *= shiftMultiplier;

        // --- ZOOM (Mausrad) ---
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(wheel) > 0.0001f)
        {
            if (zoomAsDolly)
            {
                Vector3 dolly = transform.forward * (wheel * zoomSpeed * Time.deltaTime * 100f);
                transform.position += dolly;

                // einfache Sicherheitslimits
                transform.position = ClampDistanceAndHeight(transform.position);
            }
            else
            {
                // Alternative: Zoom als Höhenänderung (Y)
                float dy = wheel * zoomSpeed * Time.deltaTime * 100f;
                Vector3 p = transform.position;
                p.y = Mathf.Clamp(p.y + dy, minDistanceFromGround, maxDistance);
                transform.position = p;
            }
        }

        // --- Anwenden: Pan + Bounds ---
        if (move != Vector3.zero)
        {
            Vector3 p = transform.position + move;
            if (useBounds)
            {
                p.x = Mathf.Clamp(p.x, minXZ.x, maxXZ.x);
                p.z = Mathf.Clamp(p.z, minXZ.y, maxXZ.y);
            }
            // Mindesthöhe halten
            if (p.y < minDistanceFromGround) p.y = minDistanceFromGround;
            transform.position = p;
        }

        lastMousePos = Input.mousePosition;
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    Vector3 ClampDistanceAndHeight(Vector3 pos)
    {
        // harte Y-Min + Abstandslimit (einfacher Schutz)
        if (pos.y < minDistanceFromGround) pos.y = minDistanceFromGround;
        Vector3 originFlat = new Vector3(0, pos.y, 0);
        float d = Vector3.Distance(pos, originFlat);
        if (d > maxDistance)
        {
            Vector3 dir = (pos - originFlat).normalized;
            pos = originFlat + dir * maxDistance;
        }
        return pos;
    }

    bool IsPointerOverUI()
    {
        return blockOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    float NormalizePitch(float x)
    {
        // Unity liefert 0..360 – wir wollen -180..180
        if (x > 180f) x -= 360f;
        return x;
    }
}
