using Game.Jolt;
using UnityEngine;

public class JoltCapsule : JoltBody
{
    protected override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return JoltBodyDesc.Capsule(0.4f, 0.4f, transform.position, Quaternion.identity, JoltMotion.Dynamic);
    }
    
}
