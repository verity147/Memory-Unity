using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using TMPro;

public class GameHandler : MonoBehaviour
{
    public GameObject widescreenMenu;
    public GameObject portraitMenu;
    public GameObject widescreenMenuBar;
    public GameObject portraitMenuBar;
    private TMP_Text attemptsText;

    private PostProcessLayer processLayer;
    private bool toggleMenu = false;
    private CardManager cardManager;
    private GameObject menu;
    private GameObject menuBar;

    private void Awake()
    {
        processLayer = Camera.main.GetComponent<PostProcessLayer>();
        cardManager = FindObjectOfType<CardManager>();
    }

    private void Start()
    {
        if(Screen.width > Screen.height)
        {
            menu = widescreenMenu;
            menuBar = widescreenMenuBar;
        }
        else
        {
            menu = portraitMenu;
            menuBar = portraitMenuBar;
        }
        menuBar.SetActive(true);
        attemptsText = menuBar.transform.Find("Attempts_T").GetComponent<TMP_Text>();
    }

    public void MenuToggle()
    {
        toggleMenu = !toggleMenu;
        menu.SetActive(toggleMenu);
        processLayer.enabled = !processLayer.enabled;
        if (toggleMenu)
        {
            cardManager.EnableGameInput(false);
        }
        else
        {
            cardManager.EnableGameInput(true);
        }
    }

    public void NewGame()
    {
        cardManager.ResetCards();
        MenuToggle();
        CountAttempt(0);
    }

    internal void CountAttempt(int attempts)
    {
        attemptsText.text = string.Format("{0} Versuche", attempts);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
