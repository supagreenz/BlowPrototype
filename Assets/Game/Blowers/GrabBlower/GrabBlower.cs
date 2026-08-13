using System;
using Game.Jolt;
using UnityEngine;

public class GrabBlower : Blower
{
    
    private CapsuleCollider _capsule;
    private Transform _capsT;
    private JoltShapeData _activeShapeData;

    protected override void Awake()
    {
        base.Awake();
        _capsule = (CapsuleCollider) mainCollider;
        _capsT = _capsule.transform;
        if (_capsule)
        {
            _activeShapeData = JoltShapeData.Capsule(_capsule.height * 0.5f, _capsule.radius);
        }
    }
    
    protected override void OnValidate()
    {
        base.OnValidate();

        if (mainCollider is not CapsuleCollider)
        {
            Debug.LogError("GrabBlower requires a capsule collider");
        }
    }
    
    public override JoltShapePose GetCurrentShapeTest()
    {
        if (!_capsule) throw new NotSupportedException("GrabBlower requires a capsule collider");
        
        return new JoltShapePose(_activeShapeData, _capsT.position, _capsT.rotation);
    }

    public override Vector3 CalculatePushAt(Vector3 at)
    {
        throw new NotImplementedException();
    }

    public override void CalculatePushesAt(int collisions, JoltBodyHandle[] bodies, Vector3[] forcesBuffer, ReadOnlySpan<JoltBodyState> statesBuffer)
    {
        throw new NotImplementedException();
    }
}
