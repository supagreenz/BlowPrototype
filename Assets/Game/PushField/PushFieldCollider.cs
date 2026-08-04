using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PushFieldCollider : MonoBehaviour
{
    public event Action<Collider> OnEnter;
    public event Action<Collider> OnExit;
    
    private void OnTriggerEnter(Collider other)
    {
        OnEnter?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        OnExit?.Invoke(other);
    }
}
