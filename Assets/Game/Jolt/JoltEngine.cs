using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.Jolt;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

    // USAGE
    private JoltWorld _activeWorld;
    
    // MODULES
    private JoltPhysicalBodiesModule _physicalBodiesModule;
    private JoltBlowersModule _blowersModule;

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

        _blowersModule = new JoltBlowersModule();
        _blowersModule.Init(_activeWorld);
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
        
        _blowersModule?.Dispose();
        _blowersModule = null;
    }

    private void FixedUpdate()
    {
        // Read ball and box
        
        if (_activeWorld == null) return;
        
        Stopwatch sw = Stopwatch.StartNew();
        TimeSpan step, blower, body;
        
        _activeWorld.Step(Time.fixedDeltaTime);
        step = sw.Elapsed;
        sw.Restart();
        
        var states = _activeWorld.ReadStates();
        
        _blowersModule.UpdateStep(states);
        blower = sw.Elapsed;
        sw.Restart();
        _physicalBodiesModule.UpdateStep(states);
        body = sw.Elapsed;
        sw.Restart();
        
        Debug.Log($"Took Step: {step.TotalMilliseconds}ms, Blower: {blower.TotalMilliseconds}ms, Bodies: {body.TotalMilliseconds}ms");
        sw.Stop();
    }
}
