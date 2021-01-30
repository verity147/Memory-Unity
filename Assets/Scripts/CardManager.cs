using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardManager : MonoBehaviour
{
    private Camera mainCamera;
    private int cameraHeight;
    private int cameraWidth;
    public GameObject[] cards;
    public Sprite[] cardpictures;

    public InputAction turnCardAction;
    public InputAction selectCardAction;

    public int amountCards;
    public GameObject cardPrefab;

    private void OnEnable()
    {
        turnCardAction.Enable();
        selectCardAction.Enable();
    }

    private void OnDisable()
    {
        turnCardAction.Disable();
        selectCardAction.Disable();
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        cameraHeight = mainCamera.pixelHeight;
        cameraWidth = mainCamera.pixelWidth;
    }

    private void Start()
    {
        InstantiateCards();
    }

    private void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(selectCardAction.ReadValue<Vector2>());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log(hit.collider.gameObject.name);
            //do hover animation
            if (turnCardAction.triggered && hitObject.GetComponent<Card>())
            {
                //check for card, stop hover animation                
                Debug.Log(hit.collider.gameObject.name + " turned");
                hitObject.GetComponent<Card>().Turn();
            }
        }

    }

    private void InstantiateCards()
    {
        cards = new GameObject[cardpictures.Length * 2];
        int cardsIndexHelper = 0;
        for (int i = 0; i < cardpictures.Length; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                GameObject cardObject = Instantiate(cardPrefab);
                Card card = cardObject.GetComponent<Card>();
                card.face = cardpictures[i];
                card.pair = i;
                card.pairNumber = j;
                cards.SetValue(cardObject, cardsIndexHelper);
                cardsIndexHelper++;
            }
        }
        LayoutCards();
    }
                               
    private void LayoutCards()
    {
        //height and width need to account for margins
        int pixelsPerUnit = (int)cards[0].GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        int height = Mathf.FloorToInt(cameraHeight / pixelsPerUnit);
        int width= Mathf.FloorToInt(cameraWidth / pixelsPerUnit);
        int rest = cards.Length % (height * width);
        int yPos = cameraHeight;
        int xPos = 0;

        foreach (GameObject card in cards)
        {
            Vector3 pixelPos = new Vector3(xPos, yPos, 0f);
            card.transform.position = mainCamera.ScreenToWorldPoint(pixelPos) - new Vector3(-0.5f, 0.5f, -10f);
            xPos += pixelsPerUnit;
            if (xPos > cameraWidth)
            {
                xPos = 0;
                yPos -= pixelsPerUnit;
            }

        }
        if (rest == 0)
        {
            //proceed, check if cards could be larger
        }
        else if(rest < width)
        {
            //just one line of cards more than space
            height++;
        }
        else if(rest > width)
        {
            //more than one row too many
        }
    }

}