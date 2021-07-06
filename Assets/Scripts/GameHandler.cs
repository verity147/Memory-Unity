using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public enum Sounds { correct, wrong }

public class GameHandler : MonoBehaviour
{
    [Header ("User Interface")]
    public GameObject widescreenMenu;
    public GameObject portraitMenu;
    public GameObject widescreenMenuBar;
    public GameObject portraitMenuBar;
    public GameObject winScreen;
    [Header ("Audio")]
    public AudioSource audioSource;
    public AudioMixer mixer;
    public AudioClip correctSound;
    public AudioClip wrongSound;

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

    public void NewGame(bool menu)
    {
        cardManager.ResetCards();
        if(menu)
            MenuToggle();
        CountAttempt(0);
    }

    public void WinGame()
    {
        winScreen.SetActive(true);
    }

    internal void CountAttempt(int attempts)
    {
        attemptsText.text = string.Format("{0} Versuche", attempts);
    }

    internal void PlaySound(Sounds sound)
    {
        switch (sound)
        {
            case Sounds.correct:
                audioSource.PlayOneShot(correctSound);
                break;
            case Sounds.wrong:
                audioSource.PlayOneShot(wrongSound);
                break;
            default:
                break;
        }
    }

    public void SetVolume(float sliderValue)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
