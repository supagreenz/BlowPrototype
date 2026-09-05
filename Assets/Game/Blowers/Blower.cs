using System;
using Game.Jolt;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public abstract class Blower : MonoBehaviour
{
    [SerializeField] protected Collider mainCollider;
    [SerializeField] protected float pushForce = 10;
    
    protected Transform ThisT;
    protected Vector3 CPos;
    protected Vector3 CForward;

    protected BlowerStatus ActiveStatus = BlowerStatus.Deactivated;

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
        
        ActiveStatus = Mouse.current.leftButton.isPressed? BlowerStatus.Activated : BlowerStatus.Deactivated;

        if (ActiveStatus == BlowerStatus.Activated) TickActive();
        else TickInactive();
    }

    protected virtual void TickActive()
    {
    }
    protected virtual void TickInactive()
    {
    }

    protected void TransitionStatus(BlowerStatus newStatus)
    {
        // if (ActiveStatus == newStatus) return;
        ActiveStatus = newStatus;
    }
    
    public bool IsActive => ActiveStatus == BlowerStatus.Activated;
    
    public abstract JoltShapePose GetCurrentShapeTest();
    public abstract Vector3 CalculatePushAt(Vector3 at);
    public abstract void CalculatePushesAt(int collisions, JoltBodyHandle[] bodies, Vector3[] forcesBuffer, ReadOnlySpan<JoltBodyState> statesBuffer);
}

public enum BlowerStatus
{
    Activated,
    Deactivated
}