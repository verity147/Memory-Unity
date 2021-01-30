using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int pairNumber;
    public int pair;
    public Sprite cardback;
    public Sprite face;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = cardback;
    }

    public void Turn()
    {
        if (spriteRenderer.sprite == cardback)
        {
            spriteRenderer.sprite = face;
        }
        else
        {
            spriteRenderer.sprite = cardback;   //animate?
        }
    }

}
