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
    [Header ("User Interface")]
    public GameObject widescreenMenu;
    public GameObject portraitMenu;
    public GameObject widescreenMenuBar;
    public GameObject portraitMenuBar;
    public GameObject winScreen;
    [Header ("Audio")]
    public AudioSource audioSourceMusic;
    public AudioSource audioSourceEffects;
    public AudioMixer mixer;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip leavesSound;
    [SerializeField] private AudioClip cardSound;

    [SerializeField] private ParticleSystem leavesParticle;
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
        PlaySound(Sounds.leaves);
        if (leavesParticle.isPlaying)
            leavesParticle.Stop();
        leavesParticle.Play();
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
