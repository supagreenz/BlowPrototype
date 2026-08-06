using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Pours out the startup batch of debris a few per frame, rather than
/// instantiating the lot in one tick, so the spawn cost is spread and the
/// bodies reach Jolt in registerable batches.
/// </summary>
[BurstCompile]
public partial struct DebrisSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DebrisSpawner>();
        state.RequireForUpdate<DebrisSpawnCounter>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<DebrisSpawner>(out var spawnerEntity))
            return;

        var spawner = SystemAPI.GetComponent<DebrisSpawner>(spawnerEntity);
        var counter = SystemAPI.GetComponentRW<DebrisSpawnCounter>(spawnerEntity);

        int remaining = counter.ValueRO.Remaining;
        if (remaining <= 0) return;

        int batch = math.min(spawner.PerFrame, remaining);

        // Start from the prefab's own transform and move it, rather than
        // building one from scratch: FromPosition would reset Scale to 1 and
        // the debris prefabs are authored at 0.5, which their Jolt shapes are
        // sized to match.
        var spawnTransform = SystemAPI.GetComponent<LocalTransform>(spawner.DebrisPrefab);
        spawnTransform.Position = float3.zero;

        var instances = state.EntityManager.Instantiate(spawner.DebrisPrefab, batch, Allocator.Temp);
        for (int i = 0; i < instances.Length; i++)
        {
            state.EntityManager.SetComponentData(instances[i], spawnTransform);
        }
        instances.Dispose();

        counter.ValueRW.Remaining = remaining - batch;
    }
}
