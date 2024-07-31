using Godot;

namespace Mystic.Shared.Packets;

public class WorldStatePacket
{
    public EntitySnapshot[] Entities;
    public int Tick;
}

public class EntitySnapshot
{
    public int Id;
    public Vector3 Position;
    public Vector3 Velocity;
    public float LateralLookAngle;
}