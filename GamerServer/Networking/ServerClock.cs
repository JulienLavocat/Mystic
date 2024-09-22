using Godot;
using LiteNetLib;
using Mystic.Shared.Packets;

namespace Mystic.GamerServer.Networking;

public partial class ServerClock : Node
{
	public override void _Ready()
	{
		Server.Subscribe<SyncClockPacket>(HandleSyncRequest);
	}

	private void HandleSyncRequest(int client, SyncClockPacket packet)
	{
		packet.ServerTick = Server.CurrentTick;
		Server.Send(client, packet, DeliveryMethod.Unreliable);
	}
}