using LiteNetLib;
using LiteNetLib.Utils;

namespace MysticFramework.ServerLib.Networking;

public class Client(int id, NetPeer peer)
{
	private readonly NetPacketProcessor _packetProcessor = new();
	private readonly NetDataWriter _writer = new();
	public readonly int Id = id;

	public void Send<T>(T packet, DeliveryMethod method, byte channel = 0) where T : class, new()
	{
		_writer.Reset();
		_packetProcessor.Write(_writer, packet);
		peer.Send(_writer, channel, method);
	}
}