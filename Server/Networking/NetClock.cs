using Godot;
using ImGuiNET;
using LiteNetLib;
using Mystic.Shared.Packets;

namespace Mystic.Server.Networking;

public partial class NetClock : ActorNode
{
    [Signal]
    public delegate void NetworkProcessTickEventHandler(double delta);

    private int _currentTick;
    private double _nextTickTimer;

    [Export] private int _tickRate = 60;

    public override void _Ready()
    {
        SubscribeToPackets<SyncClockPacket>(HandleSyncRequest);
    }

    public override void _Process(double delta)
    {
        SendNetworkTickEvent(delta);
    }

    public int ProcessTick()
    {
        _currentTick++;
        return _currentTick;
    }

    public int GetNetworkTickRate()
    {
        return _tickRate;
    }

    private void SendNetworkTickEvent(double delta)
    {
        _nextTickTimer += delta;
        if (_nextTickTimer < 1.0 / _tickRate)
            return;

        EmitSignal(SignalName.NetworkProcessTick, _nextTickTimer);
        _nextTickTimer = 0;
    }

    private void HandleSyncRequest(SyncClockPacket packet, ActorNode actor)
    {
        packet.ServerTick = _currentTick;
        SendToClient(Actors.GetPeerId(actor.Id), packet, DeliveryMethod.Unreliable);
    }

    public void DrawDebugInfo()
    {
        if (!ImGui.CollapsingHeader("Clock")) return;
        ImGui.Text($"Network Tick rate {GetNetworkTickRate()}hz");
        ImGui.Text($"Current Tick {_currentTick}");
    }
}