using Godot;
using Mystic.Shared.Packets;
using Mystic.Shared.Utils;

namespace Mystic.Shared;

public class PlayerMovement
{
	private const float MaxRunSpeed = 5;
	private const float MaxWalkSpeed = 2;
	private const float Gravity = 9.8f;
	private const float JumpVelocity = 2.0f;

	public static Vector3 CalculateVelocity(CharacterBody3D body, UserInput input)
	{
		var direction2D = InputToDirection(input.Keys);

		var isWalking = ReadInput(input.Keys, InputFlags.Shift);
		var isJumping = ReadInput(input.Keys, InputFlags.Space);
		var velocity = body.Velocity;

		var isOnFloor = body.IsOnFloor();
		var direction = new Vector3(direction2D.X, 0, direction2D.Y).Normalized();
		direction = direction.Rotated(Vector3.Up, input.LateralLookAngle);

		if (!direction.IsZeroApprox())
		{
			velocity.X = direction.X * (isWalking ? MaxWalkSpeed : MaxRunSpeed);
			velocity.Z = direction.Z * (isWalking ? MaxWalkSpeed : MaxRunSpeed);
		}
		else
		{
			velocity.X = 0;
			velocity.Z = 0;
		}

		if (!isOnFloor)
			velocity.Y -= Gravity * PhysicsUtils.DeltaTime;

		if (isJumping && isOnFloor)
			velocity.Y = JumpVelocity;

		return velocity;
	}

	public static Vector2 InputToDirection(byte input)
	{
		var direction = Vector2.Zero;

		if ((input & (byte)InputFlags.Right) > 0) direction.X += 1;
		if ((input & (byte)InputFlags.Left) > 0) direction.X -= 1;
		if ((input & (byte)InputFlags.Backward) > 0) direction.Y += 1;
		if ((input & (byte)InputFlags.Forward) > 0) direction.Y -= 1;

		return direction.Normalized();
	}

	public static bool ReadInput(byte input, InputFlags flag) => (input & (byte)flag) > 0;
}