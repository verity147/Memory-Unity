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
    private readonly float cardMargin = .2f; ///the margin as a fraction of the cardsize
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
        //height and width need to account for margins
        Vector2 cardSize = cards[0].GetComponent<SpriteRenderer>().sprite.bounds.size;
        Vector2 effectiveSpace = new Vector2(cameraSize.x - 2 * cardSize.x * cardMargin, cameraSize.y - 2 * cardSize.y * cardMargin);
        print(effectiveSpace);
        Vector2 cardStartPos = mainCamera.ScreenToWorldPoint(new Vector3(0, cameraHeight));
        print(cardStartPos);
        cardStartPos += new Vector2(cardSize.x * cardMargin * 2, -(cardSize.y * cardMargin * 2));
        print(cardStartPos);
        Vector2 nextCardPos = cardStartPos;


        // 1)check for appropriate card size, 2)scale them, 3)lay them out
        foreach (GameObject card in cards)
        {
            card.transform.position = nextCardPos;
            nextCardPos += new Vector2(cardSize.x + cardSize.x * cardMargin, 0f);
            if (nextCardPos.x > mainCamera.ScreenToWorldPoint(new Vector3(cameraWidth,cameraHeight,0f)).x)
            {
                nextCardPos = new Vector2(cardStartPos.x, nextCardPos.y - cardSize.y + cardSize.y * cardMargin);
            }

        }


        int cardsX = Mathf.FloorToInt(effectiveSpace.x / cardSize.x);
        int cardsY = Mathf.FloorToInt(effectiveSpace.y / cardSize.y);
        int rest = cards.Length % (cardsY * cardsX);
        if (rest == 0)
        {
            //proceed, check if cards could be larger
        }
        else if(rest < cardsX)
        {
            //just one line of cards more than space
            cardsY++;
        }
        else if(rest > cardsX)
        {
            //more than one row too many
        }
    }

}