using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class LocalMultiplayerSetup : MonoBehaviour
{
    [SerializeField] private PlayerInput player1;
    [SerializeField] private PlayerInput player2;

    private void Start()
    {
        int gamepadCount = Gamepad.all.Count;

        if (gamepadCount >= 2)
        {
            SetupTwoGamepads();
        }
        else if (gamepadCount == 1)
        {
            SetupOneGamepadAndKeyboard();
        }
        else
        {
            SetupSplitKeyboard();
        }
    }

    private void SetupTwoGamepads()
    {
        player1.actions.bindingMask = null;
        player2.actions.bindingMask = null;

        player1.SwitchCurrentControlScheme(
            "Gamepad",
            Gamepad.all[0]
        );

        player2.SwitchCurrentControlScheme(
            "Gamepad",
            Gamepad.all[1]
        );
    }

    private void SetupOneGamepadAndKeyboard()
    {
        player1.actions.bindingMask = null;
        player2.actions.bindingMask = null;

        player1.SwitchCurrentControlScheme(
            "Gamepad",
            Gamepad.all[0]
        );

        player2.actions.bindingMask =
            InputBinding.MaskByGroup("KeyboardP1");
    }

    private void SetupSplitKeyboard()
    {
        if (Keyboard.current == null)
        {
            Debug.LogWarning("No keyboard detected.");
            return;
        }

        player1.actions.bindingMask =
            InputBinding.MaskByGroup("KeyboardP1");

        player2.actions.bindingMask =
            InputBinding.MaskByGroup("KeyboardP2");
    }
}