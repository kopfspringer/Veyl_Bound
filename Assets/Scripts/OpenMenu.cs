using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Steuert das Öffnen von Menüs und den Übergang in die Kampfszene.
/// </summary>
public class OpenMenu : MonoBehaviour
{
    /// <summary>Singleton-Instanz des Menüs.</summary>
    public static OpenMenu instance;

    private void Awake()
    {
        instance = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Beim Betreten des Triggers werden die Kampfmenüs eingeblendet.
        OpenMenu.instance.ShowBattleMenus();
    }

    /// <summary>Wurzelobjekt für das Kampfmenü.</summary>
    public GameObject battleMenu;

    /// <summary>Versteckt sämtliche Menüs.</summary>
    public void HideMenus()
    {
        battleMenu.SetActive(false);
    }

    /// <summary>Blendet das Kampfmenü ein.</summary>
    public void ShowBattleMenus()
    {
        battleMenu.SetActive(true);
    }

    /// <summary>Wechselt in die Kampfszene.</summary>
    public void EnterBattle()
    {
        SceneManager.LoadScene("Battle_1");
    }
}

