using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardManager : MonoBehaviour, GameActionMap.IGameInputActions
{
    /// <summary>
    /// The card sizing logic only works with sprites that have pixels per unit 1:1 with their resolution
    /// </summary>


    [Tooltip("The margin between cards")]
    public float cardMargin = .3f;    ///margin between cards
    [Tooltip("How much larger cards get while hovering over them")]
    public Vector3 hoverScaling = new Vector3(0.1f, 0.1f, 0f);    ///added to card size when hovering
    public Sprite[] cardpictures;
    public GameObject cardPrefab;

    private GameActionMap inputActions;
    private Camera mainCamera;
    private GameObject[] cards;
    private int cameraHeight;
    private int cameraWidth;
    private Vector2 cardSizeScaled;
    private int turnedCards = 0;
    private Card card1;
    private Card card2;
    private Card currentCard;
    private Card lastCard;
    private int cardPairsLeft = 0;
    private Vector3 currentPos;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new GameActionMap();
            // Tell the "GameInput" action map that we want to get told about
            // when actions get triggered.
            inputActions.GameInput.SetCallbacks(this);
        }
        inputActions.GameInput.Enable();
    }

    private void OnDisable()
    {
        inputActions.GameInput.Disable();
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
        Shuffle();
        LayoutCards();
    }

    public void OnTurnCard(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (FindCurrentCard())
            {
                currentCard.Turn();
                turnedCards++;
                switch (turnedCards)
                {
                    case 1:
                        card1 = currentCard;
                        break;
                    case 2:
                        card2 = currentCard;
                        StartCoroutine(CompareCards());
                        turnedCards = 0;
                        break;
                    default:
                        Debug.LogWarning("Something went wrong, we shouldn't be here when no card has been turned");
                        break;
                }
            }
        }
    }

    private bool FindCurrentCard()
    {
        Ray ray = new Ray(currentPos, Vector3.forward);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider != null && hit.collider.gameObject.TryGetComponent(out Card card))
        {
            currentCard = card;
            return true;
        }
        else
            return false;
    }

    public void OnSelectCard(InputAction.CallbackContext context)
    {
        currentPos = new Vector3(mainCamera.ScreenToWorldPoint(context.ReadValue<Vector2>()).x, 
                                 mainCamera.ScreenToWorldPoint(context.ReadValue<Vector2>()).y, 
                                 mainCamera.transform.position.z);

        if (FindCurrentCard() && lastCard == null)
        {
           Hover(currentCard);
           lastCard = currentCard;
        }
        else if (!FindCurrentCard() && lastCard != null)
        {
            ScaleCard(lastCard);
            lastCard = null;
        }
        else if (FindCurrentCard() && currentCard !=lastCard)
        {
            ScaleCard(lastCard);
            Hover(currentCard);
            lastCard = currentCard;
        }
    }

    private void Hover(Card card)
    {
        card.gameObject.transform.localScale += hoverScaling;
    }

    private void ScaleCard(Card card)
    {
        card.gameObject.transform.localScale = cardSizeScaled;
    }

    private IEnumerator CompareCards()
    {
        inputActions.GameInput.Disable();
        if (card1.pair == card2.pair)
        {
            ///correct pair
            print("correct");
            yield return new WaitForSeconds(1f);
            card1.gameObject.SetActive(false);
            card2.gameObject.SetActive(false);
            //large card image & wait
            cardPairsLeft--;
            if (cardPairsLeft <= 0)
            {
                ///end of game
            }
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
            ///wrong pair
            print("wrong");
            card1.Turn();
            card2.Turn();
        }
        inputActions.GameInput.Enable();
    }

    private void InstantiateCards()
    {
        cards = new GameObject[cardpictures.Length * 2];
        cardPairsLeft = cardpictures.Length;
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
        Vector2 screenSizeWorld = mainCamera.ScreenToWorldPoint(new Vector2(cameraWidth, cameraHeight));
        Vector2 effectiveSpace = new Vector2(screenSizeWorld.x * 2 - 1, screenSizeWorld.y * 2 - 1);

        cardSizeScaled = DetermineCardSize(effectiveSpace);

        //scale cards first, then determine col and row for if(wider than tall)

        for (int i = 0; i < cards.Length; i++)
        {
            ScaleCard(cards[i].GetComponent<Card>());
        }
        float cardSpriteWidth = cards[0].GetComponent<SpriteRenderer>().bounds.size.x;

        float colNumRaw = effectiveSpace.x / (cardSpriteWidth + cardMargin);
        int cardColumns = Mathf.Clamp(Mathf.FloorToInt(colNumRaw), 1, cards.Length);
        float rowNumRaw = (float)cards.Length / cardColumns;
        int cardRows = Mathf.Clamp(Mathf.CeilToInt(rowNumRaw), 1, cards.Length);

        Vector2 cardStartPos = mainCamera.ScreenToWorldPoint(new Vector3(0, cameraHeight));
        float leftRightMargin;
        float topBotMargin;
        if (CardsWiderThanTall())
        {
            leftRightMargin = screenSizeWorld.x * 2 - cardColumns * (cardSizeScaled.x + cardMargin) + cardMargin * 2;
            topBotMargin = screenSizeWorld.y * 2 - cardRows * (cardSizeScaled.y + cardMargin) + cardMargin * 2;
        }
        else
        {
            leftRightMargin = Mathf.Abs(cardColumns * (cards[0].transform.localScale.x - (cardSpriteWidth + cardMargin)));
            topBotMargin = screenSizeWorld.y * 2 - cardRows * (cardSizeScaled.y + cardMargin) + cardMargin * 2;
        }

        cardStartPos += new Vector2(leftRightMargin / 2, -(cardSizeScaled.y / 2 + topBotMargin / 2));
        Vector2 nextCardPos = cardStartPos;

        int cardCounter = 0;
        for (int i = 0; i < cardRows; i++)
        {
            for (int j = 0; j < cardColumns; j++)
            {
                if (cardCounter >= cards.Length)
                    return;
                cards[cardCounter].transform.position = nextCardPos;
                nextCardPos += new Vector2(cardSpriteWidth + cardMargin, 0f);
                cardCounter++;
            }
            nextCardPos = new Vector2(cardStartPos.x, nextCardPos.y - (cardSizeScaled.y + cardMargin));
        }
    }

    private Vector2 DetermineCardSize(Vector2 effectiveSpace)
    {
        float effectiveSpaceArea = effectiveSpace.x * effectiveSpace.y;
        float ratio = effectiveSpace.x / effectiveSpace.y;  //can only do landscape format 
        float cardSlotArea = effectiveSpaceArea / cards.Length;
        float cardSlotWidth = Mathf.Sqrt(cardSlotArea * ratio);
        float cardSlotHeight = cardSlotArea / cardSlotWidth;
        float cardSize;
        if (CardsWiderThanTall())
        {
            cardSize = cardSlotWidth - cardMargin;
        }
        else
        {
            cardSize = cardSlotHeight - cardMargin;
        }
        cardSizeScaled = new Vector2(cardSize, cardSize);
        return cardSizeScaled;
    }

    private bool CardsWiderThanTall()
    {
        return cards[0].GetComponent<BoxCollider2D>().size.x > cards[0].GetComponent<BoxCollider2D>().size.y;
    }
}