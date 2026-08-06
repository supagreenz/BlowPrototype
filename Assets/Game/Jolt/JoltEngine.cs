using System;
using System.Collections.Generic;
using Game.Jolt;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

public class JoltEngine : MonoBehaviour
{
    // The world's real capacity lives with its owner; mirroring the number
    // here would let the overlap buffer silently undersize the world.
    public static readonly int MaximumWorldBodies = JoltWorldSystem.MaximumWorldBodies;
    
    [AutoStaticsCleanup] private static JoltEngine _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void CreateInstance()
    {
        if (_instance) return;
        GameObject jeo = new GameObject("JoltEngine");
        DontDestroyOnLoad(jeo);
        _instance = jeo.AddComponent<JoltEngine>();
    }

    /// <summary>
    /// The shared world, owned by JoltWorldSystem. Null until the ECS world
    /// has been created, which is why registrations are queued rather than
    /// applied where they arrive.
    /// </summary>
    private JoltWorld ActiveWorld => JoltWorldSystem.Active;

    private Dictionary<JoltBodyHandle, JoltBody> _activeBodies = new();

    // Bodies raised before the world existed, or since the last drain.
    private List<DebrisSpawnedEvent> _pendingBodies = new();

    private PushField _activePushField;
    private JoltBodyHandle[] _pushFieldBuffer = new JoltBodyHandle[MaximumWorldBodies];

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        EventBus<DebrisSpawnedEvent>.Subscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Subscribe(OnDebrisDestroyed);
        EventBus<PushFieldSpawnedEvent>.Subscribe(OnPushFieldSpawned);
        EventBus<PushFieldDestroyedEvent>.Subscribe(OnPushFieldDestroyed);
    }

    private void OnDestroy()
    {
        EventBus<DebrisSpawnedEvent>.Unsubscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Unsubscribe(OnDebrisDestroyed);
        EventBus<PushFieldSpawnedEvent>.Unsubscribe(OnPushFieldSpawned);
        EventBus<PushFieldDestroyedEvent>.Unsubscribe(OnPushFieldDestroyed);
        
        if (_instance == this) _instance = null;
    }


    private void FixedUpdate()
    {
        var world = ActiveWorld;
        if (world == null || !world.IsValid) return;

        // Unconditional: the arena walls have to reach the world whether or
        // not a push field exists, and they only ever register once.
        DrainPendingBodies(world);

        // JoltStepSystem steps the world. Stepping here as well would advance
        // the simulation twice per tick.
        var states = world.ReadStates();

        ApplyPushField(world);

        foreach (var t in states)
        {
            if (_activeBodies.TryGetValue(t.Handle, out var body))
            {
                body.StateUpdate(t);
            }
        }
    }

    private void ApplyPushField(JoltWorld world)
    {
        if (!_activePushField) return;

        _activePushField.GetColliderBox(out var center, out Vector3 extents, out Quaternion rot);
        int cols = world.OverlapBox(center, extents, _pushFieldBuffer, rot);

        for (int i = 0; i < cols; i++)
        {
            var h = _pushFieldBuffer[i];
            if (world.TryGetState(h, out JoltBodyState s))
            {
                world.AddForce(h, _activePushField.CalculatePushFrom(s.Position));
            }
        }
    }

    private void DrainPendingBodies(JoltWorld world)
    {
        if (_pendingBodies.Count == 0) return;

        for (int i = 0; i < _pendingBodies.Count; i++)
        {
            var e = _pendingBodies[i];
            if (!e.bodyRef) continue;

            var bodyHandle = world.AddBody(e.joltBodyDesc);
            if (bodyHandle.IsValid) _activeBodies[bodyHandle] = e.bodyRef;
        }

        _pendingBodies.Clear();
    }

    private void OnDebrisSpawned(DebrisSpawnedEvent e)
    {
        // Queued rather than added here: JoltBody.Awake can fire before the
        // ECS world that owns the simulation has been created.
        if (!e.bodyRef) return;

        _pendingBodies.Add(e);
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
