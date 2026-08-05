using System;
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

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        RunTest();
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }


    private void RunTest()
    {
        Debug.Log("Testing JoltEngine");
    }
}
