using System;
using Game.Jolt;
using UnityEngine;

public class JoltBlowFieldsModule : IDisposable
{
    
    private JoltWorld _activeWorld;
    
    
    private JoltBodyHandle[] _pushFieldBuffer = new JoltBodyHandle[1024];
    private PushField _activePushField;
    

    public void Init(JoltWorld joltWorld)
    {
        _activeWorld = joltWorld;
        
        EventBus<PushFieldSpawnedEvent>.Subscribe(OnPushFieldSpawned);
        EventBus<PushFieldDestroyedEvent>.Subscribe(OnPushFieldDestroyed);
    }
    
    public void Dispose()
    {
        _activeWorld = null;
        
        EventBus<PushFieldSpawnedEvent>.Unsubscribe(OnPushFieldSpawned);
        EventBus<PushFieldDestroyedEvent>.Unsubscribe(OnPushFieldDestroyed);
    }

    public void UpdateStep()
    {
        
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
