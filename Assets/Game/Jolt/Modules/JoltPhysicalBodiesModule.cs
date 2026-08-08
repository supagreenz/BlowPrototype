using System;
using Game.Jolt;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class JoltPhysicalBodiesModule : IDisposable
{
    private TransformAccessArray _bodyTransforms;
    private NativeArray<JoltBodyState> _bodyStateBuffer;
    
    private JoltWorld _activeWorld;

    public void Init(JoltWorld world)
    {
        _activeWorld = world;

        // Sized from the world itself: Jolt hands out slot indices up to its
        // capacity, and anything smaller here throws the moment it does.
        _bodyTransforms = new TransformAccessArray(new Transform[world.Capacity]);
        _bodyStateBuffer = new NativeArray<JoltBodyState>(world.Capacity, Allocator.Persistent);

        EventBus<DebrisSpawnedEvent>.Subscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Subscribe(OnDebrisDestroyed);
    }

    public void Dispose()
    {
        EventBus<DebrisSpawnedEvent>.Unsubscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Unsubscribe(OnDebrisDestroyed);

        if (_bodyTransforms.isCreated) _bodyTransforms.Dispose();
        if (_bodyStateBuffer.IsCreated) _bodyStateBuffer.Dispose();

        _activeWorld = null;
    }

    public void UpdateStep()
    {
        var states = _activeWorld.ReadStates();

        states.CopyTo(_bodyStateBuffer.GetSubArray(0, states.Length).AsSpan());

        new MoveBodiesJob
        {
            states = _bodyStateBuffer,
        }.Schedule(_bodyTransforms).Complete();
    }
    
    private void OnDebrisSpawned(DebrisSpawnedEvent e)
    {
        if (_activeWorld == null || !e.bodyRef) return;
        
        var bodyHandle = _activeWorld.AddBody(e.joltBodyDesc);
        if (!bodyHandle.IsValid) return;
        _bodyTransforms[bodyHandle.Index] = e.bodyRef.transform;
    }

    private void OnDebrisDestroyed(DebrisDestroyedEvent e)
    {
        
    }
}

[BurstCompile]
public struct MoveBodiesJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<JoltBodyState> states;

    public void Execute(int index, TransformAccess transform)
    {
        JoltBodyState state = states[index];

        // A free slot comes back zeroed, so its rotation is (0,0,0,0) — not a
        // valid quaternion. Skipping also means empty slots never touch their
        // (null) transform.
        if (!state.IsValid) return;

        transform.SetPositionAndRotation(state.Position, state.Rotation);
    }
}