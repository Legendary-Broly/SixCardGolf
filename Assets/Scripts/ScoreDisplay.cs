using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private AICardGrid gridRef; // Must implement ICardGrid
    [SerializeField] private TextMeshProUGUI scoreText;

    private ICardGrid grid;

    private void Awake()
    {
        grid = gridRef;
    }

    void Update()
    {
        if (gridRef == null) return;

        var flippedCards = gridRef.GetFlippedCardValues();
        int totalScore = 0;

        foreach (var value in flippedCards)
        {
            if (int.TryParse(value, out int cardValue))
            {
                totalScore += cardValue;
            }
        }

        scoreText.text = totalScore.ToString();
    }

}
