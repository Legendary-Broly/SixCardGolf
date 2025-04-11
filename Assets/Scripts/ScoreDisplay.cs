using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private MonoBehaviour gridRef; // must implement ICardGrid
    [SerializeField] private TextMeshProUGUI scoreText;

    private ICardGrid grid;

    private void Awake()
    {
        grid = gridRef as ICardGrid;
    }

    private void Update()
    {
        if (grid == null) return;

        var flippedValues = grid.GetFlippedCardValues();
        int score = ScoreCalculator.CalculateScore(flippedValues);
        scoreText.text = score.ToString();
    }
}
