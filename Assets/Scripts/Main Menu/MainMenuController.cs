
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName = "GameScene";

    private bool isStarting = false;

    void Update()
    {
        if (isStarting)
            return;

        bool pressedStart = false;

        // Enter or Space
        if (Keyboard.current != null)
        {
            pressedStart =
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        // Left mouse click
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            pressedStart = true;
        }

        if (pressedStart)
        {
            StartGame();
        }
    }

    void StartGame()
    {
        isStarting = true;
        SceneManager.LoadScene(gameSceneName);
    }
}
