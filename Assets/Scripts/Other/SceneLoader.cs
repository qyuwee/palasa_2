using UnityEngine;
using UnityEngine.SceneManagement; // Обязательный модуль для управления сценами

public class SceneLoader : MonoBehaviour
{
    // Загрузка сцены по её точному имени в Build Settings
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Загрузка сцены по её номеру-индексу (0, 1, 2...)
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Перезапуск текущей активной сцены
    public void RestartCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    // Выход из приложения (работает в скомпилированном .exe)
    public void QuitGame()
    {
        Application.Quit();
    }
}