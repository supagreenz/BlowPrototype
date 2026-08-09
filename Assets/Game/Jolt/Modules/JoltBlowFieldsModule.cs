using System;
using System.Collections.Generic;
using Game.Jolt;
using UnityEngine;

public class JoltBlowFieldsModule : IDisposable
{
    
    private JoltWorld _activeWorld;
    
    
    private JoltBodyHandle[] _blowFieldBuffer = new JoltBodyHandle[1024];
    private List<BlowField> _activeBlowFields = new();
    

    public void Init(JoltWorld joltWorld)
    {
        _activeWorld = joltWorld;
        
        EventBus<BlowFieldRegisterEvent>.Subscribe(OnPushFieldSpawned);
        EventBus<BlowFieldUnregisterEvent>.Subscribe(OnPushFieldDestroyed);
    }
    
    public void Dispose()
    {
        _activeWorld = null;
        
        EventBus<BlowFieldRegisterEvent>.Unsubscribe(OnPushFieldSpawned);
        EventBus<BlowFieldUnregisterEvent>.Unsubscribe(OnPushFieldDestroyed);
    }

    public void UpdateStep()
    {
        foreach (var bf in _activeBlowFields)
        {
            bf.GetColliderBox(out var center, out Vector3 extents, out Quaternion rot);
            int cols = _activeWorld.OverlapBox(center, extents, _blowFieldBuffer, rot);
        
            for (int i = 0; i < cols; i++)
            {
                var h = _blowFieldBuffer[i];
                if (_activeWorld.TryGetState(h, out JoltBodyState s))
                {
                    _activeWorld.AddForce(h, bf.CalculatePushFrom(s.Position));  
                } 
            }   
        }
    }
    
    private void OnPushFieldSpawned(BlowFieldRegisterEvent e)
    {
        if (!e.BlowField) return;

        _activeBlowFields.Add(e.BlowField);
    }

    private void OnPushFieldDestroyed(BlowFieldUnregisterEvent e)
    {
        _activeBlowFields.Remove(e.BlowField);
    }
}
