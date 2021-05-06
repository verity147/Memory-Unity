using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardManager : MonoBehaviour, GameActionMap.IGameInputActions
{
    private GameActionMap inputActions;

    private Camera mainCamera;
    private int cameraHeight;
    private int cameraWidth;
    private readonly float cardMargin = .3f;    ///margin between cards
    private readonly Vector3 hoverScaling = new Vector3(0.1f, 0.1f, 0f);    ///added to card size when hovering
    private Vector2 cardSizeScaled;
    private int amountCardsX;
    private int amountCardsY;
    private int turnedCards = 0;
    private Card card1;
    private Card card2;
    private Card currentCard;
    private Card lastCard;
    private int cardPairsLeft = 0;
    private Vector3 currentPos;
    private int amountCards;

    public GameObject[] cards;
    public Sprite[] cardpictures;
    public GameObject cardPrefab;

    /// <summary>
    /// The card sizing logic only works with sprites that have pixels per unit 1:1 with their resolution
    /// </summary>

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
        Vector2 effectiveSpace = new Vector2(screenSizeWorld.x * 2 - 2 * cardMargin, screenSizeWorld.y * 2 - 2 * cardMargin);

        Vector2 cardSize = cards[0].GetComponent<SpriteRenderer>().sprite.bounds.size;
        cardSizeScaled = cardSize;
        print(cardSize);
        cardSizeScaled = DetermineCardSize(cardSize, effectiveSpace);
        print(cardSizeScaled);
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
                ScaleCard(cards[cardCounter].GetComponent<Card>());
                cards[cardCounter].transform.position = nextCardPos;
                nextCardPos += new Vector2(cardSizeScaled.x + cardMargin, 0f);
                cardCounter++;
            }
            nextCardPos = new Vector2(cardStartPos.x, nextCardPos.y - (cardSizeScaled.y + cardMargin));
        }
    }

    private Vector2 DetermineCardSize(Vector2 cardSize, Vector2 effectiveSpace)
    {
        float cardAspectRatio = cardSize.x / cardSize.y;
        print(cardAspectRatio);
        amountCards = cards.Length;
        amountCardsX = Mathf.FloorToInt(effectiveSpace.x / (cardSize.x + cardMargin));
        amountCardsY = Mathf.CeilToInt(amountCards / amountCardsX);
        int stop = 0;

        while ((effectiveSpace.y - TotalCardHeight() > cardSizeScaled.y + cardMargin || 
               effectiveSpace.y - TotalCardHeight() < 0) && stop < 500)
        {
            if (TotalCardHeight() > effectiveSpace.y)
            {
                ///cards too large
                amountCardsX++;
                print("too large");
            }
            else if (effectiveSpace.y - TotalCardHeight() > cardSizeScaled.y + cardMargin)
            {
                ///cards too small
                amountCardsX--;
                print("too small");
            }
            amountCardsY = Mathf.CeilToInt(amountCards / amountCardsX);
            float size = effectiveSpace.x / amountCardsX - cardMargin;
            cardSizeScaled = new Vector2(size, size / cardAspectRatio);
            stop++;

            if (stop == 500)
                Debug.LogWarning("Hit the resize limit");
        }

        ///correct size, remove margins for actual card only size
        //cardSizeScaled = new Vector2(cardSizeScaled.x - cardMargin, cardSizeScaled.y - cardMargin);
        return cardSizeScaled;
    }

    private float TotalCardHeight()
    {
        return amountCardsY * (cardSizeScaled.y + cardMargin);
    }
}