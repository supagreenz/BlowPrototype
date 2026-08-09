using System;
using Game.Jolt;
using UnityEngine;


public class RegularBlower : Blower
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
            Debug.LogError("RegularBlower requires a capsule collider");
        }
    }
    
    public override JoltShapePose GetCurrentShapeTest()
    {
        if (!_capsule) throw new NotSupportedException("RegularBlower requires a capsule collider");
        
        return new JoltShapePose(_activeShapeData, _capsT.position, _capsT.rotation);
    }

    public override Vector3 CalculatePushAt(Vector3 at)
    {
         return (at - CPos + Vector3.up).normalized * pushForce;
    }
}
