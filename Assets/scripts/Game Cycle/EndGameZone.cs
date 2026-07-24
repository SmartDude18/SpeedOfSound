using UnityEngine;

public class EndGameZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            GameManager.Instance.gameState = GameState.LOADLEVELCOMPLETESCREEN;
        }
    }
}
