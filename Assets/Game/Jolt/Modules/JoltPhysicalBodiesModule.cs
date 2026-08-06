using System;
using Game.Jolt;
using Unity.Collections;
using UnityEngine;

public class JoltPhysicalBodiesModule : IDisposable
{
    private JoltBody[] _activeBodies = new JoltBody[JoltEngine.MaxWorldBodies];
    
    private JoltWorld _activeWorld;

    public void Init(JoltWorld world)
    {
        _activeWorld = world;
        EventBus<DebrisSpawnedEvent>.Subscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Subscribe(OnDebrisDestroyed);
    }
    
    public void Dispose()
    {
        EventBus<DebrisSpawnedEvent>.Unsubscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Unsubscribe(OnDebrisDestroyed);
    }

    public void UpdateStep()
    {
        var states = _activeWorld.ReadStates();
        
        foreach (var t in states)
        {
            var index = t.Handle.Raw;
            var body = _activeBodies[index]; 
            if (body) body.StateUpdate(t);
        }
    }
    
    private void OnDebrisSpawned(DebrisSpawnedEvent e)
    {
        if (_activeWorld == null || !e.bodyRef) return;
        
        var bodyHandle = _activeWorld.AddBody(e.joltBodyDesc);
        _activeBodies[bodyHandle.Raw] = e.bodyRef;
    }

    private void OnDebrisDestroyed(DebrisDestroyedEvent e)
    {
        
    }
}
