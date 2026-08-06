using Game.Jolt;
using UnityEngine;

/// <summary>
/// Base for the authoring components that give an entity a Jolt body, the
/// entity counterpart of JoltBody. Subclasses describe a shape; each carries
/// its own Baker, since a Baker is resolved against one concrete authoring
/// type.
/// </summary>
public abstract class JoltBodyAuthoring : MonoBehaviour
{
    /// <summary>
    /// Shape, motion and material for this body. Leave Position and Rotation
    /// alone: JoltRegistrationSystem overwrites them per instance from the
    /// entity's transform.
    /// </summary>
    public abstract JoltBodyDesc ConstructJoltBodyDesc();
}
