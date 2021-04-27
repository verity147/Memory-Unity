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
    private Vector2 cardSizeScaled;
    private int amountCardsX;
    private int amountCardsY;

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
            //Debug.Log(hit.collider.gameObject.name);
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
        Vector2 screenSizeWorld = mainCamera.ScreenToWorldPoint(new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight));
        print("SSW: " + screenSizeWorld);

        Vector2 effectiveSpace = new Vector2(screenSizeWorld.x*2 - 2 * cardMargin, screenSizeWorld.y*2 - 2 * cardMargin);
        print(effectiveSpace);

        cardSizeScaled = ScaleCards(cardSize, effectiveSpace);
        Vector2 cardStartPos = mainCamera.ScreenToWorldPoint(new Vector3(0, cameraHeight));

        float leftRightMargin = screenSizeWorld.x * 2 - amountCardsX * (cardSizeScaled.x + cardMargin) + cardMargin * 2;
        float topBotMargin = screenSizeWorld.y * 2 - amountCardsY * (cardSizeScaled.y + cardMargin) + cardMargin * 2;

        cardStartPos += new Vector2(cardSizeScaled.x / 2 + leftRightMargin / 2, -(cardSizeScaled.y / 2 + topBotMargin / 2));
        Vector2 nextCardPos = cardStartPos;

        int cardCounter = 0;
        for (int i = 0; i < amountCardsY; i++)
        {
            for (int j = 0; j < amountCardsX; j++)
            {
                if (cardCounter > cards.Length - 1)
                    return;
                cards[cardCounter].transform.localScale = cardSizeScaled;
                cards[cardCounter].transform.position = nextCardPos;
                nextCardPos += new Vector2(cardSizeScaled.x + cardMargin, 0f);
                cardCounter++;
            }
            if (cardCounter > cards.Length - 1)
                return;
            nextCardPos = new Vector2(cardStartPos.x, nextCardPos.y - (cardSizeScaled.y + cardMargin));
            cards[cardCounter].transform.localScale = cardSizeScaled;
            cards[cardCounter].transform.position = nextCardPos;
            cardCounter++;
        }
    }

    private Vector2 ScaleCards(Vector2 cardSize, Vector2 effectiveSpace)
    {
        amountCards = cards.Length;
        amountCardsX = Mathf.FloorToInt(effectiveSpace.x / (cardSize.x + cardMargin));
        amountCardsY = Mathf.CeilToInt(amountCards / amountCardsX);
        int stop = 0;

        while ((effectiveSpace.y - amountCardsY * (cardSizeScaled.y + cardMargin) > cardSizeScaled.y + cardMargin || 
               effectiveSpace.y - amountCardsY * (cardSizeScaled.y + cardMargin) < 0) && stop < 500)
        {
            if (amountCardsY * (cardSizeScaled.y + cardMargin) > effectiveSpace.y)
            {
                print("too large");
                amountCardsX++;
            }
            else if (effectiveSpace.y - amountCardsY * (cardSizeScaled.y + cardMargin) > cardSizeScaled.y + cardMargin)
            {
                print("too small");
                amountCardsX--;
            }
            amountCardsY = Mathf.CeilToInt(amountCards / amountCardsX);
            float size = effectiveSpace.x / amountCardsX - cardMargin;
            cardSizeScaled = new Vector2(size, size);
            stop++;
        }

            //correct size, remove margins for actual card only size
            cardSizeScaled = new Vector2(cardSizeScaled.x - cardMargin, cardSizeScaled.y - cardMargin);
            print("size correct");

        return cardSizeScaled;
    }

}