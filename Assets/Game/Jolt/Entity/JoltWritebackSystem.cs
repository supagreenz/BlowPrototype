using Game.Jolt;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// The update stage: pushes the tick's snapshot onto LocalTransform, the
/// counterpart of JoltBody.StateUpdate.
///
/// Reads the shared snapshot rather than the world, and reads it by slot: each
/// entity indexes straight to its own body instead of anyone scanning the
/// whole buffer looking for owners.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(JoltReadbackSystem))]
public partial class JoltWritebackSystem : SystemBase
{
    private JoltWorldSystem _worldSystem;

    protected override void OnCreate()
    {
        _worldSystem = World.GetOrCreateSystemManaged<JoltWorldSystem>();
    }

    protected override void OnUpdate()
    {
        int stateCount = _worldSystem.StateCount;
        if (stateCount == 0) return;

        // A view of the shared snapshot, not a copy of it.
        var states = _worldSystem.States.GetSubArray(0, stateCount);

        Dependency = new WritebackJob { States = states }.ScheduleParallel(Dependency);

        // Completed here rather than left running: next tick's readback
        // overwrites this very buffer from the main thread, and JoltEngine
        // reads it from FixedUpdate on a clock of its own.
        Dependency.Complete();
    }

    [BurstCompile]
    [WithNone(typeof(JoltStaticBody))]
    private partial struct WritebackJob : IJobEntity
    {
        [ReadOnly] public NativeArray<JoltBodyState> States;

        private void Execute(in JoltBodyRef body, ref LocalTransform transform)
        {
            int slot = body.Handle.Index;
            if ((uint)slot >= (uint)States.Length) return;

            JoltBodyState state = States[slot];

            // Compare the whole handle, generation included. A recycled slot
            // holds a different body now, and an entity whose registration was
            // rejected holds no body at all.
            if (!state.IsValid || state.RawHandle != body.Handle.Raw) return;

            transform.Position = new float3(state.Position.x, state.Position.y, state.Position.z);
            transform.Rotation = new quaternion(state.Rotation.x, state.Rotation.y, state.Rotation.z,
                state.Rotation.w);
        }
    }
}
