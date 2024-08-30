using System;
using LiteNetLib;

namespace Mystic.Server.Networking;

public static class ServerImpl
{
    
    public static void SubscribeToTicks(Action<int> handler) {}
    
    public static void SendToClient<T>(int peerId, T packet, DeliveryMethod method, byte channel = 0)
        where T : class, new()
    {
        Server.Host.SendToClient(peerId, packet, method, channel);
    }

    public static void SubscribeToAllPackets<T>(Action<T, ActorNode> action) where T : class, new()
    {
    }

    public static void SubscribeToActorPackets<T>(ActorId id, Action<T> action) where T : class, new()
    {
    }
}