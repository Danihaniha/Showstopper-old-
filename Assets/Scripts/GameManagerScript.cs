using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] private UI_Inventory uiInventory;

    public GameObject settingsMenuUI;
    public GameObject gameOverlayUI;
    public GameObject journalUI;

    private Inventory inventory;

    private void Awake()
    {
        inventory = new Inventory();
        uiInventory.SetInventory(inventory);
    }

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
