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

    /// <summary>
    /// The shared world, owned by JoltWorldSystem. Null until the ECS world
    /// has been created, which is why registrations are queued rather than
    /// applied where they arrive.
    /// </summary>
    private JoltWorld ActiveWorld => JoltWorldSystem.Active;

    private Dictionary<JoltBodyHandle, JoltBody> _activeBodies = new();

    // Bodies raised before the world existed, or since the last drain.
    private List<DebrisSpawnedEvent> _pendingBodies = new();

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        EventBus<DebrisSpawnedEvent>.Subscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Subscribe(OnDebrisDestroyed);
    }

    private void OnDestroy()
    {
        EventBus<DebrisSpawnedEvent>.Unsubscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Unsubscribe(OnDebrisDestroyed);

        if (_instance == this) _instance = null;
    }


    private void FixedUpdate()
    {
        var world = ActiveWorld;
        if (world == null || !world.IsValid) return;

        // The arena walls have to reach the world whether or not anything else
        // is going on, and they only ever register once.
        DrainPendingBodies(world);

        UpdateOwnedBodies();
    }

    /// <summary>
    /// Reads the shared snapshot for the handful of bodies still driven by a
    /// MonoBehaviour. Iterates those bodies and indexes the snapshot, rather
    /// than walking the snapshot looking for owners: the world holds thousands
    /// of entity bodies that have nothing to do with this.
    /// </summary>
    private void UpdateOwnedBodies()
    {
        if (_activeBodies.Count == 0) return;

        var worldSystem = JoltWorldSystem.Instance;
        if (worldSystem == null) return;

        foreach (var owned in _activeBodies)
        {
            if (!owned.Value) continue;

            if (worldSystem.TryGetState(owned.Key, out JoltBodyState state))
            {
                owned.Value.StateUpdate(state);
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
}
