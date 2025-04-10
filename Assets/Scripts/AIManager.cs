using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    [SerializeField] private List<CardBehavior> aiCards;
    [SerializeField] private CardManager cardManager;

    private bool isMyTurn = false;
    private Dictionary<string, int> cardPoints = new Dictionary<string, int>
    {
        { "A", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
        { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 }, { "10", 10 },
        { "J", 11 }, { "Q", 12 }, { "K", 0 }, { "JOKER", -2 }
    };

    public void StartAITurn()
    {
        if (isMyTurn) return;
        isMyTurn = true;
        StartCoroutine(AITurnRoutine());
    }

    private IEnumerator AITurnRoutine()
    {
        yield return new WaitForSeconds(1f);

        string discardCard = cardManager.PeekTopDiscard();
        int faceUp = CountFaceUpCards();
        int visibleScore = CalculateGridScore();

        // Priority: Third match completion from discard
        if (CountMatchedColumns() == 2)
        {
            int forcedMatch = FindThirdMatchOpportunity(discardCard);
            if (forcedMatch != -1)
            {
                if (!aiCards[forcedMatch].isFaceUp)
                    aiCards[forcedMatch].FlipCard();

                string replaced = aiCards[forcedMatch].cardValue;
                aiCards[forcedMatch].SetCardValue(cardManager.TakeDiscardCard());
                cardManager.PlaceInDiscardPile(replaced);
                yield return new WaitForSeconds(1f);
                CheckFinalCardFlip();
                EndTurn();
                yield break;
            }
        }

        // Final face-down discard swap if keeps score under 4
        if (faceUp == 5 && visibleScore < 4)
        {
            int discardScore = GetCardScore(discardCard);
            if (visibleScore + discardScore < 4)
            {
                CardBehavior hidden = GetFirstHiddenCard();
                if (hidden != null)
                {
                    hidden.FlipCard();
                    string replaced = hidden.cardValue;
                    hidden.SetCardValue(cardManager.TakeDiscardCard());
                    cardManager.PlaceInDiscardPile(replaced);
                    yield return new WaitForSeconds(1f);
                    CheckFinalCardFlip();
                    EndTurn();
                    yield break;
                }
            }
        }

        // Match from discard
        int matchIndex = FindBestMatchingColumn(discardCard);
        if (matchIndex != -1)
        {
            if (!aiCards[matchIndex].isFaceUp)
                aiCards[matchIndex].FlipCard();

            string replaced = aiCards[matchIndex].cardValue;
            aiCards[matchIndex].SetCardValue(cardManager.TakeDiscardCard());
            cardManager.PlaceInDiscardPile(replaced);
            yield return new WaitForSeconds(1f);
            CheckFinalCardFlip();
            EndTurn();
            yield break;
        }

        int worstIndex = GetWorstKnownCardIndex();

        // Always take Joker
        if (discardCard == "JOKER" && worstIndex != -1)
        {
            string replaced = aiCards[worstIndex].cardValue;
            aiCards[worstIndex].SetCardValue(cardManager.TakeDiscardCard());
            cardManager.PlaceInDiscardPile(replaced);
            yield return new WaitForSeconds(1f);
            CheckFinalCardFlip();
            EndTurn();
            yield break;
        }

        // Discard card is 2+ points better than worst visible card
        if (worstIndex != -1)
        {
            int current = GetCardScore(aiCards[worstIndex].cardValue);
            int incoming = GetCardScore(discardCard);

            if (incoming + 2 <= current)
            {
                string replaced = aiCards[worstIndex].cardValue;
                aiCards[worstIndex].SetCardValue(cardManager.TakeDiscardCard());
                cardManager.PlaceInDiscardPile(replaced);
                yield return new WaitForSeconds(1f);
                CheckFinalCardFlip();
                EndTurn();
                yield break;
            }
        }

        // Flip a hidden card if discard is bad and less than 5 face-up cards
        if (faceUp < 5 && !IsCardValuable(discardCard))
        {
            CardBehavior hidden = GetFirstHiddenCard();
            if (hidden != null)
            {
                hidden.FlipCard();
                yield return new WaitForSeconds(1f);
                CheckFinalCardFlip();
                EndTurn();
                yield break;
            }
        }

        // Final card flip allowed only in specific cases
        else if (faceUp == 5)
        {
            // Flip final card to attempt third match
            if (visibleScore < 4 && CountMatchedColumns() == 2)
            {
                CardBehavior hidden = GetFirstHiddenCard();
                if (hidden != null)
                {
                    hidden.FlipCard();
                    yield return new WaitForSeconds(1f);
                    CheckFinalCardFlip();
                    EndTurn();
                    yield break;
                }
            }
        }

        // DRAW a card
        cardManager.DrawCardFromDeck();
        yield return new WaitForSeconds(1f);
        string drawn = cardManager.GetDrawnCard();
        if (string.IsNullOrEmpty(drawn))
        {
            CheckFinalCardFlip();
            EndTurn();
            yield break;
        }

        // Priority: Third match from drawn card
        if (CountMatchedColumns() == 2)
        {
            int drawMatch = FindThirdMatchOpportunity(drawn);
            if (drawMatch != -1)
            {
                if (!aiCards[drawMatch].isFaceUp)
                    aiCards[drawMatch].FlipCard();

                string replaced = aiCards[drawMatch].cardValue;
                aiCards[drawMatch].SetCardValue(drawn);
                cardManager.PlaceInDiscardPile(replaced);
                cardManager.ClearDrawnCard();
                yield return new WaitForSeconds(1f);
                CheckFinalCardFlip();
                EndTurn();
                yield break;
            }
        }

        // Match from drawn card
        matchIndex = FindBestMatchingColumn(drawn);
        if (matchIndex != -1)
        {
            if (!aiCards[matchIndex].isFaceUp)
                aiCards[matchIndex].FlipCard();

            string replaced = aiCards[matchIndex].cardValue;
            aiCards[matchIndex].SetCardValue(drawn);
            cardManager.PlaceInDiscardPile(replaced);
            cardManager.ClearDrawnCard();
            yield return new WaitForSeconds(1f);
            CheckFinalCardFlip();
            EndTurn();
            yield break;
        }

        // Always take Joker
        if (drawn == "JOKER" && worstIndex != -1)
        {
            string replaced = aiCards[worstIndex].cardValue;
            aiCards[worstIndex].SetCardValue(drawn);
            cardManager.PlaceInDiscardPile(replaced);
            cardManager.ClearDrawnCard();
            yield return new WaitForSeconds(1f);
            CheckFinalCardFlip();
            EndTurn();
            yield break;
        }

        // Drawn card is better than worst known card
        if (worstIndex != -1 && GetCardScore(drawn) < GetCardScore(aiCards[worstIndex].cardValue))
        {
            string replaced = aiCards[worstIndex].cardValue;
            aiCards[worstIndex].SetCardValue(drawn);
            cardManager.PlaceInDiscardPile(replaced);
            cardManager.ClearDrawnCard();
            yield return new WaitForSeconds(1f);
            CheckFinalCardFlip();
            EndTurn();
            yield break;
        }

        // Final face-down swap if drawn card keeps score under 4
        if (faceUp == 5 && visibleScore < 4)
        {
            int total = visibleScore + GetCardScore(drawn);
            if (total < 4)
            {
                CardBehavior hidden = GetFirstHiddenCard();
                if (hidden != null)
                {
                    hidden.FlipCard();
                    string replaced = hidden.cardValue;
                    hidden.SetCardValue(drawn);
                    cardManager.PlaceInDiscardPile(replaced);
                    cardManager.ClearDrawnCard();
                    yield return new WaitForSeconds(1f);
                    CheckFinalCardFlip();
                    EndTurn();
                    yield break;
                }
            }
        }

        // Otherwise, discard drawn card
        cardManager.PlaceInDiscardPile(drawn);
        cardManager.ClearDrawnCard();
        yield return new WaitForSeconds(1f);
        CheckFinalCardFlip();
        EndTurn();
    }

    private int GetCardScore(string value)
    {
        return cardPoints.ContainsKey(value) ? cardPoints[value] : 0;
    }

    private int CalculateGridScore()
    {
        int score = 0;
        foreach (CardBehavior card in aiCards)
        {
            if (card.isFaceUp)
                score += GetCardScore(card.cardValue);
        }
        return score;
    }

    private int CountFaceUpCards()
    {
        int count = 0;
        foreach (CardBehavior card in aiCards)
        {
            if (card.isFaceUp) count++;
        }
        return count;
    }

    private int GetWorstKnownCardIndex()
    {
        int worstScore = -1;
        int index = -1;

        for (int i = 0; i < aiCards.Count; i++)
        {
            if (!aiCards[i].isFaceUp) continue;
            if (IsPartOfMatchedColumn(i)) continue;

            int score = GetCardScore(aiCards[i].cardValue);
            if (score > worstScore)
            {
                worstScore = score;
                index = i;
            }
        }

        return index;
    }

    private int FindBestMatchingColumn(string value)
    {
        int bestIndex = -1;
        int highestScore = -1;

        for (int col = 0; col < 3; col++)
        {
            int top = col;
            int bottom = col + 3;

            if (aiCards[top].isFaceUp && aiCards[top].cardValue == value && !IsPartOfMatchedColumn(bottom))
            {
                int score = aiCards[bottom].isFaceUp ? GetCardScore(aiCards[bottom].cardValue) : 0;
                if (score > highestScore)
                {
                    bestIndex = bottom;
                    highestScore = score;
                }
            }

            if (aiCards[bottom].isFaceUp && aiCards[bottom].cardValue == value && !IsPartOfMatchedColumn(top))
            {
                int score = aiCards[top].isFaceUp ? GetCardScore(aiCards[top].cardValue) : 0;
                if (score > highestScore)
                {
                    bestIndex = top;
                    highestScore = score;
                }
            }
        }

        return bestIndex;
    }

    private int FindThirdMatchOpportunity(string value)
    {
        if (string.IsNullOrEmpty(value)) return -1;

        for (int col = 0; col < 3; col++)
        {
            if (IsColumnMatched(col)) continue;

            int top = col;
            int bottom = col + 3;

            if (aiCards[top].isFaceUp && aiCards[bottom].isFaceUp)
            {
                if (aiCards[top].cardValue == "JOKER" || aiCards[bottom].cardValue == "JOKER")
                    return aiCards[top].cardValue == "JOKER" ? bottom : top;

                if (aiCards[top].cardValue == value)
                    return bottom;

                if (aiCards[bottom].cardValue == value)
                    return top;
            }
            else if (aiCards[top].isFaceUp && aiCards[top].cardValue == value)
            {
                return bottom;
            }
            else if (aiCards[bottom].isFaceUp && aiCards[bottom].cardValue == value)
            {
                return top;
            }
        }

        return -1;
    }

    private bool IsColumnMatched(int col)
    {
        int top = col;
        int bottom = col + 3;

        return aiCards[top].isFaceUp && aiCards[bottom].isFaceUp &&
               aiCards[top].cardValue == aiCards[bottom].cardValue;
    }

    private bool IsPartOfMatchedColumn(int index)
    {
        int col = index % 3;
        return IsColumnMatched(col);
    }

    private int CountMatchedColumns()
    {
        int count = 0;
        for (int col = 0; col < 3; col++)
        {
            if (IsColumnMatched(col))
                count++;
        }
        return count;
    }

    private CardBehavior GetFirstHiddenCard()
    {
        foreach (CardBehavior card in aiCards)
        {
            if (!card.isFaceUp)
                return card;
        }
        return null;
    }

    private bool IsCardValuable(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        int score = GetCardScore(value);
        return score <= 4 || value == "JOKER" || value == "K";
    }

    private void CheckFinalCardFlip()
    {
        foreach (CardBehavior card in aiCards)
        {
            if (!card.isFaceUp)
                return;
        }

        TurnManager.Instance.TriggerFinalTurn();
    }

    public void RevealAllCards()
    {
        foreach (CardBehavior card in aiCards)
        {
            if (!card.isFaceUp)
                card.FlipCard();
        }
    }

    private void EndTurn()
    {
        isMyTurn = false;

        if (TurnManager.Instance.GameStarted && AreAllPlayerCardsFaceUp() && !AreAllAICardsFaceUp())
        {
            RevealAllCards();
        }

        TurnManager.Instance.EndAITurn();
    }

    private bool AreAllPlayerCardsFaceUp()
    {
        foreach (CardBehavior card in cardManager.GetPlayerCards())
        {
            if (!card.isFaceUp)
                return false;
        }
        return true;
    }

    private bool AreAllAICardsFaceUp()
    {
        foreach (CardBehavior card in aiCards)
        {
            if (!card.isFaceUp)
                return false;
        }
        return true;
    }
}
