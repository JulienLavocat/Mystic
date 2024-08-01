using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LiteNetLib.Utils;

namespace Mystic.Server.Networking;

public class PacketsProcessor
{
    private readonly Dictionary<ulong, Dictionary<ActorId, List<SubscribeDelegate>>> _actorsCallbacks = new();

    private readonly Dictionary<ulong, List<SubscribeDelegate>> _globalCallbacks = new();
    private readonly NetSerializer _serializer = new();

    private ulong GetHash<T>()
    {
        return HashCache<T>.Id;
    }

    private void WriteHash<T>(NetDataWriter writer)
    {
        writer.Put(GetHash<T>());
    }

    /*
     * Subscribe to all packets of type T.
     * It should only be used for objects that lives forever as global subscriptions aren't removed
     */
    public void Subscribe<T>(Action<T, ActorNode> onReceived) where T : class, new()
    {
        _serializer.Register<T>();
        var packetId = GetHash<T>();

        SubscribeDelegate subscribeDelegate = (reader, peerId) =>
        {
            var reference = new T();
            _serializer.Deserialize(reader, reference);
            if (!Actors.TryGetByPeerId(peerId, out var actor)) return;
            onReceived(reference, actor);
        };

        if (_globalCallbacks.TryGetValue(packetId, out var subscriptions))
            subscriptions.Add(subscribeDelegate);
        else _globalCallbacks[packetId] = [subscribeDelegate];
    }

    /*
     * Subscribe to any packets of type T coming from an Actor
     */
    public void Subscribe<T>(uint id, Action<T> onReceived) where T : class, new()
    {
        _serializer.Register<T>();
        var packetId = GetHash<T>();

        SubscribeDelegate subscribeDelegate = (reader, userData) =>
        {
            var reference = new T();
            _serializer.Deserialize(reader, reference);
            onReceived(reference);
        };

        if (_actorsCallbacks.TryGetValue(packetId, out var actorCallbacks))
            if (actorCallbacks.TryGetValue(id, out var callbacks))
                callbacks.Add(subscribeDelegate);
            else
                actorCallbacks[id] = [subscribeDelegate];
        else
            _actorsCallbacks[packetId] = new Dictionary<uint, List<SubscribeDelegate>>
                { { id, [subscribeDelegate] } };
    }

    public void UnsubscribeActor(uint id)
    {
        foreach (var kv in _actorsCallbacks)
            kv.Value.Remove(id);
    }

    public void Write<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties |
                                    DynamicallyAccessedMemberTypes.NonPublicProperties)]
        T>(
        NetDataWriter writer,
        T packet)
        where T : class, new()
    {
        WriteHash<T>(writer);
        _serializer.Serialize(writer, packet);
    }


    public void ReadAllPackets(NetDataReader reader, int peerId)
    {
        while (reader.AvailableBytes > 0)
        {
            var packetId = reader.GetULong();
            if (!_globalCallbacks.TryGetValue(packetId, out var globalCallbacks))
                throw new ParseException("Undefined packet in NetDataReader");

            foreach (var callback in globalCallbacks)
                callback(reader, peerId);
        }
    }

    private delegate void SubscribeDelegate(NetDataReader reader, int peerId);

    private static class HashCache<T>
    {
        public static readonly ulong Id;

        static HashCache()
        {
            var num1 = 14695981039346656037;
            foreach (ulong num2 in typeof(T).ToString())
                num1 = (num1 ^ num2) * 1099511628211UL;
            Id = num1;
        }
    }
}