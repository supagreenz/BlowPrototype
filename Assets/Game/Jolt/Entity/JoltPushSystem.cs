using System.Collections.Generic;
using Game.Jolt;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// The push stage: everything that accumulates force on a body happens here,
/// once, before the step. Jolt clears accumulated forces after every step, so
/// this has to land between registration and JoltStepSystem.
///
/// Push fields stay MonoBehaviours, like the arena walls. Any number of them
/// may be in play at once; a body inside several simply accumulates force from
/// each.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(JoltRegistrationSystem))]
[UpdateBefore(typeof(JoltStepSystem))]
public partial class JoltPushSystem : SystemBase
{
    private JoltWorldSystem _worldSystem;

    private readonly List<PushField> _pushFields = new();

    private JoltBodyHandle[] _overlapBuffer;

    protected override void OnCreate()
    {
        _worldSystem = World.GetOrCreateSystemManaged<JoltWorldSystem>();
        _overlapBuffer = new JoltBodyHandle[JoltWorldSystem.MaximumWorldBodies];

        EventBus<PushFieldSpawnedEvent>.Subscribe(OnPushFieldSpawned);
    }

    protected override void OnDestroy()
    {
        EventBus<PushFieldSpawnedEvent>.Unsubscribe(OnPushFieldSpawned);
    }

    protected override void OnUpdate()
    {
        JoltWorld jolt = _worldSystem.Jolt;
        if (jolt == null || !jolt.IsValid) return;

        for (int i = _pushFields.Count - 1; i >= 0; i--)
        {
            PushField field = _pushFields[i];

            // Destroyed fields drop out here rather than on the destroyed
            // event, which carries no reference and so cannot say which one
            // went.
            if (!field)
            {
                _pushFields.RemoveAt(i);
                continue;
            }

            if (!field.isActiveAndEnabled) continue;

            ApplyField(jolt, field);
        }
    }

    private void ApplyField(JoltWorld jolt, PushField field)
    {
        field.GetColliderBox(out Vector3 center, out Vector3 extents, out Quaternion rot);
        int overlapping = jolt.OverlapBox(center, extents, _overlapBuffer, rot);

        for (int i = 0; i < overlapping; i++)
        {
            JoltBodyHandle body = _overlapBuffer[i];

            // Positions come from the snapshot rather than the world, so the
            // push stage adds no read of its own.
            if (_worldSystem.TryGetState(body, out JoltBodyState state))
            {
                jolt.AddForce(body, field.CalculatePushFrom(state.Position));
            }
        }
    }

    private void OnPushFieldSpawned(PushFieldSpawnedEvent e)
    {
        Register(e.pushField);
    }

    private void Register(PushField field)
    {
        if (!field || _pushFields.Contains(field)) return;

        _pushFields.Add(field);
    }
}
