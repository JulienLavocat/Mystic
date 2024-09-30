namespace MysticFramework.Shared.Packets;

public class SyncClockPacket
{
	public int ClientTime { get; set; }
	public uint ServerTick { get; set; }
}