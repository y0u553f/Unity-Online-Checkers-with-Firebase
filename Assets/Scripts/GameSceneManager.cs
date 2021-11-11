using UnityEngine;
using UnityEngine.SceneManagement;
public static class GameSceneManager 
{
    public static void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
