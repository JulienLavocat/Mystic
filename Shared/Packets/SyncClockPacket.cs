namespace Mystic.Shared.Packets;

public class SyncClockPacket
{
    public int ClientTime { get; set; }
    public int ServerTick { get; set; }
}