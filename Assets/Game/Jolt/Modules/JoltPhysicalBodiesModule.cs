using System;
using System.Collections.Generic;
using Game.Jolt;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class JoltPhysicalBodiesModule : IDisposable
{
    /// <summary>Speed at which stretch reaches its maximum.</summary>
    public float StretchAtSpeed = 15f;

    /// <summary>How far a body elongates at full speed. 1 doubles its length.</summary>
    public float MaxStretch = 1.2f;

    /// <summary>Below this speed there is no meaningful direction of travel.</summary>
    public float MinStretchSpeed = 0.5f;

    /// <summary>
    /// How quickly a body reaches its target stretch. Higher is snappier; at
    /// around 8 the shape settles in roughly a third of a second.
    /// </summary>
    public float StretchSharpness = 8f;

    private TransformAccessArray _bodyTransforms;
    private NativeArray<JoltBodyState> _bodyStateBuffer;

    // The authored scale of each body, so stretch multiplies it rather than
    // replacing it — the debris prefabs are authored at 0.5.
    private NativeArray<float3> _baseScales;

    // Carried between ticks so the shape can ease toward its target rather
    // than snapping to whatever this tick's velocity implies.
    private NativeArray<DebrisStretchState> _stretchStates;

    private JoltWorld _activeWorld;

    public void Init(JoltWorld world)
    {
        _activeWorld = world;

        // Sized from the world itself: Jolt hands out slot indices up to its
        // capacity, and anything smaller here throws the moment it does.
        _bodyTransforms = new TransformAccessArray(new Transform[world.Capacity]);
        _bodyStateBuffer = new NativeArray<JoltBodyState>(world.Capacity, Allocator.Persistent);
        _baseScales = new NativeArray<float3>(world.Capacity, Allocator.Persistent);
        _stretchStates = new NativeArray<DebrisStretchState>(world.Capacity, Allocator.Persistent);

        EventBus<DebrisSpawnedEvent>.Subscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Subscribe(OnDebrisDestroyed);
    }

    public void Dispose()
    {
        EventBus<DebrisSpawnedEvent>.Unsubscribe(OnDebrisSpawned);
        EventBus<DebrisDestroyedEvent>.Unsubscribe(OnDebrisDestroyed);

        if (_bodyTransforms.isCreated) _bodyTransforms.Dispose();
        if (_bodyStateBuffer.IsCreated) _bodyStateBuffer.Dispose();
        if (_baseScales.IsCreated) _baseScales.Dispose();
        if (_stretchStates.IsCreated) _stretchStates.Dispose();

        _activeWorld = null;
    }

    public void UpdateStep(ReadOnlySpan<JoltBodyState> states)
    {
        states.CopyTo(_bodyStateBuffer.GetSubArray(0, states.Length).AsSpan());

        new MoveBodiesJob
        {
            states = _bodyStateBuffer,
            baseScales = _baseScales,
            stretchStates = _stretchStates,
            stretchAtSpeed = math.max(0.01f, StretchAtSpeed),
            maxStretch = MaxStretch,
            minStretchSpeed = MinStretchSpeed,
            stretchSharpness = StretchSharpness,
            deltaTime = Time.fixedDeltaTime,
        }.Schedule(_bodyTransforms).Complete();
    }

    private void OnDebrisSpawned(DebrisSpawnedEvent e)
    {
        if (_activeWorld == null || !e.bodyRef) return;

        var bodyHandle = _activeWorld.AddBody(e.joltBodyDesc);
        if (!bodyHandle.IsValid) return;

        Transform bodyTransform = e.bodyRef.transform;
        Vector3 authoredScale = bodyTransform.localScale;

        _bodyTransforms[bodyHandle.Index] = bodyTransform;
        _baseScales[bodyHandle.Index] = new float3(authoredScale.x, authoredScale.y, authoredScale.z);

        // At rest, and with no axis yet — a slot may be carrying whatever its
        // previous occupant left behind.
        _stretchStates[bodyHandle.Index] = new DebrisStretchState
        {
            Axis = float3.zero,
            Stretch = 1f,
        };
    }

    private void OnDebrisDestroyed(DebrisDestroyedEvent e)
    {
        
    }
}

