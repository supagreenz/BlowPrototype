using System;
using UnityEngine;

[RequireComponent(typeof(JoltBody))]
public class Debris : MonoBehaviour
{
    private JoltBody _jBody;
    
    private void Awake()
    {
        _jBody = GetComponent<JoltBody>();
    }
}