using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static ScoreManager instance;

    public TextMeshProUGUI tank1ScoreText;
    public TextMeshProUGUI tank2ScoreText;

    int tank1Score = 0;
    int tank2Score = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int playerNum)
    {
        if (playerNum == 1)
        {
            tank1Score++;
            tank1ScoreText.text = "Tank1 : " + tank1Score;
        }
        else if(playerNum == 2)
        {
            tank2Score++;
            tank2ScoreText.text = "Tank2 : " + tank2Score;
        }
    }    
}
