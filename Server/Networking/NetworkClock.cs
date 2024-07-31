using Godot;
using ImGuiNET;
using LiteNetLib;
using Mystic.Shared.Packets;

namespace Mystic.Server.Networking;

public partial class NetworkClock : ServerNode
{
	[Signal] public delegate void NetworkProcessTickEventHandler(double delta); 
	
	[Export]
	private int _tickRate = 60;
	private double _netTickTimer;
	private int _currentTick;

	public override void _Ready()
	{
		Server.Instance.SubscribeToPacket<SyncClockPacket>(HandleSyncRequest);
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
		_netTickTimer += delta;
		if (_netTickTimer < 1.0 / _tickRate)
			return;

		EmitSignal(SignalName.NetworkProcessTick, _netTickTimer);
		_netTickTimer = 0;
	}

	private void HandleSyncRequest(SyncClockPacket packet, NetPeer peer)
	{
		packet.ServerTick = _currentTick;
		SendToClient(peer.Id, packet, DeliveryMethod.Unreliable);
	}

	public void DrawDebugInfo()
	{
		if (!ImGui.CollapsingHeader("Clock")) return;
		ImGui.Text($"Network Tickrate {GetNetworkTickRate()}hz");
		ImGui.Text($"Current Tick {_currentTick}");
	}
}
