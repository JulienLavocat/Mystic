using System.Collections.Generic;
using System.Linq;
using Godot;
using ImGuiNET;
using LiteNetLib;
using Mystic.Shared.Packets;

namespace Mystic.Client.Networking;

public partial class PlayerMovementSync : NetworkNode
{
    private readonly List<InputData> _playerInputs = [];

    private CharacterBody3D _player;

    public override void _Ready()
    {
        _player = GetParent<CharacterBody3D>();
    }

    public override void _Process(double delta)
    {
        DisplayDebugInfo();
    }

    protected override void OnProcessTick(int currentTick, int currentRemoteTick)
    {
        if (!NetworkReady) return;

        var data = GeneratePlayerInput(currentRemoteTick);

        _playerInputs.Add(data);
        SendInputs(currentRemoteTick);
    }

    private void SendInputs(int currentTick)
    {
        var packet = new PlayerInputPacket
        {
            Tick = currentTick,
            Inputs = _playerInputs.Select(i => i.Input).ToArray()
        };

        SendToServer(packet, DeliveryMethod.Unreliable);
    }

    private InputData GeneratePlayerInput(int tick)
    {
        byte keys = 0;

        if (Input.IsActionPressed("right")) keys |= (byte)InputFlags.Right;
        if (Input.IsActionPressed("left")) keys |= (byte)InputFlags.Left;
        if (Input.IsActionPressed("forward")) keys |= (byte)InputFlags.Forward;
        if (Input.IsActionPressed("backward")) keys |= (byte)InputFlags.Backward;
        if (Input.IsActionPressed("space")) keys |= (byte)InputFlags.Space;
        if (Input.IsActionPressed("shift")) keys |= (byte)InputFlags.Shift;

        var input = new PlayerInput
        {
            Keys = keys,
            LateralLookAngle = _player.Rotation.Y
        };

        return new InputData
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
            $"Position ({_player.GlobalPosition.X:0.00}, {_player.GlobalPosition.Y:0.00}, {_player.GlobalPosition.Z:0.00})");
        ImGui.Text($"Velocity ({_player.Velocity.X:0.00}, {_player.Velocity.Y:0.00}, {_player.Velocity.Z:0.00})");
        // ImGui.Text($"Redundant Inputs {_userInputs.Count}");
        // ImGui.Text($"Last Stamp Rec. {_lastStampReceived}");
        // ImGui.Text($"Misspredictions {_misspredictionCounter}");
        // ImGui.Text($"Saved Local States {_userInputs.Count}");
        // ImGui.Checkbox("Automove?", ref _autoMoveEnabled);
        ImGui.End();
    }

    private class InputData
    {
        public PlayerInput Input;
        public Vector3 Position;
        public int Tick;
    }
}