using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{

    public TetrisManager tetrisManager;
    public TextMeshProUGUI scoreText;

    public GameObject EndGamePanel;

    public void UIUpdateScore()
    {
        scoreText.text = $"SCORE: {tetrisManager.score}";
    }

    public void UpdateGameOver()
    {
        EndGamePanel.SetActive(tetrisManager.gameOver);
    }

    public void PlayAgain()
    {
        tetrisManager.SetGameOver(false);
    }
}
