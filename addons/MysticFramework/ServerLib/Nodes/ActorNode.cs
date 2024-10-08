using Godot;
using MysticFramework.ServerLib.Networking;

namespace MysticFramework.ServerLib.Nodes;

public abstract partial class ActorNode : Node3D
{
	public ActorNode()
	{
		Id = Actors.NextId();
		Actors.Add(Id, this);
	}

	public uint Id { get; }

	protected virtual void OnProcessTick(double delta)
	{
	}

	public override void _ExitTree()
	{
		Actors.Remove(Id);
	}

	public override void _Ready()
	{
		Server.OnTick += OnProcessTick;
	}
}