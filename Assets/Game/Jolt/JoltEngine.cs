using System;
using System.Collections.Generic;
using Game.Jolt;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

public class JoltEngine : MonoBehaviour
{
    public static readonly int MaxWorldBodies = 4096;
    
    [AutoStaticsCleanup] private static JoltEngine _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void CreateInstance()
    {
        if (_instance) return;
        GameObject jeo = new GameObject("JoltEngine");
        DontDestroyOnLoad(jeo);
        _instance = jeo.AddComponent<JoltEngine>();
    }

    private JoltWorld _activeWorld;

    private PushField _activePushField;
    private JoltBodyHandle[] _pushFieldBuffer = new JoltBodyHandle[1024];
    
    // MODULES
    private JoltPhysicalBodiesModule _physicalBodiesModule;

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitJoltWorld();
        InitModules();
        
        EventBus<PushFieldSpawnedEvent>.Subscribe(OnPushFieldSpawned);
        EventBus<PushFieldDestroyedEvent>.Subscribe(OnPushFieldDestroyed);
    }

    private void OnDestroy()
    {
        DisposeJoltWorld();
        DisposeModules();
        
        EventBus<PushFieldSpawnedEvent>.Unsubscribe(OnPushFieldSpawned);
        EventBus<PushFieldDestroyedEvent>.Unsubscribe(OnPushFieldDestroyed);
        
        if (_instance == this) _instance = null;
    }


    private void InitJoltWorld()
    {
        _activeWorld = new JoltWorld(MaxWorldBodies * 2);
    }

    private void InitModules()
    {
        _physicalBodiesModule = new JoltPhysicalBodiesModule();
        _physicalBodiesModule.Init(_activeWorld);
    }

    private void DisposeJoltWorld()
    {
        _activeWorld?.Dispose();
        _activeWorld = null;
    }

    private void DisposeModules()
    {
        _physicalBodiesModule?.Dispose();
        _physicalBodiesModule = null;
    }

    private void FixedUpdate()
    {
        // Read ball and box
        
        if (_activeWorld == null || !_activePushField) return;
        
        _activeWorld.Step(Time.fixedDeltaTime);

        _activePushField.GetColliderBox(out var center, out Vector3 extents, out Quaternion rot);
        int cols = _activeWorld.OverlapBox(center, extents, _pushFieldBuffer, rot);
        
        for (int i = 0; i < cols; i++)
        {
            var h = _pushFieldBuffer[i];
            if (_activeWorld.TryGetState(h, out JoltBodyState s))
            {
                _activeWorld.AddForce(h, _activePushField.CalculatePushFrom(s.Position));  
            } 
        }

        _physicalBodiesModule.UpdateStep();
    }

    private void OnPushFieldSpawned(PushFieldSpawnedEvent e)
    {
        if (!e.pushField) return;
        
        _activePushField = e.pushField;
    }

    private void OnPushFieldDestroyed(PushFieldDestroyedEvent e)
    {
        
    }
}
