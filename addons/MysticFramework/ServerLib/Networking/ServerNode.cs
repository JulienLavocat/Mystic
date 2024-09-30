using Godot;

namespace MysticFramework.ServerLib.Networking;

public partial class ServerNode : Node
{
	public override void _Ready()
	{
		Server.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		Server.Tick(delta);
	}

	public override void _ExitTree()
	{
		Server.Stop();
	}
}