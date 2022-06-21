using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public bool begin;

    public void BeginGame()
    {
        begin = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
