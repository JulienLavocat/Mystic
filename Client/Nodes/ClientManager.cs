using Godot;
using Mystic.Client.Networking;

namespace Mystic.Client.Nodes;

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
		Networking.Client.Connect(_clock);
	}

	public override void _Process(double delta)
	{
		Networking.Client.Update();
	}

	public override void _PhysicsProcess(double delta)
	{
		Networking.Client.Tick();
	}

	public override void _ExitTree()
	{
		Networking.Client.Disconnect();
	}
}