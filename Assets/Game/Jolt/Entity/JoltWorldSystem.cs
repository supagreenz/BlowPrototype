using Game.Jolt;
using Unity.Collections;
using Unity.Entities;

/// <summary>
/// Owns the one Jolt world everything simulates in, and the one snapshot of it
/// everything reads. Never updates itself: it exists for the create and
/// dispose ends of the world's life, and as the place the rest of the pipeline
/// meets.
///
/// There is deliberately a single world rather than one per path. Entity
/// bodies and the GameObject bodies that have not been migrated yet — the
/// arena walls above all — have to collide with each other, and bodies in
/// separate worlds cannot see each other at all.
///
/// One tick touches Jolt exactly three times, in this order: push, step, pull.
/// JoltPushSystem accumulates forces, JoltStepSystem advances the simulation,
/// JoltReadbackSystem takes the single snapshot into <see cref="States"/>.
/// Everything downstream — entity writeback, JoltEngine's remaining GameObject
/// bodies — reads that snapshot rather than going back to the world.
/// </summary>
public partial class JoltWorldSystem : SystemBase
{
    public static readonly int MaximumWorldBodies = 2048;

    /// <summary>
    /// Worker threads Jolt simulates on. Passing 0 would take processor count
    /// minus one, which sizes Jolt's pool for the whole machine on top of
    /// Unity's job system doing the same — twice the threads, same cores. Set
    /// back to 0 to compare.
    /// </summary>
    public const int PhysicsWorkerThreads = 8;

    /// <summary>
    /// The live world, or null once it has been torn down. Nothing outside
    /// this system may dispose it.
    ///
    /// Deliberately not exposed through a static: domain reload is off, so a
    /// static reference would survive stop play still pointing at a disposed
    /// world. MonoBehaviours resolve this system through
    /// World.DefaultGameObjectInjectionWorld instead.
    /// </summary>
    public JoltWorld Jolt { get; private set; }

    /// <summary>
    /// Every body's state as of the last step, indexed by
    /// <see cref="JoltBodyHandle.Index"/>. Refreshed once per tick by
    /// JoltReadbackSystem; only the first <see cref="StateCount"/> entries
    /// hold anything.
    /// </summary>
    public NativeArray<JoltBodyState> States => _states;

    public int StateCount { get; private set; }

    private NativeArray<JoltBodyState> _states;

    protected override void OnCreate()
    {
        Jolt = new JoltWorld(MaximumWorldBodies * 3, PhysicsWorkerThreads);
        _states = new NativeArray<JoltBodyState>(Jolt.Capacity, Allocator.Persistent);

        Enabled = false;
    }

    protected override void OnDestroy()
    {
        if (_states.IsCreated) _states.Dispose();

        Jolt?.Dispose();
        Jolt = null;
    }

    /// <summary>
    /// The tick's single read of the native world. Called by
    /// JoltReadbackSystem, once, after the step; nothing else should pull from
    /// Jolt directly.
    /// </summary>
    internal void PullStates()
    {
        if (Jolt == null || !Jolt.IsValid)
        {
            StateCount = 0;
            return;
        }

        var states = Jolt.ReadStates();
        states.CopyTo(_states.GetSubArray(0, states.Length).AsSpan());
        StateCount = states.Length;
    }

    /// <summary>
    /// This body's state in the current snapshot. False for a body that is
    /// gone, or whose slot has since been handed to someone else.
    /// </summary>
    public bool TryGetState(JoltBodyHandle body, out JoltBodyState state)
    {
        state = default;

        if (!body.IsValid) return false;

        int slot = body.Index;
        if ((uint)slot >= (uint)StateCount) return false;

        JoltBodyState candidate = _states[slot];

        // The whole handle, generation included: a recycled slot holds a
        // different body now.
        if (!candidate.IsValid || candidate.RawHandle != body.Raw) return false;

        state = candidate;
        return true;
    }

    protected override void OnUpdate()
    {
    }
}
