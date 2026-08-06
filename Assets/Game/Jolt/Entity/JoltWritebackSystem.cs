using Game.Jolt;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Pushes the results of the last step onto LocalTransform, the counterpart of
/// JoltBody.StateUpdate.
///
/// The GameObject path walks the state buffer and looks each body up in a
/// dictionary. Here it runs the other way round: every entity carries the slot
/// its body occupies, so writeback is an indexed read per entity and the whole
/// thing parallelises.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(JoltStepSystem))]
public partial class JoltWritebackSystem : SystemBase
{
    private JoltWorldSystem _worldSystem;

    protected override void OnCreate()
    {
        _worldSystem = World.GetOrCreateSystemManaged<JoltWorldSystem>();
    }

    protected override void OnUpdate()
    {
        JoltWorld jolt = _worldSystem.Jolt;
        if (jolt == null || !jolt.IsValid) return;

        var states = jolt.ReadStates();
        if (states.Length == 0) return;

        // ReadStates views a pinned managed buffer that the next read
        // overwrites, so the job works from its own copy.
        var stateCopy = new NativeArray<JoltBodyState>(states.Length, Allocator.TempJob,
            NativeArrayOptions.UninitializedMemory);
        states.CopyTo(stateCopy.AsSpan());

        Dependency = new WritebackJob { States = stateCopy }.ScheduleParallel(Dependency);
        Dependency = stateCopy.Dispose(Dependency);
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
