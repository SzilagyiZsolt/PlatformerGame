using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Ezt hívjuk meg a PLAY gombról
    public void PlayGame()
    {
        // Betölti a következõ Scene-t a sorban (ami az 1. pálya lesz)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Ezt hívjuk meg a QUIT gombról
    public void QuitGame()
    {
        Debug.Log("KILÉPÉS A JÁTÉKBÓL!");
        Application.Quit();
    }
}