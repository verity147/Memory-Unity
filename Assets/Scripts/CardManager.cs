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
    private readonly float cardMargin = .2f; 
    public GameObject[] cards;
    private Vector2 cameraSize;

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
        cameraSize = new Vector2(mainCamera.orthographicSize * 2 * mainCamera.aspect, mainCamera.orthographicSize * 2);
    }

    private void Start()
    {
        InstantiateCards();
        Shuffle();
        LayoutCards();
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
    }

    public void Shuffle()
    {
        GameObject temp;
        for (int i = 0; i < cards.Length - 1; i++)
        {
            int rnd = UnityEngine.Random.Range(i, cards.Length);
            temp = cards[rnd];
            cards[rnd] = cards[i];
            cards[i] = temp;
        }
    }

    private void LayoutCards()
    {
        Vector2 cardSize = cards[0].GetComponent<SpriteRenderer>().sprite.bounds.size;
        Vector2 screenSizeWorld = new Vector2(mainCamera.aspect * mainCamera.orthographicSize * 2, mainCamera.orthographicSize * 2);
        print(screenSizeWorld);

        Vector2 effectiveSpace = new Vector2(screenSizeWorld.x - 2 * cardMargin, screenSizeWorld.y - 2 * cardMargin);
        print(effectiveSpace);
        //float cardSpace = cards.Length * ((cardSize.x + cardMargin)* (cardSize.y + cardMargin));
        //print(cardSpace);

        //float cardSizeFactor = (effectiveSpace.x * effectiveSpace.y / cardSpace);
        //print(cardSizeFactor);
        ////float cardSizeFactor = 3.91f;
        //Vector2 cardSizeScaled = new Vector2(cardSize.x * cardSizeFactor - cardMargin, cardSize.y * cardSizeFactor - cardMargin);

        Vector2 cardSizeScaled = ScaleCards(cardSize, effectiveSpace);
        Vector2 cardStartPos = mainCamera.ScreenToWorldPoint(new Vector3(0, cameraHeight));
        cardStartPos += new Vector2(cardMargin * 2 + cardSizeScaled.x / 2, 
                                    -(cardMargin * 2 + cardSizeScaled.y / 2));
        Vector2 nextCardPos = cardStartPos;

        foreach (GameObject card in cards)
        {
            card.transform.localScale = cardSizeScaled;
            if (nextCardPos.x + cardSizeScaled.x/2 + cardMargin/2 > mainCamera.ScreenToWorldPoint(new Vector3(cameraWidth, cameraHeight, 0f)).x)//right edge of camera
            {
                nextCardPos = new Vector2(cardStartPos.x, nextCardPos.y - (cardSizeScaled.y + cardMargin));
                card.transform.position = nextCardPos;

            }
            else
            {
                card.transform.position = nextCardPos;
                nextCardPos += new Vector2(cardSizeScaled.x + cardMargin, 0f);
            }
        }
    }

    private Vector2 ScaleCards(Vector2 cardSize, Vector2 effectiveSpace)
    {
        int amountCardsX = Mathf.FloorToInt(effectiveSpace.x / cardSize.x + cardMargin);
        int amountCardsY = Mathf.CeilToInt(amountCards / amountCardsX); 
        Vector2 cardSizeScaled = cardSize;

        //repeat until last else if is fulfilled, then return
        if (amountCardsY * (cardSizeScaled.y + cardMargin) > effectiveSpace.y)
        {
            //cardsize down so amountCardsX is +1
            amountCardsX++;
            amountCardsY = Mathf.CeilToInt(amountCards / amountCardsX);
            cardSizeScaled = new Vector2(effectiveSpace.x / (amountCardsX) * (cardSizeScaled.x + cardMargin), cardSizeScaled.x);
        }
        else if (effectiveSpace.y - amountCardsY * (cardSizeScaled.y + cardMargin) > cardSizeScaled.y + cardMargin)
        {
            //cardsize up so amountcardsx is -1
            amountCardsX--;
            amountCardsY = Mathf.CeilToInt(amountCards / amountCardsX);
            cardSizeScaled = new Vector2(effectiveSpace.x / (amountCardsX) * (cardSizeScaled.x + cardMargin), cardSizeScaled.x);
        }
        else if (effectiveSpace.y - amountCardsY * (cardSizeScaled.y + cardMargin) < cardSizeScaled.y + cardMargin)
        {
            //correct size, remove margins for actual card only size
            cardSizeScaled = new Vector2(cardSizeScaled.x - cardMargin, cardSizeScaled.y);
        }

        return cardSizeScaled;
    }
}