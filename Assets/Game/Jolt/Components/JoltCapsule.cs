using Game.Jolt;
using UnityEngine;

public class JoltCapsule : JoltBody
{
    protected override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return JoltBodyDesc.Capsule(0.2f, 0.2f, transform.position, Quaternion.identity, JoltMotion.Dynamic);
    }
    
}
