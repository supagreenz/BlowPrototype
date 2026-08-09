using Game.Jolt;

public struct DebrisSpawnedEvent
{
    public JoltBodyDesc joltBodyDesc;
    public JoltBody bodyRef;
}

public struct DebrisDestroyedEvent
{
    
}

public struct BlowerRegisterEvent
{
    public Blower Blower;
}

public struct BlowerUnregisterEvent
{
    public Blower Blower;
}