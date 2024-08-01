using System.Collections.Generic;
using System.Threading;
using Mystic.Server.Networking;

namespace Mystic.Server;

public static class Actors
{
    private static readonly Dictionary<ActorId, ActorNode> IdToActors = new();
    private static readonly Dictionary<int, ActorNode> ActorsFromPeerId = new();
    private static readonly Dictionary<ActorId, int> PeerIdFromActorId = new();

    private static ActorId _nextId = 10;

    public static int GetPeerId(ActorId id)
    {
        return PeerIdFromActorId[id];
    }

    public static ActorId NextId()
    {
        return Interlocked.Increment(ref _nextId);
    }

    public static void Add(ActorId id, ActorNode actor)
    {
        IdToActors.Add(id, actor);
    }

    public static void LinkToPeer(ActorId id, int peerId, ActorNode actor)
    {
        ActorsFromPeerId.Add(peerId, actor);
        PeerIdFromActorId.Add(id, peerId);
    }

    public static bool TryGet(ActorId id, out ActorNode actor)
    {
        var result = IdToActors.TryGetValue(id, out var foundActor);

        actor = foundActor;
        return result;
    }

    public static bool TryGetByPeerId(int id, out ActorNode actor)
    {
        var result = ActorsFromPeerId.TryGetValue(id, out var foundActor);

        actor = foundActor;
        return result;
    }

    public static bool Remove(ActorId id)
    {
        return IdToActors.Remove(id);
    }

    public static bool UnlinkActor(ActorId id, int peerId)
    {
        PeerIdFromActorId.Remove(id);
        return ActorsFromPeerId.Remove(peerId);
    }
}