using UnityEngine;

public class HUD : MonoBehaviour
{
    public void OnContinueClicked()
    {
        StartCoroutine(GameManager.Instance.UnpauseGame());

        //GameManager.Instance.UnpauseGame();
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1.0f;

        GameManager.Instance.gameState = GameState.RESTARTLEVEL;
    }

    public void OnQuitToMainClicked()
    {
        Time.timeScale = 1.0f;

        GameManager.Instance.gameState = GameState.LOADMAINMENU;
    }
}
