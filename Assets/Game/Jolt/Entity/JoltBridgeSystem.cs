using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class JoltBridgeSystem : SystemBase
{
    protected override void OnCreate()
    {
        if (!SystemAPI.TryGetSingletonEntity<JoltState>(out _))
            EntityManager.CreateEntity(typeof(JoltState));
    }

    protected override void OnUpdate()
    {
        var engine = Engine.Instance;
        if (engine == null) return;

        SystemAPI.SetSingleton(new EngineState
        {
            TimeScale = engine.TimeScale,
            Gravity   = engine.Gravity,
            Phase     = engine.CurrentPhase
        });
    }
}
