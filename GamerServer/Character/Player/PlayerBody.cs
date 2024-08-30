using Mystic.GamerServer.Nodes;
using Mystic.Shared.Packets;

namespace Mystic.GamerServer.Character.Player;

public partial class PlayerBody : CharacterBody3DActor
{
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