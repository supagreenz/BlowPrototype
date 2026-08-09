using System;
using System.Collections.Generic;
using Game.Jolt;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

public class JoltEngine : MonoBehaviour
{
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
    
    // MODULES
    private JoltPhysicalBodiesModule _physicalBodiesModule;
    private JoltBlowFieldsModule _blowFieldsModule;

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitJoltWorld();
        InitModules();
    }

    private void OnDestroy()
    {
        DisposeJoltWorld();
        DisposeModules();
        
        if (_instance == this) _instance = null;
    }


    private void InitJoltWorld()
    {
        _activeWorld = new JoltWorld(JoltConstants.MaxWorldBodies * 2);
    }

    private void InitModules()
    {
        _physicalBodiesModule = new JoltPhysicalBodiesModule();
        _physicalBodiesModule.Init(_activeWorld);

        _blowFieldsModule = new JoltBlowFieldsModule();
        _blowFieldsModule.Init(_activeWorld);
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
        
        _blowFieldsModule?.Dispose();
        _blowFieldsModule = null;
    }

    private void FixedUpdate()
    {
        // Read ball and box
        
        if (_activeWorld == null) return;
        
        _activeWorld.Step(Time.fixedDeltaTime);
        
        _blowFieldsModule.UpdateStep();
        _physicalBodiesModule.UpdateStep();
    }
}
