using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Erzeugt ein Raster aus Bewegungsfeldern und zeigt Bewegungs- bzw. Angriffsreichweiten an.
/// </summary>
public class MoveGrid : MonoBehaviour
{
    /// <summary>Singleton-Instanz für einfachen Zugriff.</summary>
    public static MoveGrid instance;

    /// <summary>Vorlage für ein Bewegungsfeld.</summary>
    public MovePoint startPoint;
    /// <summary>Bereich, in dem Felder erzeugt werden.</summary>
    public Vector2Int spawnRange;
    /// <summary>Layer, die als Boden gelten.</summary>
    public LayerMask whatIsGround;
    /// <summary>Layer, die ein Hindernis darstellen.</summary>
    public LayerMask whatIsObstacle;
    /// <summary>Radius zur Überprüfung auf Hindernisse.</summary>
    public float obstacleCheckRange;

    /// <summary>Liste aller erzeugten Bewegungsfelder.</summary>
    public List<MovePoint> allMovePoints = new List<MovePoint>();

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Initialisiert das Raster in Abhängigkeit von der aktuellen Szene.
    /// </summary>
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

        if (sceneName == "WorldMap")
        {
            OpenMenu.instance.HideMenus();
        }
        HideMovePoints();
    }

    void Update()
    {
    }

    /// <summary>
    /// Erzeugt Bewegungsfelder in einem rechteckigen Bereich um das Grid.
    /// </summary>
    public void GenerateMoveGrid()
    {
        for (int x = -spawnRange.x; x <= spawnRange.x; x++)
        {
            for (int y = -spawnRange.y; y <= spawnRange.y; y++)
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.position + new Vector3(x, 10f, y), Vector3.down, out hit, 20f, whatIsGround))
                {
                    if (Physics.OverlapSphere(hit.point, obstacleCheckRange, whatIsObstacle).Length == 0)
                    {
                        MovePoint newPoint = Instantiate(startPoint, hit.point, transform.rotation);
                        newPoint.transform.SetParent(transform);

                        allMovePoints.Add(newPoint);
                    }
                }
            }
        }
        startPoint.gameObject.SetActive(false);
    }

    /// <summary>
    /// Blendet alle Bewegungsfelder aus und setzt ihren Zustand zurück.
    /// </summary>
    public void HideMovePoints()
    {
        foreach (MovePoint movePoint in allMovePoints)
        {
            movePoint.gameObject.SetActive(false);
            movePoint.ResetColor();
            // Sicherstellen, dass die Felder beim nächsten Anzeigen wieder anklickbar sind.
            movePoint.SetClickable(true);
        }
    }

    /// <summary>
    /// Liefert alle Felder innerhalb einer bestimmten Reichweite zurück.
    /// </summary>
    public List<MovePoint> GetPointsInRange(Vector3 center, int range)
    {
        List<MovePoint> pointsInRange = new List<MovePoint>();

        foreach (MovePoint movePoint in allMovePoints)
        {
            if (Vector3.Distance(center, movePoint.transform.position) <= range)
            {
                pointsInRange.Add(movePoint);
            }
        }

        return pointsInRange;
    }

    /// <summary>
    /// Zeigt alle Bewegungsfelder innerhalb einer Reichweite an.
    /// </summary>
    public void ShowMovePointsAround(Vector3 center, int range)
    {
        HideMovePoints();

        List<MovePoint> pointsToShow = GetPointsInRange(center, range);

        foreach (MovePoint movePoint in pointsToShow)
        {
            movePoint.gameObject.SetActive(true);
            movePoint.SetClickable(true);
        }
    }

    /// <summary>
    /// Zeigt die Reichweite eines Angriffs an und färbt die Felder rot.
    /// </summary>
    public void ShowAttackRange(Vector3 center, int range)
    {
        HideMovePoints();

        List<MovePoint> pointsToShow = GetPointsInRange(center, range);

        foreach (MovePoint movePoint in pointsToShow)
        {
            movePoint.gameObject.SetActive(true);
            movePoint.SetColor(Color.red);
            // Felder während einer Angriffsauswahl nicht anklickbar machen.
            movePoint.SetClickable(false);
        }
    }
}

