using Game.Jolt;
using UnityEngine;

public class JoltBox : JoltBody
{
    
    protected override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return JoltBodyDesc.Box(Vector3.one * 0.5f, transform.position, Quaternion.identity, JoltMotion.Dynamic);
    }
}
