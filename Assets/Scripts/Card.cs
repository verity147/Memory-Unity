using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int pair;    //identifies a pair of cards
    public int pairNumber;  //identifies each card within a pair
    public Sprite cardback;
    public Sprite face;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = cardback;
        GetComponent<BoxCollider2D>().size = spriteRenderer.bounds.size;
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
