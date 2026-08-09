using System;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class Blower : MonoBehaviour
{
    [SerializeField] protected Collider mainCollider;
    [SerializeField] protected float pushForce = 10;
    
    protected Transform ThisT;

    protected void Awake()
    {
        ThisT = transform;
        
        EventBus<BlowerRegisterEvent>.Raise(new (){Blower = this});
    }

    private void OnValidate()
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
}
