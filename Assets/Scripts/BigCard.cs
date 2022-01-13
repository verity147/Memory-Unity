using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigCard : MonoBehaviour
{
    public float lerpTime = 0.5f;
    [HideInInspector]
    public bool fade = false;

    private SpriteRenderer spriteRenderer;
    private float currentLerpTime;
    private Color newColor;
    private Color currentColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        newColor = spriteRenderer.color;

    }
    private void Update()
    {
        if (fade)
        {
            currentColor = spriteRenderer.color;
            newColor.a = (currentColor.a == 0f) ? 1f : 0f;
            currentLerpTime = 0f;
            fade = false;
        }
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime <= lerpTime)
        {
            float perc = currentLerpTime / lerpTime;
            spriteRenderer.color = Color.Lerp(currentColor, newColor, perc);
        }
    }

    internal void MakeTransparent()
    {
        Color transparent = new Color(1f, 1f, 1f, 0f);
        spriteRenderer.color = transparent;
    }
}
