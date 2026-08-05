using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ForCam
{
    A,
    B
}

[RequireComponent(typeof(CinemachineCamera))]
public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private ForCam forCam = ForCam.A;

    private CinemachineCamera _cineCam;
    
    private void Awake()
    {
        _cineCam = GetComponent<CinemachineCamera>();
    }

    private void Update()
    {
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            forCam = forCam == ForCam.A ? ForCam.B : ForCam.A;
            UpdateCamval();
        }
    }

    private void UpdateCamval()
    {
        _cineCam.Priority = forCam == ForCam.A ? 10 : 5;
    }
}
