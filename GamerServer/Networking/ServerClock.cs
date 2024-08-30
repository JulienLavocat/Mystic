using LiteNetLib;
using Mystic.GamerServer.Nodes;
using Mystic.Shared.Packets;

namespace Mystic.GamerServer.Networking;

public partial class ServerClock : ActorNode
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