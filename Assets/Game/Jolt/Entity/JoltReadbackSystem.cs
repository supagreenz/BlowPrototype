using Unity.Entities;

/// <summary>
/// The pull stage: the tick's one and only read of the native world, into the
/// shared snapshot on JoltWorldSystem. Everything that wants to know where a
/// body ended up reads that snapshot, so the cost of the readback is paid once
/// however many bodies and consumers there are.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(JoltStepSystem))]
[UpdateBefore(typeof(JoltWritebackSystem))]
public partial class JoltReadbackSystem : SystemBase
{
    private JoltWorldSystem _worldSystem;

    protected override void OnCreate()
    {
        _worldSystem = World.GetOrCreateSystemManaged<JoltWorldSystem>();
    }

    protected override void OnUpdate()
    {
        _worldSystem.PullStates();
    }
}
