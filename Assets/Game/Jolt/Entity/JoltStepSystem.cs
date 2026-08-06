using Game.Jolt;
using Unity.Entities;

/// <summary>
/// Advances the entity path's Jolt world, once per fixed tick. The equivalent
/// of the Step call in JoltEngine.FixedUpdate.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class JoltStepSystem : SystemBase
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

        // Fixed by virtue of the group this runs in. Stepping on a variable
        // timestep gives up determinism.
        jolt.Step(SystemAPI.Time.DeltaTime);
    }
}
