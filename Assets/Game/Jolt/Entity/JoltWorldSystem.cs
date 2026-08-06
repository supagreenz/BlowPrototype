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
    public const int MaximumWorldBodies = 2048;

    /// <summary>
    /// The live world, or null before the ECS world has been created and
    /// after it has been torn down. MonoBehaviours reach the simulation
    /// through here; nothing outside this system may dispose it.
    /// </summary>
    public static JoltWorld Active { get; private set; }

    /// <summary>
    /// The running instance, for the MonoBehaviours that still need to read
    /// state. Null outside of play.
    /// </summary>
    public static JoltWorldSystem Instance { get; private set; }

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
        Jolt = new JoltWorld(MaximumWorldBodies * 3);
        _states = new NativeArray<JoltBodyState>(Jolt.Capacity, Allocator.Persistent);

        Active = Jolt;
        Instance = this;
        Enabled = false;
    }

    protected override void OnDestroy()
    {
        if (ReferenceEquals(Active, Jolt)) Active = null;
        if (ReferenceEquals(Instance, this)) Instance = null;

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
