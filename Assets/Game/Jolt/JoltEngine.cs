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

    private PushField _activePushField;
    private JoltBodyHandle[] _pushFieldBuffer = new JoltBodyHandle[1024];

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
        EventBus<PushFieldSpawnedEvent>.Subscribe(OnPushFieldSpawned);
        EventBus<PushFieldDestroyedEvent>.Subscribe(OnPushFieldDestroyed);
    }

    private void OnDestroy()
    {
        DisposeJoltWorld();
        
        EventBus<DebrisSpawnedEvent>.Unsubscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Unsubscribe(OnDebrisDestroyed);
        EventBus<PushFieldSpawnedEvent>.Unsubscribe(OnPushFieldSpawned);
        EventBus<PushFieldDestroyedEvent>.Unsubscribe(OnPushFieldDestroyed);
        
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
        
        if (_activeWorld == null || !_activePushField) return;
        
        _activeWorld.Step(Time.fixedDeltaTime);

        var states = _activeWorld.ReadStates();
        
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

    private void OnPushFieldSpawned(PushFieldSpawnedEvent e)
    {
        if (!e.pushField) return;
        
        _activePushField = e.pushField;
    }

    private void OnPushFieldDestroyed(PushFieldDestroyedEvent e)
    {
        
    }
}
