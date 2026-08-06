using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public partial struct DebrisSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DebrisData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        DebrisJob job = new DebrisJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        job.ScheduleParallel();
    }

    public partial struct DebrisJob : IJobEntity
    {
        public float DeltaTime;
        private void Execute(ref DebrisData debris, ref LocalTransform transform)
        {
            
        }
    }
}
