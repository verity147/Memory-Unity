using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public enum Sounds { correct, wrong, win, leaves, card }

public class GameHandler : MonoBehaviour
{
    private enum Menus { options = 0, winScreen = 1, cardInfo = 2, info = 3 }
    [Header ("User Interface")]
    public GameObject widescreenMenuBar;
    public GameObject portraitMenuBar;
    [SerializeField] GameObject options, winScreen, cardInfo, info;
    [Header ("Audio")]
    public AudioSource audioSourceMusic;
    public AudioSource audioSourceEffects;
    public AudioMixer mixer;
    [SerializeField] private AudioClip correctSound, wrongSound, winSound, leavesSound, cardSound;
    [Header ("Effects")]
    [SerializeField] private ParticleSystem leavesParticle;
    private TMP_Text attemptsText;

    private PostProcessLayer processLayer;
    private bool toggleMenu = false;
    private CardManager cardManager;
    private GameObject menu;
    private GameObject menuBar;
    private int lastOpenMenu;

    private void Awake()
    {
        processLayer = Camera.main.GetComponent<PostProcessLayer>();
        cardManager = FindObjectOfType<CardManager>();
    }

    private void Start()
    {
        if(Screen.width > Screen.height)
        {
            menuBar = widescreenMenuBar;
        }
        else
        {
            menuBar = portraitMenuBar;
        }
        menuBar.SetActive(true);
        attemptsText = menuBar.transform.Find("Attempts_T").GetComponent<TMP_Text>();
    }

    public void MenuToggle(int menuChoice)
    {
        toggleMenu = !toggleMenu;
        switch ((Menus)menuChoice)
        {
            case Menus.options:
                menu = options;
                break;
            case Menus.winScreen:
                menu = winScreen;
                break;
            case Menus.cardInfo:
                menu = cardInfo;
                break;
            case Menus.info:
                menu = info;
                break;
            default:
                break;
        }
        menu.SetActive(toggleMenu);
        processLayer.enabled = !processLayer.enabled;
        if (toggleMenu)
        {
            cardManager.EnableGameInput(false);
            lastOpenMenu = menuChoice;
        }
        else
        {
            cardManager.EnableGameInput(true);
        }
    }

    public void NewGame(bool menu)
    {
        cardManager.ResetCards();
        if(toggleMenu)
            MenuToggle(lastOpenMenu);
        CountAttempt(0);
    }

    public void WinGame()
    {
        PlaySound(Sounds.leaves);
        if (leavesParticle.isPlaying)
            leavesParticle.Stop();
        leavesParticle.Play();
        MenuToggle(1);
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
                audioSourceEffects.PlayOneShot(correctSound);
                break;
            case Sounds.wrong:
                audioSourceEffects.PlayOneShot(wrongSound);
                break;
            case Sounds.win:
                audioSourceEffects.PlayOneShot(winSound);
                break;
            case Sounds.card:
                audioSourceEffects.PlayOneShot(cardSound);
                break;
            case Sounds.leaves:
                audioSourceEffects.PlayOneShot(leavesSound);
                break;
            default:
                break;
        }
    }

    public void SetMusicVolume(float sliderValue)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void SetEffectsVolume(float sliderValue)
    {
        mixer.SetFloat("EffectsVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
