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
    public GameObject winMenu;
    public TMP_Text winText;
    public string winMessage;
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
    internal int attempts = 0;

    private void Awake()
    {
        processLayer = Camera.main.GetComponent<PostProcessLayer>();
        cardManager = FindObjectOfType<CardManager>();
    }

    private void Start()
    {
        Cursor.visible = true;
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

    public void PauseGameInput(bool pauseInput)
    {
        cardManager.EnableGameInput(!pauseInput);
    }

    public void NewGame()
    {
        cardManager.ResetCards();
        attempts = 0;
        WriteAttempt();
        audioSourceMusic.Play();
    }

    public IEnumerator WinGame()
    {
        PauseGameInput(true);
        float startvolume = audioSourceMusic.volume;
        while (audioSourceMusic.volume > 0f)
        {
            audioSourceMusic.volume -= startvolume * Time.deltaTime / 0.5f;
            yield return null;
        }
        audioSourceMusic.Stop();
        audioSourceMusic.volume = startvolume;
        if (leavesParticle.isPlaying)
            leavesParticle.Stop();
        PlaySound(Sounds.leaves);
        leavesParticle.Play();
        yield return new WaitForSeconds(leavesParticle.main.duration * 2f);
        PlaySound(Sounds.win);
        winMenu.SetActive(true);
        winText.text = string.Format(winMessage, attempts);
    }

    internal void WriteAttempt()
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
