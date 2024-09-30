using Godot;
using MysticFramework.ServerLib.Networking;
using MysticFramework.Shared.Packets;

namespace MysticFramework.ServerLib.Features.PlayerMovement;

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