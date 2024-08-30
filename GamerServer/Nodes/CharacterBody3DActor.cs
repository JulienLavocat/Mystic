using Godot;
using Mystic.GamerServer.Networking;

namespace Mystic.GamerServer.Nodes;

public abstract partial class CharacterBody3DActor : CharacterBody3D
{
    public CharacterBody3DActor()
    {
        Id = Actors.NextId();
        Actors.Add(Id, this);
    }

    public uint Id { get; }

    protected virtual void _Tick(double delta)
    {
    }

    public override void _ExitTree()
    {
        Server.OnTick -= _Tick;
        Actors.Remove(Id);
    }

    public override void _Ready()
    {
        Server.OnTick += _Tick;
    }
}