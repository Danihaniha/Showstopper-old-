using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsMenuUI;
    public GameObject gameOverlayUI;
    public GameObject journalUI;


    public void ResumeGame()
    {
        settingsMenuUI.SetActive(false);
        journalUI.SetActive(false);
        gameOverlayUI.SetActive(true);
    }

    public void SettingsPage()
    {
        settingsMenuUI.SetActive(true);
        journalUI.SetActive(false);
        gameOverlayUI.SetActive(false);
    }

    public void JournalPage()
    {
        journalUI.SetActive(true);
        settingsMenuUI.SetActive(false);
        gameOverlayUI.SetActive(false);
    }
}
