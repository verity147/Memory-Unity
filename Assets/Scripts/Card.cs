using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int pair;    //identifies a pair of cards
    public int pairNumber;  //identifies each card within a pair
    public Sprite cardback;
    public Sprite face;
    public int spriteNumber;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = cardback;
        GetComponent<BoxCollider2D>().size = spriteRenderer.bounds.size;
    }

    private void Start()
    {
        bool findNum = int.TryParse(face.name.Substring(4, 2), out spriteNumber);
        if (!findNum)
            Debug.LogError("Could not determine valid spriteNumber!");        
    }

    public void Turn()
    {
        if (spriteRenderer.sprite == cardback)
        {
            spriteRenderer.sprite = face;
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            spriteRenderer.sprite = cardback;   //animate?
            gameObject.GetComponent<Collider2D>().enabled = true;
        }
    }

}
