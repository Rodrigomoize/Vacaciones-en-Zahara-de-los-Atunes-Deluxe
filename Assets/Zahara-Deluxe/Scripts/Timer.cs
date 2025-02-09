using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timeLeft = 150f;
    public TextMeshProUGUI countdownText;
    ScoreL1 score;
    public bool isPaused = false;
    void Start()
    {
      score = FindObjectOfType<ScoreL1>();
      isPaused = false;
    }

    void Update()
    {
      if(isPaused) return; 
      else
      {    
        if  (timeLeft > 0)
        {
          timeLeft -= Time.deltaTime;
          UpdateTimerDisplay();
        }
        else
        {
          timeLeft = 0;
          TimerEnded();
        }
      }
 
    }

    void UpdateTimerDisplay()
    {
      int minutes = Mathf.FloorToInt(timeLeft / 60f);
      int seconds = Mathf.FloorToInt(timeLeft % 60f);
      countdownText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    void TimerEnded()
    {
      if(isPaused) return;
      isPaused = true;
      countdownText.text = "00:00";
      score.EndGame();
      Debug.Log("Time has run out!");
    }
}
