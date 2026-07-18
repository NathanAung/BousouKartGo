using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] string raceSceneName = "RaceScene";

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(raceSceneName);
            return;
        }

        if (Gamepad.current == null)
            return;

        foreach (InputControl control in Gamepad.current.allControls)
        {
            if (control is ButtonControl button &&
                button.wasPressedThisFrame)
            {
                SceneManager.LoadScene(raceSceneName);
                return;
            }
        }
    }

    public void LoadRaceScene()
    {
        SceneManager.LoadScene(raceSceneName);
    }
}