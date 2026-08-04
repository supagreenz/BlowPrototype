using System;
using System.Collections.Generic;
using UnityEngine;

public class PushField : MonoBehaviour
{
    [SerializeField] private PushFieldCollider _fieldCollider;
    
    private HashSet<Debris> _debrisInField = new();

    private void Awake()
    {
        if (_fieldCollider)
        {
            _fieldCollider.OnEnter += OnEnter;
            _fieldCollider.OnExit += OnExit;
        }
    }

    private void OnEnter(Collider collider)
    {
        if (collider.TryGetComponent(out Debris debris))
        {
            _debrisInField.Add(debris);
        }
    }

    private void OnExit(Collider collider)
    {
        if (collider.TryGetComponent(out Debris debris))
        {
            _debrisInField.Remove(debris);
        }
    }

    private void FixedUpdate()
    {
        Debug.Log(_debrisInField.Count);
    }
}
