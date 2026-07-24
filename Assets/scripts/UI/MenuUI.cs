using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public void OnPlayClicked()
    {
        GameManager.Instance.gameState = GameState.LOADLEVEL;
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
