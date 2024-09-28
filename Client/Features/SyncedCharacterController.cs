using System.Collections.Generic;
using Godot;
using ImGuiNET;
using LiteNetLib;
using Mystic.Client.Nodes;
using Mystic.Shared.Packets;

namespace Mystic.Client.Features;

[GlobalClass]
public abstract partial class SyncedCharacterController : ClientNetworkNode
{
	private readonly List<LocalInputData> _userInputs = [];

	[Export] private CharacterBody3D _characterBody;

	private uint _lastStampReceived;
	private int _missPredictionCounter;

	protected abstract Vector3 CalculateVelocity(CharacterBody3D body, UserInput packet);
	protected abstract bool CalculateDeviation(EntitySnapshot incomingState, Vector3 savedState);
	protected abstract void HandleReconciliation(CharacterBody3D body, EntitySnapshot incomingState);
	protected abstract UserInput CaptureCurrentInput(CharacterBody3D body);

	public override void _Ready()
	{
		Networking.Client.RegisterNestedType<UserInput>();
		SubscribeToEntitySnapshot(ProcessEntitySnapshot);
		base._Ready();
	}

	public override void _Process(double delta)
	{
		// DisplayDebugInfo();
	}

	protected override void OnProcessTick(int currentTick, int currentRemoteTick)
	{
		var localInputData = GenerateUserInput(currentRemoteTick);

		_userInputs.Add(localInputData);
		SendInputs(currentRemoteTick);
		AdvancePhysics(localInputData);
		localInputData.Position = _characterBody.Position;
	}


	private void SendInputs(int currentTick)
	{
		var packet = new CharacterInputPacket
		{
			Tick = currentTick,
			Inputs = new UserInput[_userInputs.Count]
		};

		for (var i = 0; i < _userInputs.Count; i++) packet.Inputs[i] = _userInputs[i].Input;

		Networking.Client.Send(packet, DeliveryMethod.Unreliable);
	}

	private void AdvancePhysics(LocalInputData localInputData)
	{
		_characterBody.Velocity = CalculateVelocity(_characterBody, localInputData.Input);
		_characterBody.MoveAndSlide();
	}

	private void ProcessEntitySnapshot(EntitySnapshot snapshot, uint incomingSnapshotTick)
	{
		if (!NetworkReady) return;

		// We only process the most recent snapshot
		if (incomingSnapshotTick > _lastStampReceived)
			_lastStampReceived = incomingSnapshotTick;
		else return;

		_userInputs.RemoveAll(input => input.Tick < incomingSnapshotTick);
		var savedState = PopSavedPositionForTick(incomingSnapshotTick);

		if (savedState == Vector3.Inf)
		{
			GD.PrintErr($"No local state saved for ticks {incomingSnapshotTick}");
			return;
		}

		var deviation = CalculateDeviation(snapshot, savedState);
		if (!deviation)
			return; // If the current position is too far from the server one, we need to reconcile with what the server sent

		HandleReconciliation(_characterBody, snapshot);
		foreach (var inputData in _userInputs)
		{
			_characterBody.Velocity = CalculateVelocity(_characterBody, inputData.Input);

			// THIS IS IMPORTANT DO NOT REMOVE, source: https://github.com/grazianobolla/godot-monke-net/issues/8
			_characterBody.Velocity *= (float)GetPhysicsProcessDeltaTime() / (float)GetProcessDeltaTime();
			_characterBody.MoveAndSlide();
			_characterBody.Velocity /= (float)GetPhysicsProcessDeltaTime() / (float)GetProcessDeltaTime();

			inputData.Position =
				_characterBody
					.GlobalPosition; // Since all states after a miss prediction are wrong we need to update them
		}

		_missPredictionCounter++;
	}

	private Vector3 PopSavedPositionForTick(uint tick)
	{
		for (var i = 0; i < _userInputs.Count; i++)
			if (_userInputs[i].Tick == tick)
			{
				var position = _userInputs[i].Position;
				_userInputs.RemoveAt(i);
				return position;
			}

		return Vector3.Inf;
	}

	private LocalInputData GenerateUserInput(int tick)
	{
		var input = CaptureCurrentInput(_characterBody);

		return new LocalInputData
		{
			Input = input,
			Position = Vector3.Inf,
			Tick = tick
		};
	}

	private void DisplayDebugInfo()
	{
		if (!ImGui.Begin("Player Data")) return;

		ImGui.Text(
			$"Position: ({_characterBody.GlobalPosition.X:0.00}, {_characterBody.GlobalPosition.Y:0.00}, {_characterBody.GlobalPosition.Z:0.00})");
		ImGui.Text(
			$"Velocity: ({_characterBody.Velocity.X:0.00}, {_characterBody.Velocity.Y:0.00}, {_characterBody.Velocity.Z:0.00})");
		ImGui.Text($"Redundant Inputs: {_userInputs.Count}");
		ImGui.Text($"Last Stamp Rec.: {_lastStampReceived}");
		ImGui.Text($"Miss predictions :{_missPredictionCounter}");
		ImGui.Text($"Saved Local States: {_userInputs.Count}");
		ImGui.End();
	}

	private class LocalInputData
	{
		public UserInput Input;
		public Vector3 Position;
		public int Tick;
	}
}