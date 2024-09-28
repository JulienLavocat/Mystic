using Godot;
using Mystic.GamerServer.Networking;
using Mystic.Shared.Packets;

namespace Mystic.GamerServer.Features.PlayerMovement;

[GlobalClass]
public partial class PlayerInputsHandler : Node
{
	public override void _Ready()
	{
		Server.RegisterNestedType<UserInput>();

		Server.Subscribe<CharacterInputPacket>(HandlePlayerInput);
	}

	private static void HandlePlayerInput(int peerId, CharacterInputPacket packet)
	{
		GD.Print($"{packet.Tick}");
	}
}