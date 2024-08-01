using System;
using Godot;
using LiteNetLib;

namespace Mystic.Server.Networking;

public abstract partial class ActorNode : Node3D
{
    public ActorNode()
    {
        Id = Actors.NextId();
        Actors.Add(Id, this);
    }

    public ActorId Id { get; private set; }

    protected virtual void OnProcessTick(int currentTick)
    {
    }

    public override void _ExitTree()
    {
        Actors.Remove(Id);
    }

    public override void _Ready()
    {
        Server.Instance.ServerTick += OnProcessTick;
    }

    protected static void SendToClient<T>(int peerId, T packet, DeliveryMethod method, byte channel = 0)
        where T : class, new()
    {
        Server.Host.SendToClient(peerId, packet, method, channel);
    }

    protected static void SubscribeToPackets<T>(Action<T, ActorNode> action) where T : class, new()
    {
        Server.Host.SubscribeToPacket(action);
    }

    protected void SubscribeToActorPackets<T>(Action<T> action) where T : class, new()
    {
        Server.Host.SubscribeToPacket(Id, action);
    }
}