using Godot;
using LiteNetLib;
using MysticFramework.Shared.Packets;

namespace MysticFramework.ServerLib.Networking;

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