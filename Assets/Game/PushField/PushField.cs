using System;
using System.Collections.Generic;
using UnityEngine;

public class PushField : MonoBehaviour
{
    [SerializeField] private PushFieldCollider fieldCollider;
    [SerializeField] private float pushForce = 10;
    
    private readonly Collider[] _colliderBuffer = new Collider[1024];
    private Transform _thisT;
    
    private void Awake()
    {
        _thisT = transform;
    }

    private void FixedUpdate()
    {
        RunFieldPushBeat();
    }

    private void RunFieldPushBeat()
    {
        RunCollisionTest();
        RunPush();
    }

    private void RunCollisionTest()
    {
        if (!fieldCollider) return;
        int res = fieldCollider.ColliderTest(_colliderBuffer);
    }

    private void RunPush()
    {
        Vector3 cPos = _thisT.position;
        foreach (var c in _colliderBuffer)
        {
            if (!c || !c.TryGetComponent(out JoltBody jBody)) continue;
            Vector3 dPos = jBody.transform.position;
            Vector3 dn = dPos - cPos;
            dn = dn.normalized;
            jBody.AddPush(dn.normalized * pushForce);
        }
    }
}