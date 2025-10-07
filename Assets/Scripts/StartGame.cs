using GamingIsLove.ORKFramework;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public void startGame()
    {
        ORK.Game.NewGame(true);
    }
}