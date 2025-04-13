using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Update()
    {
        if (scoreManager == null) return;

        // Delegate score update to ScoreManager
        scoreText.text = "Score: " + scoreManager.GetCurrentScore();
    }
}
