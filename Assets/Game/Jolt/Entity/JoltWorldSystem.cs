using Game.Jolt;
using Unity.Entities;

/// <summary>
/// Owns the one Jolt world everything simulates in, and nothing else. Never
/// updates: it exists for the create and dispose ends of the world's life.
///
/// There is deliberately a single world rather than one per path. Entity
/// bodies and the GameObject bodies that have not been migrated yet — the
/// arena walls above all — have to collide with each other, and bodies in
/// separate worlds cannot see each other at all.
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

    public JoltWorld Jolt { get; private set; }

    protected override void OnCreate()
    {
        Jolt = new JoltWorld(MaximumWorldBodies * 3);
        Active = Jolt;
        Enabled = false;
    }

    protected override void OnDestroy()
    {
        if (ReferenceEquals(Active, Jolt)) Active = null;

        Jolt?.Dispose();
        Jolt = null;
    }

    protected override void OnUpdate()
    {
    }
}
