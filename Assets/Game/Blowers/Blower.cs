using System;
using Game.Jolt;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class Blower : MonoBehaviour
{
    [SerializeField] protected Collider mainCollider;
    [SerializeField] protected float pushForce = 10;
    
    protected Transform ThisT;
    protected Vector3 CPos;
    protected Vector3 CForward;

    protected virtual void Awake()
    {
        ThisT = transform;
        
        EventBus<BlowerRegisterEvent>.Raise(new (){Blower = this});
    }

    protected virtual void OnValidate()
    {
        if (!mainCollider)
        {
            Debug.LogError("The main collider is null. Make a new one as a child with correct orientation");
        }
    }

    protected void OnDestroy()
    {
        EventBus<BlowerUnregisterEvent>.Raise(new (){Blower = this});
    }

    protected virtual void FixedUpdate()
    {
        CPos = ThisT.position;
        CForward = ThisT.forward;
    }

    public abstract JoltShapePose GetCurrentShapeTest();
    public abstract Vector3 CalculatePushAt(Vector3 at);
    public abstract void CalculatePushesAt(int collisions, JoltBodyHandle[] bodies, Vector3[] forcesBuffer, ReadOnlySpan<JoltBodyState> statesBuffer);
}
