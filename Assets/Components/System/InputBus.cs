using UnityEngine;
using UnityEngine.InputSystem;

public static class InputBus
{
    private static readonly InputActionAsset _asset;
    
    private static readonly InputAction MoveAction;
    private static readonly InputAction SprintAction;

    static InputBus()
    {
        _asset = Resources.Load<InputActionAsset>("InputSystem_Actions");
        if (!_asset)
        {
            Debug.LogError("Could not find 'GameInputs.inputactions' in a Resources folder!");
            return;
        }

        _asset.Enable();
        
        // Find Actions
        MoveAction = _asset.FindAction("Move");
        SprintAction = _asset.FindAction("Sprint");
        if (MoveAction == null || SprintAction == null)
        {
            Debug.LogError("Input Actions not set up correctly!");
        }
        
    }
    
    public static Vector2 GetMoveDir() => MoveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public static bool IsSprinting() => SprintAction?.IsPressed() ?? false;
}
