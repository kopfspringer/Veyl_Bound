using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Steuert das �ffnen von Men�s und den �bergang in die Kampfszene.
/// </summary>
public class OpenMenu : MonoBehaviour
{
    /// <summary>Singleton-Instanz des Men�s.</summary>
    public static OpenMenu instance;

    private void Awake()
    {
        instance = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Beim Betreten des Triggers werden die Kampfmen�s eingeblendet.
        OpenMenu.instance.ShowBattleMenus();
    }

    /// <summary>Wurzelobjekt f�r das Kampfmen�.</summary>
    public GameObject battleMenu;

    /// <summary>Versteckt s�mtliche Men�s.</summary>
    public void HideMenus()
    {
        battleMenu.SetActive(false);
    }

    /// <summary>Blendet das Kampfmen� ein.</summary>
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

