using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private List<CardBehavior> gridCards;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI aiScoreText;

    private Dictionary<string, int> cardPoints = new Dictionary<string, int>()
    {
        { "A", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
        { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 }, { "10", 10 },
        { "J", 11 }, { "Q", 12 }, { "K", 0 }, { "JOKER", -2 }
    };

    private bool isScoreUpdatePending = false; // Flag to prevent infinite updates

    private void Start()
    {
        if (playerScoreText == null)
        {
            Debug.LogError("[ScoreManager] PlayerScoreText is not assigned in the Inspector.");
        }

        if (aiScoreText == null)
        {
            Debug.LogError("[ScoreManager] AIScoreText is not assigned in the Inspector.");
        }

        if (gridCards == null || gridCards.Count == 0)
        {
            Debug.LogError("[ScoreManager] gridCards list is not assigned or is empty. Ensure it is populated in the Inspector.");
        }
        else
        {
            for (int i = 0; i < gridCards.Count; i++)
            {
                if (gridCards[i] == null)
                {
                    Debug.LogError($"[ScoreManager] gridCards[{i}] is null. Ensure all elements are assigned.");
                }
                else if (gridCards[i].GetComponent<CardBehavior>() == null)
                {
                    Debug.LogError($"[ScoreManager] gridCards[{i}] does not have a CardBehavior component attached.");
                }
            }
        }
    }

    private void OnCardFlipOrSwap()
    {
        Debug.Log("[ScoreManager] OnCardFlipOrSwap triggered.");

        // Ensure this method is only called when necessary
        if (!isScoreUpdatePending)
        {
            isScoreUpdatePending = true;
            UpdateScores();
            isScoreUpdatePending = false;
        }
        else
        {
            Debug.LogWarning("[ScoreManager] OnCardFlipOrSwap called while an update is already in progress.");
        }
    }

    public void UpdateScores()
    {
        Debug.Log("[ScoreManager] UpdateScores called.");

        if (isScoreUpdatePending)
        {
            Debug.LogWarning("[ScoreManager] UpdateScores skipped because another update is in progress.");
            return;
        }

        int playerScore = CalculateScore(); // Assuming this is for the player
        int aiScore = 0; // Placeholder for AI score calculation logic

        Debug.Log($"[ScoreManager] Player Score: {playerScore}, AI Score: {aiScore}");

        if (playerScoreText != null)
        {
            playerScoreText.text = "Player Score: " + playerScore;
        }

        if (aiScoreText != null)
        {
            aiScoreText.text = "AI Score: " + aiScore;
        }
    }

    private int CalculateScore()
    {
        Debug.Log("[ScoreManager] Starting score calculation.");

        if (gridCards == null || gridCards.Count < 6)
        {
            Debug.LogError("[ScoreManager] gridCards list is null or does not have enough elements. Skipping score calculation.");
            return 0;
        }

        int total = 0;

        for (int col = 0; col < 3; col++)
        {
            if (col >= gridCards.Count || col + 3 >= gridCards.Count)
            {
                Debug.LogError($"[ScoreManager] Index out of range for column {col}. Skipping this column.");
                continue;
            }

            CardBehavior top = gridCards[col];
            CardBehavior bottom = gridCards[col + 3];

            if (top == null || bottom == null)
            {
                Debug.LogError($"[ScoreManager] Null card detected in column {col}. Skipping this column.");
                continue;
            }

            Debug.Log($"[ScoreManager] Evaluating column {col}: TopCard={top.cardValue}, BottomCard={bottom.cardValue}");

            // Normalize values for comparison
            string topValueNormalized = top.cardValue?.Trim().ToUpper();
            string bottomValueNormalized = bottom.cardValue?.Trim().ToUpper();

            Debug.Log($"[ScoreManager] Normalized values for column {col}: TopCardNormalized={topValueNormalized}, BottomCardNormalized={bottomValueNormalized}");

            bool bothFaceUp = top.isFaceUp && bottom.isFaceUp;
            bool match = bothFaceUp && topValueNormalized == bottomValueNormalized;

            Debug.Log($"[ScoreManager] Match condition for column {col}: BothFaceUp={bothFaceUp}, Match={match}");

            if (match)
            {
                Debug.Log($"[ScoreManager] Matched column detected at index {col}: Top={top.cardValue}, Bottom={bottom.cardValue}");
                continue; // Skip adding points for matched columns
            }

            if (top.isFaceUp && cardPoints.ContainsKey(topValueNormalized))
            {
                int topValue = cardPoints[topValueNormalized];
                Debug.Log($"[ScoreManager] Adding top card value at column {col}: {top.cardValue} = {topValue}");
                total += topValue;
            }

            if (bottom.isFaceUp && cardPoints.ContainsKey(bottomValueNormalized))
            {
                int bottomValue = cardPoints[bottomValueNormalized];
                Debug.Log($"[ScoreManager] Adding bottom card value at column {col}: {bottom.cardValue} = {bottomValue}");
                total += bottomValue;
            }
        }

        Debug.Log($"[ScoreManager] Total score calculated: {total}");
        return total;
    }

    public int GetCurrentScore()
    {
        return CalculateScore();
    }
}
