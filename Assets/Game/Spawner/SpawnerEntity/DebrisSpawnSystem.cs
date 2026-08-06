using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

[BurstCompile]
public partial struct DebrisSpawnSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DebrisSpawner>();
        state.RequireForUpdate<DebrisSpawnFlag>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<DebrisSpawner>(out var spawnerEntity))
            return;

        var spawner = SystemAPI.GetComponent<DebrisSpawner>(spawnerEntity);
        var flag = SystemAPI.GetSingleton<DebrisSpawnFlag>();

        if (!flag.CanSpawnNow) return;
        
        var origin = float3.zero;

        var e = state.EntityManager.Instantiate(spawner.DebrisPrefab);
        state.EntityManager.SetComponentData(e, LocalTransform.FromPosition(origin));
        SystemAPI.SetComponent(spawnerEntity, spawner);
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class DebrisSpawnInputReader : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<DebrisSpawnFlag>();
    }
    
    protected override void OnUpdate()
    {
        var flag = SystemAPI.GetSingleton<DebrisSpawnFlag>();
        flag.CanSpawnNow = Keyboard.current.pKey.isPressed;
        SystemAPI.SetSingleton(flag);
    }
}