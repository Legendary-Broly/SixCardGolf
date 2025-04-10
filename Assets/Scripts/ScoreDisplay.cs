using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private MonoBehaviour gridRef; // Must implement ICardGrid
    [SerializeField] private TextMeshProUGUI scoreText;

    private ICardGrid grid;

    private void Awake()
    {
        grid = gridRef as ICardGrid;
    }

    private void Update()
    {
        if (grid == null || scoreText == null) return;

        var models = grid.GetCardModels();
        int score = ScoreCalculator.GetGridScore(models);
        scoreText.text = $"Score: {score}";
    }
}
