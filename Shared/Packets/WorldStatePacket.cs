using Godot;

namespace Mystic.Shared.Packets;

public class WorldStatePacket
{
    public EntitySnapshot[] Entities;
    public int Tick;
}

public class EntitySnapshot
{
    public uint Id;
    public float LateralLookAngle;
    public Vector3 Position;
    public Vector3 Velocity;
}