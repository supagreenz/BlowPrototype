using System;
using Game.Jolt;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

public class JoltEngine : MonoBehaviour
{
    [AutoStaticsCleanup] private static JoltEngine _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void CreateInstance()
    {
        if (_instance) return;
        GameObject jeo = new GameObject("JoltEngine");
        DontDestroyOnLoad(jeo);
        _instance = jeo.AddComponent<JoltEngine>();
    }

    private JoltWorld _activeWorld;

    private JoltBodyHandle _boxHandle;
    private JoltBodyHandle _ballHandle;

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitJoltWorld();
    }

    private void OnDestroy()
    {
        DisposeJoltWorld();
        
        if (_instance == this) _instance = null;
    }


    private void InitJoltWorld()
    {
        _activeWorld = new JoltWorld(1024);
        
        // Spawn a box
        _boxHandle = _activeWorld.AddBody(JoltBodyDesc.Box(Vector3.one * 0.5f, Vector3.zero, Quaternion.identity,
            JoltMotion.Static));
        
        // Spawn a ball
        _ballHandle = _activeWorld.AddBody(JoltBodyDesc.Sphere(0.3f, new Vector3(0, 2, 0), JoltMotion.Dynamic));
    }

    private void DisposeJoltWorld()
    {
        _activeWorld?.Dispose();
        _activeWorld = null;
    }

    private void FixedUpdate()
    {
        // Read ball and box
        
        if (_activeWorld == null) return;
        
        _activeWorld.Step(Time.fixedDeltaTime);
        
        var states = _activeWorld.ReadStates();
        foreach (JoltBodyState s in states)
        {
            Debug.Log(s.Position);
        }
    }
}
