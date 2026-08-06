using Game.Jolt;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Keeps the Jolt world in step with the entities that want bodies in it. Adds
/// a body for every entity that has not been registered yet, and removes the
/// body of every entity that has since been destroyed.
///
/// This replaces the DebrisSpawnedEvent / DebrisDestroyedEvent round trip and
/// the handle dictionary the GameObject path keeps in JoltEngine.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(JoltStepSystem))]
public partial class JoltRegistrationSystem : SystemBase
{
    private JoltWorldSystem _worldSystem;
    private EntityQuery _orphanedBodies;

    protected override void OnCreate()
    {
        _worldSystem = World.GetOrCreateSystemManaged<JoltWorldSystem>();

        // Once the entity is destroyed the cleanup component is all that is
        // left of it, so its absence of a desc is what marks it as orphaned.
        _orphanedBodies = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(JoltBodyCleanup) },
            None = new ComponentType[] { typeof(JoltBodyDescData) }
        });
    }

    protected override void OnUpdate()
    {
        JoltWorld jolt = _worldSystem.Jolt;
        if (jolt == null || !jolt.IsValid) return;

        // Free slots before claiming new ones, so a frame that destroys as
        // many bodies as it spawns cannot run the world out of capacity.
        RemoveOrphanedBodies(jolt);
        AddUnregisteredBodies(jolt);
    }

    private void RemoveOrphanedBodies(JoltWorld jolt)
    {
        if (_orphanedBodies.IsEmpty) return;

        var cleanups = _orphanedBodies.ToComponentDataArray<JoltBodyCleanup>(Allocator.Temp);
        for (int i = 0; i < cleanups.Length; i++)
        {
            if (cleanups[i].Handle.IsValid) jolt.RemoveBody(cleanups[i].Handle);
        }
        cleanups.Dispose();

        EntityManager.RemoveComponent<JoltBodyCleanup>(_orphanedBodies);
    }

    private void AddUnregisteredBodies(JoltWorld jolt)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (descData, transform, bodyRef, entity) in
                 SystemAPI.Query<RefRO<JoltBodyDescData>, RefRO<LocalTransform>, RefRW<JoltBodyRef>>()
                     .WithNone<JoltBodyCleanup>()
                     .WithEntityAccess())
        {
            // The baked desc carries shape, motion and material only. Where the
            // body actually goes is whatever the instance's transform says,
            // which for spawned debris is set the moment it is instantiated.
            JoltBodyDesc desc = descData.ValueRO.Desc;
            desc.Position = (Vector3)transform.ValueRO.Position;
            desc.Rotation = (Quaternion)transform.ValueRO.Rotation;

            JoltBodyHandle handle = jolt.AddBody(desc);
            bodyRef.ValueRW.Handle = handle;

            // Tag it even when the world rejected the body, otherwise a full
            // world means retrying every rejected entity on every tick. The
            // handle stays invalid, and writeback skips it.
            ecb.AddComponent(entity, new JoltBodyCleanup { Handle = handle });
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
