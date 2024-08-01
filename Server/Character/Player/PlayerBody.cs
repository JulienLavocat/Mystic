using Godot;
using Mystic.Shared.Packets;

namespace Mystic.Server.Character.Player;

public partial class PlayerBody : CharacterBody3D
{
    public int Id { get; set; }
    public float LateralLookAngle { get; set; }

    public EntitySnapshot GetSnapshot()
    {
        return new EntitySnapshot
        {
            Id = Id,
            LateralLookAngle = LateralLookAngle,
            Position = Position,
            Velocity = Velocity
        };
    }
}