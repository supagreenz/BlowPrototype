using System;
using Game.Jolt;
using UnityEngine;

public abstract class JoltBody : MonoBehaviour
{
    [SerializeField] protected JoltBodyType joltBodyType = JoltBodyType.None;

    private Transform _thisT;
    
    private void Awake()
    {
        EventBus<DebrisSpawnedEvent>.Raise(new ()
        {
            joltBodyDesc = ConstructJoltBodyDesc(),
            bodyRef = this
        });
        
        if (joltBodyType == JoltBodyType.None) Destroy(gameObject);
        _thisT = transform;
    }

    private void OnDestroy()
    {
        EventBus<DebrisDestroyedEvent>.Raise(new ());
    }

    protected abstract JoltBodyDesc ConstructJoltBodyDesc();

    public virtual void StateUpdate(JoltBodyState newState)
    {
        _thisT.position = newState.Position;
        _thisT.rotation = newState.Rotation;
    }
    
    public void AddPush(Vector3 push)
    {
        
    }
}

public enum JoltBodyType
{
    None = 0,
    Sphere = 1,
    Box = 2,
    Capsule = 3,
    
    Wall = 100,
}