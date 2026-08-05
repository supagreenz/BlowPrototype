using Game.Jolt;
using UnityEngine;

public class JoltBall : JoltBody
{


    protected override JoltBodyDesc ConstructJoltBodyDesc()
    {
        return JoltBodyDesc.Sphere(0.25f, transform.position, JoltMotion.Dynamic);
    }
}