/// <summary>
/// What a body's shape is doing right now, carried between ticks so it can
/// ease rather than snap. Axis is the last heading worth stretching along,
/// kept while the body slows so it has something to relax back along.
/// </summary>
public struct DebrisStretchState
{
    public float3 Axis;
    public float Stretch;
}

/// <summary>
/// Writes the simulation onto the render transforms, stretching each body
/// along its direction of travel as it goes.
///
/// The stretch is volume preserving — elongating by s narrows both other axes
/// by 1/sqrt(s) — which is what makes it read as an elastic body rather than
/// one that simply grows.
///
/// This spends the body's rotation on the direction of travel, so it only
/// works for spheres, whose physics rotation is invisible anyway. A box would
/// lose its tumble and look obviously wrong.
/// </summary>
[BurstCompile]
public struct MoveBodiesJob : IJobParallelForTransform
{
    /// <summary>At or below this, a body counts as back at its rest shape.</summary>
    private const float RestThreshold = 0.002f;

    [ReadOnly] public NativeArray<JoltBodyState> states;
    [ReadOnly] public NativeArray<float3> baseScales;

    // Each index is touched by exactly one worker, since the transform index
    // and the array index are the same. The safety system cannot know that.
    [NativeDisableParallelForRestriction] public NativeArray<DebrisStretchState> stretchStates;

    public float stretchAtSpeed;
    public float maxStretch;
    public float minStretchSpeed;
    public float stretchSharpness;
    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        JoltBodyState state = states[index];

        // A free slot comes back zeroed, so its rotation is (0,0,0,0) — not a
        // valid quaternion. Skipping also means empty slots never touch their
        // (null) transform.
        if (!state.IsValid) return;
        if (!transform.isValid) return;

        float3 baseScale = baseScales[index];
        float3 velocity = new float3(state.LinearVelocity.x, state.LinearVelocity.y, state.LinearVelocity.z);
        float speed = math.length(velocity);

        DebrisStretchState stretchState = stretchStates[index];

        // Slowing to a stop targets the rest shape rather than dropping the
        // effect outright, and the last known heading is kept so the body has
        // an axis to relax along on the way there.
        float targetStretch = 1f;
        if (speed >= minStretchSpeed)
        {
            targetStretch = 1f + math.saturate(speed / stretchAtSpeed) * maxStretch;
            stretchState.Axis = velocity / speed;
        }

        // Framerate independent easing: a fixed lerp factor would change the
        // feel with the timestep.
        float ease = 1f - math.exp(-stretchSharpness * deltaTime);
        stretchState.Stretch = math.lerp(stretchState.Stretch, targetStretch, ease);
        stretchStates[index] = stretchState;

        bool settled = stretchState.Stretch <= 1f + RestThreshold;
        bool hasAxis = math.lengthsq(stretchState.Axis) > 0.5f;

        if (settled || !hasAxis)
        {
            // Only once the shape is back to rest does the body get its own
            // rotation again — handing it back mid-stretch would pop.
            transform.SetPositionAndRotation(state.Position, state.Rotation);
            transform.localScale = new Vector3(baseScale.x, baseScale.y, baseScale.z);
            return;
        }

        float stretch = stretchState.Stretch;
        float squash = math.rsqrt(stretch);

        // Local Y is put on the stretch axis, so scaling Y elongates along
        // travel. LookRotationSafe orthonormalises, and the forward it is
        // handed is already perpendicular, so Y lands exactly on the axis.
        quaternion aligned = quaternion.LookRotationSafe(AnyPerpendicular(stretchState.Axis), stretchState.Axis);

        transform.SetPositionAndRotation(state.Position,
            new Quaternion(aligned.value.x, aligned.value.y, aligned.value.z, aligned.value.w));

        float3 stretched = baseScale * new float3(squash, stretch, squash);
        transform.localScale = new Vector3(stretched.x, stretched.y, stretched.z);
    }

    /// <summary>
    /// Any unit vector at right angles to v. Crossing against whichever world
    /// axis v is least aligned with keeps the result well conditioned.
    /// </summary>
    private static float3 AnyPerpendicular(float3 v)
    {
        float3 reference = math.abs(v.y) < 0.9f ? new float3(0f, 1f, 0f) : new float3(1f, 0f, 0f);
        return math.normalize(math.cross(v, reference));
    }
}