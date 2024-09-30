using Godot;
using MysticFramework.ClientLib.Networking;

namespace MysticFramework.ClientLib.Nodes;

[GlobalClass]
public partial class ClientManager : Node
{
	[Export] private NetClock _clock;

	public static ClientManager Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		Client.Connect(_clock);
	}

	public override void _Process(double delta)
	{
		Client.Update();
	}

	public override void _PhysicsProcess(double delta)
	{
		Client.Tick();
	}

	public override void _ExitTree()
	{
		Client.Disconnect();
	}
}