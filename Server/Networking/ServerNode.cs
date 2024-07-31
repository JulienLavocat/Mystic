using Godot;
using Mystic.Shared;

namespace Mystic.Server.Networking;

public abstract partial class ServerNode : Node
{
    protected virtual void OnProcessTick(int currentTick) { }

    public override void _Ready()
    {
        Server.Instance.ServerTick += OnProcessTick;
    }

    public void SendToClient<T>(int peerId, T packet, LiteNetLib.DeliveryMethod method, byte channel = 0) where T : class, new()
    {
        Server.Instance.SendToClient(peerId, packet, method, channel);
    }
}