using Godot;
using MysticFramework.ClientLib.Features;
using MysticFramework.Shared;
using MysticFramework.Shared.Packets;

namespace MysticFramework.ClientLib.Nodes;

public partial class ClientCharacterController : SyncedCharacterController
{
	// How much we allow our local prediction to deviate from the servers authoritative state this should always be as close to 0 as possible
	[Export] private float _maxDeviationAllowedThousands = 0.1f;

	protected override Vector3 CalculateVelocity(CharacterBody3D body, UserInput input) =>
		PlayerMovement.CalculateVelocity(body, input);

	protected override bool CalculateDeviation(EntitySnapshot incomingState, Vector3 savedState) =>
		(incomingState.Position - savedState).LengthSquared() > _maxDeviationAllowedThousands;

	protected override void HandleReconciliation(CharacterBody3D body, EntitySnapshot incomingState)
	{
		body.Position = incomingState.Position;
		body.Velocity = incomingState.Velocity;
	}

	protected override UserInput CaptureCurrentInput(CharacterBody3D body)
	{
		byte keys = 0;

		// TODO: Use an array of inputs defined as an export, each string will results in a new bit offset. For now only 2 bytes will be supported (16 inputs at the same time)
		if (Input.IsActionPressed("Right")) keys |= (byte)InputFlags.Right;
		if (Input.IsActionPressed("Left")) keys |= (byte)InputFlags.Left;
		if (Input.IsActionPressed("Forwards")) keys |= (byte)InputFlags.Forward;
		if (Input.IsActionPressed("Backwards")) keys |= (byte)InputFlags.Backward;
		if (Input.IsActionPressed("Jump")) keys |= (byte)InputFlags.Space;
		if (Input.IsActionPressed("Run")) keys |= (byte)InputFlags.Shift;
		if (Input.IsActionPressed("Control")) keys |= (byte)InputFlags.Control;

		return new UserInput
		{
			Keys = keys,
			LateralLookAngle = body.Rotation.Y
		};
	}
}