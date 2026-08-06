using System;
using System.Collections.Generic;
using UnityEngine;

public class PushField : MonoBehaviour
{
    [SerializeField] private PushFieldCollider fieldCollider;
    [SerializeField] private float pushForce = 10;
    
    private readonly Collider[] _colliderBuffer = new Collider[1024];
    private Transform _thisT;
    private Vector3 cPos;
    
    private void Awake()
    {
        _thisT = transform;
    }

    private void Start()
    {
        EventBus<PushFieldSpawnedEvent>.Raise(new (){pushField = this});
    }

    private void OnDestroy()
    {
        EventBus<PushFieldDestroyedEvent>.Raise(new ());
    }

    public void GetColliderBox(out Vector3 center, out Vector3 extents, out Quaternion rot)
    {
        fieldCollider.GetColliderBox(out center, out extents, out rot);
    }

    private void FixedUpdate()
    {
        cPos = _thisT.position;
    }

    public Vector3 CalculatePushFrom(Vector3 bodyPos)
    {
        Vector3 dn = bodyPos - cPos;
        dn = dn.normalized * pushForce;
        return dn;
    }
}