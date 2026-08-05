using Game.Jolt;

public struct DebrisSpawnedEvent
{
    public JoltBodyDesc joltBodyDesc;
    public JoltBody bodyRef;
}

public struct DebrisDestroyedEvent
{
    
}

public struct PushFieldSpawnedEvent
{
    public PushField pushField;
}

public struct PushFieldDestroyedEvent
{
    
}