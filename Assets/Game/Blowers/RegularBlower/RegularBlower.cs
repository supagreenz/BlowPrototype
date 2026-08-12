using System;
using Game.Jolt;
using UnityEngine;
using UnityEngine.Serialization;


public class RegularBlower : Blower
{
    [SerializeField] private float upFactor = 0.2f;
    [FormerlySerializedAs("convergePoint")] [SerializeField] private float convergeDistance = 10000f;
    
    
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
         var convergePoint = CPos + (CForward + Vector3.up * upFactor) * convergeDistance;
         var pushDir = (convergePoint - at).normalized;
         return pushDir * pushForce;
    }

    public override void CalculatePushesAt(int collisions, JoltBodyHandle[] bodies, Vector3[] forcesBuffer, ReadOnlySpan<JoltBodyState> statesBuffer)
    {
        var convergePoint = CPos + (CForward + Vector3.up * upFactor) * convergeDistance;
        for (int i = 0; i < collisions; ++i)
        {
            var pushDir = (convergePoint - statesBuffer[i].Position).normalized;
            forcesBuffer[i] += pushDir * pushForce;
        }
    }
}
