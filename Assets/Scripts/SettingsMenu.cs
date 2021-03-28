using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsMenuUI;
    public GameObject gameOverlayUI;

    public void ResumeGame()
    {
        settingsMenuUI.SetActive(false);
        gameOverlayUI.SetActive(true);
    }

    public void SettingsPage()
    {
        settingsMenuUI.SetActive(true);
        gameOverlayUI.SetActive(false);
    }
}
