using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputBus
{
    [AutoStaticsCleanup]
    private static InputActionAsset _asset;
    
    [AutoStaticsCleanup]
    private static InputAction _moveAction;
    [AutoStaticsCleanup]
    private static InputAction _sprintAction;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        _asset = Resources.Load<InputActionAsset>("InputSystem_Actions");
        if (!_asset)
        {
            Debug.LogError("Could not find 'GameInputs.inputactions' in a Resources folder!");
            return;
        }

        _asset.Enable();
        
        // Find Actions
        _moveAction = _asset.FindAction("Move");
        _sprintAction = _asset.FindAction("Sprint");
        if (_moveAction == null || _sprintAction == null)
        {
            Debug.LogError("Input Actions not set up correctly!");
        }
    }
    
    public static Vector2 GetMoveDir() => _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public static bool IsSprinting() => _sprintAction?.IsPressed() ?? false;
}
