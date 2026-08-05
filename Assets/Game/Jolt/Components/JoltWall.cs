
using Game.Jolt;
using UnityEngine;

public class JoltWall : JoltBody
{
    protected override JoltBodyDesc ConstructJoltBodyDesc()
    {
        var ps = transform.position;
        var sc = transform.localScale * 0.5f;
        return JoltBodyDesc.Box(sc, ps, Quaternion.identity, JoltMotion.Static);
    }

    public override void StateUpdate(JoltBodyState newState)
    {
        
    }
}