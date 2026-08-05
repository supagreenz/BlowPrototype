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

    private Dictionary<JoltBodyHandle, JoltBody> _activeBodies = new();

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitJoltWorld();
        
        EventBus<DebrisSpawnedEvent>.Subscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Subscribe(OnDebrisDestroyed);
    }

    private void OnDestroy()
    {
        DisposeJoltWorld();
        
        EventBus<DebrisSpawnedEvent>.Unsubscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Unsubscribe(OnDebrisDestroyed);
        
        if (_instance == this) _instance = null;
    }


    private void InitJoltWorld()
    {
        _activeWorld = new JoltWorld();
        
        // // Spawn a box
        // _boxHandle = _activeWorld.AddBody(JoltBodyDesc.Box(Vector3.one * 0.5f, Vector3.zero, Quaternion.identity,
        //     JoltMotion.Static));
        //
        // // Spawn a ball
        // _ballHandle = _activeWorld.AddBody(JoltBodyDesc.Sphere(0.3f, new Vector3(0, 2, 0), JoltMotion.Dynamic));
    }

    private void DisposeJoltWorld()
    {
        _activeWorld?.Dispose();
        _activeWorld = null;
    }

    private void FixedUpdate()
    {
        // Read ball and box
        
        if (_activeWorld == null) return;
        
        _activeWorld.Step(Time.fixedDeltaTime);

        var states = _activeWorld.ReadStates();

        foreach (var t in states)
        {
            var handle = t.Handle;

            if (_activeBodies.TryGetValue(handle, out var body))
            {
                body.StateUpdate(t);
            }
        }
    }

    private void OnDebrisSpawned(DebrisSpawnedEvent e)
    {
        if (_activeWorld == null || !e.bodyRef) return;
        
        var bodyHandle = _activeWorld.AddBody(e.joltBodyDesc);
        _activeBodies[bodyHandle] = e.bodyRef;
    }

    private void OnDebrisDestroyed(DebrisDestroyedEvent e)
    {
        
    }
}
