using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Debris : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 20f;

    private Rigidbody _rbody;
    
    private void Awake()
    {
        _rbody = GetComponent<Rigidbody>();
    }

    public void AddPush(Vector3 push)
    {
        _rbody.AddForce(push, ForceMode.Force);
    }

    private void FixedUpdate()
    {
        var v = _rbody.linearVelocity;
        if (v.sqrMagnitude > maxSpeed * maxSpeed) _rbody.linearVelocity = v.normalized * maxSpeed;
    }
}
