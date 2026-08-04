using System;
using UnityEngine;

public class Hero : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float sprintMultiplier = 2.5f;
    [SerializeField] private float turnSpeed = 720f;
    
    
    
    private CharacterController _controller;
    private Camera _camRef;
    
    
    private float _currentSpeed = 0f;
    
    private void Awake()
    {
        _camRef = Camera.main;
        _controller = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!_camRef || !_controller) return;

        Vector2 input = InputBus.GetMoveDir();
        
        Vector3 camForward = _camRef.transform.forward;
        Vector3 camRight = _camRef.transform.right;
        
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        
        Vector3 moveDir = camRight * input.x + camForward * input.y;
        float targetMaxSpeed = (input.sqrMagnitude > 0) ? moveSpeed * (InputBus.IsSprinting() ? sprintMultiplier : 1f) : 0f;
        
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetMaxSpeed, acceleration * Time.deltaTime);

        _controller.SimpleMove(moveDir * _currentSpeed);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}
