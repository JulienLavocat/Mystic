using LiteNetLib.Utils;

namespace Mystic.Shared.Packets;

public class CharacterInputPacket
{
	public UserInput[] Inputs { get; set; }
	public int Tick { get; set; }
}

public struct UserInput : INetSerializable
{
	public byte Keys { get; set; }
	public float LateralLookAngle { get; set; }

	public void Serialize(NetDataWriter writer)
	{
		writer.Put(Keys);
		writer.Put(LateralLookAngle);
	}

	public void Deserialize(NetDataReader reader)
	{
		Keys = reader.GetByte();
		LateralLookAngle = reader.GetFloat();
	}
}

public enum InputFlags
{
	Forward = 0b_0000_0001,
	Backward = 0b_0000_0010,
	Left = 0b_0000_0100,
	Right = 0b_0000_1000,
	Space = 0b_0001_0000,
	Shift = 0b_0010_0000,
	Control = 0b_0100_0000
}