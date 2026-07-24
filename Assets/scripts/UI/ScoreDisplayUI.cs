using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayUI : MonoBehaviour
{
    [SerializeField] private Button NextLevelButton;
    [SerializeField] private TMPro.TMP_Text FinalTimeText;
    [SerializeField] private TMPro.TMP_Text TopSpeedText;


    private void Start()
    {
        StringBuilder sbTime = new StringBuilder();
        sbTime.Append("Final Time: ");
        sbTime.Append(GameManager.Instance.getTimer());
        sbTime.Append(" Seconds");

        FinalTimeText.text = sbTime.ToString();

        StringBuilder sbSpeed = new StringBuilder();
        sbSpeed.Append("Top Speed: ");
        sbSpeed.Append(GameManager.Instance.getTopSpeed());
        sbSpeed.Append(" MPH");

        TopSpeedText.text = sbSpeed.ToString();

        if(GameManager.Instance.GetCurrentLevel() >= GameManager.Instance.GetMaxLevel()-1)
        {
            NextLevelButton.gameObject.SetActive(false);
        }
    }



    public void OnNextLevelClick()
    {
        GameManager.Instance.gameState = GameState.LOADLEVEL;
    }

    public void OnExitToMenuClick()
    {
        GameManager.Instance.gameState = GameState.LOADMAINMENU;
    }
}
