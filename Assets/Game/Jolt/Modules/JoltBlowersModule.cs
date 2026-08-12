using System;
using System.Collections.Generic;
using Game.Jolt;
using UnityEngine;

public class JoltBlowersModule : IDisposable
{
    
    private JoltWorld _activeWorld;
    
    private readonly JoltBodyHandle[] _blowerColBuffer = new JoltBodyHandle[JoltConstants.MaxWorldBodies];
    private readonly Vector3[] _totalForces = new Vector3[JoltConstants.MaxWorldBodies];
    private readonly List<Blower> _activeBlowers = new();
    

    public void Init(JoltWorld joltWorld)
    {
        _activeWorld = joltWorld;
        
        EventBus<BlowerRegisterEvent>.Subscribe(OnPushFieldSpawned);
        EventBus<BlowerUnregisterEvent>.Subscribe(OnPushFieldDestroyed);
    }
    
    public void Dispose()
    {
        _activeWorld = null;
        
        EventBus<BlowerRegisterEvent>.Unsubscribe(OnPushFieldSpawned);
        EventBus<BlowerUnregisterEvent>.Unsubscribe(OnPushFieldDestroyed);
    }

    public void UpdateStep(ReadOnlySpan<JoltBodyState> states)
    {
        foreach (Blower bf in _activeBlowers)
        {
            // bf.GetColliderBox(out var center, out Vector3 extents, out Quaternion rot);
            // int cols = _activeWorld.OverlapBox(center, extents, _blowerColBuffer, rot);
            //
            // for (int i = 0; i < cols; i++)
            // {
            //     var h = _blowerColBuffer[i];
            //     if (_activeWorld.TryGetState(h, out JoltBodyState s))
            //     {
            //         _activeWorld.AddForce(h, bf.CalculatePushFrom(s.Position));  
            //     } 
            // }

            int cols = _activeWorld.OverlapShape(bf.GetCurrentShapeTest(), _blowerColBuffer);
            //bf.CalculatePushesAt(cols, _blowerColBuffer, _totalForces, states);
            
            for (int i = 0; i < cols; i++)
            {
                var h = _blowerColBuffer[i];
                _activeWorld.AddForceAtPoint(h, 
                    bf.CalculatePushAt(states[i].Position), states[i].Position + (Vector3.down * 0.1f));
            }
        }
    }
    
    private void OnPushFieldSpawned(BlowerRegisterEvent e)
    {
        if (!e.Blower) return;

        _activeBlowers.Add(e.Blower);
    }

    private void OnPushFieldDestroyed(BlowerUnregisterEvent e)
    {
        _activeBlowers.Remove(e.Blower);
    }
}
