using Godot;
using Mystic.GamerServer.Networking;
using Mystic.Shared.Packets;

namespace Mystic.GamerServer.Features.PlayerMovement;

[GlobalClass]
public partial class PlayerInputsHandler : Node
{
	public override void _Ready()
	{
		Server.RegisterNestedType<PlayerInput>();
		Server.Subscribe<PlayerInputPacket>(HandlePlayerInput);
	}

	private static void HandlePlayerInput(int peerId, PlayerInputPacket packet)
	{
		GD.Print($"{packet.Tick}");
	}
}