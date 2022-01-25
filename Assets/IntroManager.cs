using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Canvas canvas;
    public float showTitle = 1f;

    private void Start()
    {
        StartCoroutine(IntroTiming());
        Cursor.visible = false;
    }

    private IEnumerator IntroTiming()
    {
        yield return new WaitForSeconds((float)videoPlayer.clip.length);
        videoPlayer.gameObject.SetActive(false);
        canvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(showTitle);
        SceneManager.LoadSceneAsync(1);
    }
}
