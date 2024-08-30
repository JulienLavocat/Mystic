using System.Collections.Generic;
using System.Threading;
using Godot;

namespace Mystic.GamerServer;

public static class Actors
{
    private static readonly Dictionary<uint, Node> IdsToActors = new();

    private static uint _nextId = 10;

    public static uint NextId()
    {
        return Interlocked.Increment(ref _nextId);
    }

    public static void Add(uint id, Node actor)
    {
        IdsToActors.Add(id, actor);
    }

    public static bool TryGet<T>(uint id, out T actor) where T : Node
    {
        var result = IdsToActors.TryGetValue(id, out var foundActor);

        actor = (T)foundActor;
        return result;
    }

    public static bool Remove(uint id)
    {
        return IdsToActors.Remove(id);
    }
}