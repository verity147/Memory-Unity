using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigCard : MonoBehaviour
{
    public bool fade = false;
    public float lerpTime = 0.5f;

    private SpriteRenderer spriteRenderer;
    private float currentLerpTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (fade)
        {
            Color currentColor = spriteRenderer.color;
            Color newColor = currentColor;
            newColor.a = (currentColor.a == 100f) ? 0 : 100;
        }
            //Color.Lerp(currentColor, newColor, 5f);
    }
}
