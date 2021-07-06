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
    [Tooltip("Minimum margin on screen border")]
    public float screenBorderMargin = 1f;
    public float menuSpace = 2f;
    [Tooltip("How much larger cards get while hovering over them")]
    public Vector3 hoverScaling = new Vector3(0.1f, 0.1f, 0f);    ///added to card size when hovering
    [Tooltip("Minimum number of card columns")]
    public int minColumns = 4;
    public int minRows = 4;
    [Tooltip("How long the cards fade in")]
    public float fadeIn = .2f;
    public Sprite[] cardpictures;
    public Sprite[] bigCardpictures;
    public GameObject cardPrefab;
    public GameObject bigCard;

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
    private int attempts = 0;
    private GameHandler gameHandler;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new GameActionMap();
            // Tell the "GameInput" action map that we want to get told about
            // when actions get triggered.
            inputActions.GameInput.SetCallbacks(this);
        }
    }

    private void OnDisable()
    {
        EnableGameInput(false);
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        gameHandler = FindObjectOfType<GameHandler>();
        cameraHeight = mainCamera.pixelHeight;
        cameraWidth = mainCamera.pixelWidth;
    }

    private void Start()
    {
        cards = new GameObject[0];
        ResetCards();
    }

    internal void ResetCards()
    {
        EnableGameInput(false);
        if (cards.Length > 0)
        {
            foreach (var card in cards)
            {
                Destroy(card);
            }
            Array.Clear(cards, 0, cards.Length);
        }        
        InstantiateCards();
        Shuffle();
        StartCoroutine(LayoutCards());
        attempts = 0;
    }

    public void EnableGameInput(bool input)
    {
        if (input)
        {
            inputActions.GameInput.Enable();
        }
        else
        {
            inputActions.GameInput.Disable();
        }
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
                        attempts++;
                        gameHandler.CountAttempt(attempts);
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
        EnableGameInput(false);
        if (card1.pair == card2.pair)
        {
            ///correct pair
            print("correct");
            yield return new WaitForSeconds(1f);
            card1.gameObject.SetActive(false);
            card2.gameObject.SetActive(false);
            bigCard.GetComponent<SpriteRenderer>().sprite = bigCardpictures[card1.spriteNumber - 1];    ///sprite names count from 1, so -1 for correct index
            bigCard.SetActive(true);    //lerp fade in and out over .5s?
            yield return new WaitForSeconds(1.5f);
            bigCard.SetActive(false);
            cardPairsLeft--;
            if (cardPairsLeft <= 0)
            {
                //end of game
                //gameHandler.NewGame();
                gameHandler.WinGame();
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
        EnableGameInput(true);
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
                GameObject cardObject = Instantiate(cardPrefab, new Vector3(0f, 0f, -20f), Quaternion.identity);
                cardObject.layer = 6;
                Card card = cardObject.GetComponent<Card>();
                card.GetComponent<SpriteRenderer>().color = Color.clear;
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

    private IEnumerator LayoutCards()
    {
        Vector2 screenSizeWorld = mainCamera.ScreenToWorldPoint(new Vector2(cameraWidth, cameraHeight));
        Vector2 effectiveSpace = new Vector2(screenSizeWorld.x * 2 - screenBorderMargin * 2, screenSizeWorld.y * 2 - screenBorderMargin * 2 - menuSpace);

        cardSizeScaled = DetermineCardSize(effectiveSpace);
        for (int i = 0; i < cards.Length; i++)
        {
            ScaleCard(cards[i].GetComponent<Card>());
        }
        Vector2 cardSpriteSize = cards[0].GetComponent<SpriteRenderer>().bounds.size;

        int columns = Mathf.Clamp(Mathf.FloorToInt(effectiveSpace.x / (cardSpriteSize.x + cardMargin)), 1, cards.Length);
        int rows = Mathf.Clamp(Mathf.FloorToInt(effectiveSpace.y / (cardSpriteSize.y + cardMargin)), 1, cards.Length);
        if (cameraWidth / cameraHeight >= 1f)
        {
            rows = Mathf.Clamp(Mathf.CeilToInt((float)cards.Length / columns), 1, cards.Length);
            if (rows < minRows && minRows * (cardSpriteSize.y + cardMargin) < effectiveSpace.y)
            {
                rows = minRows;
                columns = cards.Length / rows;
            }
        }
        else
        {
            columns = Mathf.Clamp(Mathf.CeilToInt((float)cards.Length / rows), 1, cards.Length);
            if (columns < minColumns && minColumns * (cardSpriteSize.x + cardMargin) < effectiveSpace.x)
            {
                columns = minColumns;
                rows = cards.Length / columns;
            }
        }

        float totalCardWidth = columns * (cardSpriteSize.x + cardMargin);
        float leftRightMargin = effectiveSpace.x - totalCardWidth;
        float topBotMargin = effectiveSpace.y - (rows * (cardSpriteSize.y + cardMargin));
        Vector2 cardStartPos = mainCamera.ScreenToWorldPoint(new Vector3(0, cameraHeight));
        cardStartPos += new Vector2((cardSpriteSize.x + cardMargin + leftRightMargin) / 2 + screenBorderMargin,
                                    -((cardSpriteSize.y + cardMargin + topBotMargin) / 2 + screenBorderMargin + menuSpace));
        Vector2 nextCardPos = cardStartPos;

        float timeElapsed = 0f;
        int cardCounter = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (cardCounter >= cards.Length)
                    yield break;
                cards[cardCounter].transform.position = nextCardPos;
                while (timeElapsed<fadeIn)
                {
                    float t = (timeElapsed / fadeIn) * (timeElapsed / fadeIn);
                    cards[cardCounter].GetComponent<SpriteRenderer>().color = Color.Lerp(Color.clear, Color.white, t);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                cards[cardCounter].GetComponent<SpriteRenderer>().color = Color.white;  ///ensure the sprite never stays slightly transparent
                timeElapsed = 0f;
                nextCardPos += new Vector2(cardSpriteSize.x + cardMargin, 0f);
                cardCounter++;
            }
            nextCardPos = new Vector2(cardStartPos.x, nextCardPos.y - (cardSpriteSize.y + cardMargin));
        }
        EnableGameInput(true);
    }

    private Vector2 DetermineCardSize(Vector2 effectiveSpace)
    {
        float effectiveSpaceArea = effectiveSpace.x * effectiveSpace.y;
        float cardSlotArea = effectiveSpaceArea / cards.Length;
        float cardSlotWidth;
        if (cameraWidth / cameraHeight >= 1f)   ///switch for landscape vs portrait screen
        {
            cardSlotWidth = Mathf.Sqrt(cardSlotArea * effectiveSpace.x / effectiveSpace.y);
        }
        else
        {
            cardSlotWidth = Mathf.Sqrt(cardSlotArea * (effectiveSpace.y / effectiveSpace.x));
        }
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