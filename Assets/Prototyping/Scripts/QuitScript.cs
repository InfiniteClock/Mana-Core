using UnityEngine;

public class QuitScript : MonoBehaviour
{
    public void CloseGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }

}
