namespace Mystic.Shared.Packets;

public class PlayerInputPacket
{
    public PlayerInput[] Inputs;
    public int Tick;
}

public class PlayerInput
{
    public byte Keys { get; set; }
    public float LateralLookAngle { get; set; }
}

public enum InputFlags
{
    Forward = 0b_0000_0001,
    Backward = 0b_0000_0010,
    Left = 0b_0000_0100,
    Right = 0b_0000_1000,
    Space = 0b_0001_0000,
    Shift = 0b_0010_0000
}