using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Wechseln in eine andere Szene, wenn ein Trigger betreten wird.
/// </summary>
public class SceneChanger : MonoBehaviour
{
    /// <summary>
    /// Lädt beim Betreten des Triggers die Szene "Battle_1".
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene("World");
    }
}
