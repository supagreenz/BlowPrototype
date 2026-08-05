using System;
using Game.Jolt;
using UnityEngine;

public abstract class JoltBody : MonoBehaviour
{
    [SerializeField] protected JoltBodyType joltBodyType = JoltBodyType.None;
    
    private void Awake()
    {
        EventBus<DebrisSpawnedEvent>.Raise(new (){joltBodyDesc = ConstructJoltBodyDesc()});
        
        if (joltBodyType == JoltBodyType.None) Destroy(gameObject);
    }

    private void OnDestroy()
    {
        EventBus<DebrisDestroyedEvent>.Raise(new ());
    }

    protected virtual JoltBodyDesc ConstructJoltBodyDesc()
    {
        throw new NotImplementedException();
    }
}

public enum JoltBodyType
{
    None = 0,
    Sphere = 1,
    Box = 2,
    Capsule = 3,
}