using System;
using System.Net;
using System.Net.Sockets;
using Godot;
using ImGuiNET;
using LiteNetLib;
using LiteNetLib.Utils;
using Vector2 = System.Numerics.Vector2;

namespace Mystic.Client.Networking;

[GlobalClass]
public partial class Client : Node
{
	[Signal]
	public delegate void ClientTickEventHandler(int currentTick, int currentRemoteTick);

	[Signal]
	public delegate void NetworkReadyEventHandler();

	private readonly NetPacketProcessor _packetProcessor = new();
	private readonly NetDataWriter _writer = new();

	[Export] private NetClock _clock;

	private NetManager _netManager;
	public static Client Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		var listener = new EventBasedNetListener();
		_netManager = new NetManager(listener)
		{
			EnableStatistics = true,
		};

		listener.PeerConnectedEvent += OnPeerConnected;
		listener.PeerDisconnectedEvent += OnPeerDisconnected;
		listener.NetworkErrorEvent += OnNetworkError;
		listener.NetworkReceiveEvent += OnNetworkReceive;

		Connect();
	}

	public override void _Process(double delta)
	{
		_netManager.PollEvents();
		DisplayDebugInfo();
	}

	public override void _PhysicsProcess(double delta)
	{
		_clock.ProcessTick();
		EmitSignal(SignalName.ClientTick, _clock.GetCurrentTick(), _clock.GetCurrentRemoteTick());
	}

	public void Connect()
	{
		_netManager.Start();
		if (_netManager.Connect("localhost", 30000, "") != null) EmitSignal(SignalName.NetworkReady);
	}

	public void Send<T>(T packet, DeliveryMethod method, byte channel = 0) where T : class, new()
	{
		_writer.Reset();
		_packetProcessor.Write(_writer, packet);
		_netManager.FirstPeer?.Send(_writer, channel, method);
	}

	public void SubscribeToPacket<T>(Action<T, NetPeer> action) where T : class, new()
	{
		_packetProcessor.SubscribeReusable(action);
	}

	private void DisplayDebugInfo()
	{
		ImGui.SetNextWindowPos(Vector2.Zero);
		ImGui.Begin("Client", ImGuiWindowFlags.AlwaysAutoResize);
		ImGui.Text(
			$"Connected: {_netManager.FirstPeer?.ConnectionState ?? ConnectionState.Disconnected}"
		);
		ImGui.Text(
			$"Bytes received / Sent: {_netManager.Statistics.BytesReceived}b / {_netManager.Statistics.BytesSent}b"
		);
		ImGui.Text(
			$"Packets received / Sent: {_netManager.Statistics.PacketsReceived} / {_netManager.Statistics.PacketsSent}"
		);
		ImGui.Text(
			$"Lost packets: {_netManager.Statistics.PacketLoss} ({_netManager.Statistics.PacketLossPercent}%)"
		);
		_clock.DisplayDebugInfo();
		ImGui.End();
	}

	public void OnPeerConnected(NetPeer peer)
	{
		GD.Print("Connected to server");
	}

	public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
	{
		GD.PrintErr("disconnected", disconnectInfo.Reason);
	}

	public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
	{
		GD.PrintErr(socketError);
	}

	public void OnNetworkReceive(
		NetPeer peer,
		NetPacketReader reader,
		byte channelNumber,
		DeliveryMethod deliveryMethod
	)
	{
		_packetProcessor.ReadAllPackets(reader, peer);
	}
}
