using Game.Jolt;
using Unity.Entities;

/// <summary>
/// The body this entity wants in the Jolt world, as baked from its authoring
/// component. Position and Rotation are ignored: JoltRegistrationSystem takes
/// those from the entity's LocalTransform, since a prefab's baked transform
/// says nothing about where an instance ends up.
/// </summary>
public struct JoltBodyDescData : IComponentData
{
    public JoltBodyDesc Desc;
}

/// <summary>
/// This entity's body in the Jolt world. Baked in as
/// <see cref="JoltBodyHandle.Invalid"/> so that registering a body later is a
/// value write rather than a structural change.
///
/// Bake the invalid handle explicitly. A default JoltBodyHandle is raw zero,
/// which reads as slot 0 generation 0 — a plausible enough handle that
/// writeback would snap the entity onto whichever body owns that slot.
/// </summary>
public struct JoltBodyRef : IComponentData
{
    public JoltBodyHandle Handle;
}

/// <summary>
/// Outlives the entity so JoltRegistrationSystem can hand the body back to
/// the world after the entity is destroyed. Without it the handle disappears
/// with the entity and the native body leaks.
/// </summary>
public struct JoltBodyCleanup : ICleanupComponentData
{
    public JoltBodyHandle Handle;
}

/// <summary>
/// A body Jolt will never move, so writeback can skip it. The counterpart of
/// JoltWall overriding StateUpdate to do nothing.
/// </summary>
public struct JoltStaticBody : IComponentData
{
}
