using System;
using System.Net;
using System.Net.Sockets;
using Godot;
using ImGuiNET;
using LiteNetLib;
using LiteNetLib.Utils;
using Vector2 = System.Numerics.Vector2;

namespace Mystic.Client.Networking;

public static class Client
{
	public delegate void ClientTickEventHandler(int currentTick, int currentRemoteTick);

	public delegate void NetworkReadyEventHandler();

	private static readonly NetPacketProcessor PacketProcessor = new();
	private static readonly NetDataWriter Writer = new();

	private static NetClock _clock;

	private static NetManager _netManager;

	public static event NetworkReadyEventHandler OnNetworkReady;
	public static event ClientTickEventHandler OnClientTick;

	public static void Update()
	{
		_netManager.PollEvents();
		DisplayDebugInfo();
	}

	public static void Tick()
	{
		_clock.ProcessTick();
		OnClientTick?.Invoke(_clock.GetCurrentTick(), _clock.GetCurrentRemoteTick());
	}

	public static void Connect(NetClock clock)
	{
		_clock = clock;
		var listener = new EventBasedNetListener();
		_netManager = new NetManager(listener)
		{
			EnableStatistics = true
		};

		listener.PeerConnectedEvent += OnPeerConnected;
		listener.PeerDisconnectedEvent += OnPeerDisconnected;
		listener.NetworkErrorEvent += OnNetworkError;
		listener.NetworkReceiveEvent += OnNetworkReceive;

		_netManager.Start();
		if (_netManager.Connect("localhost", 30000, "") != null) OnNetworkReady?.Invoke();
	}

	public static void Send<T>(T packet, DeliveryMethod method, byte channel = 0) where T : class, new()
	{
		Writer.Reset();
		PacketProcessor.Write(Writer, packet);
		_netManager.FirstPeer?.Send(Writer, channel, method);
	}

	public static void SubscribeToPacket<T>(Action<T, NetPeer> action) where T : class, new()
	{
		PacketProcessor.SubscribeReusable(action);
	}

	private static void DisplayDebugInfo()
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

	private static void OnPeerConnected(NetPeer peer)
	{
		GD.Print("Connected to server");
	}

	private static void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
	{
		GD.PrintErr("Disconnected from server: ", disconnectInfo.Reason);
	}

	private static void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
	{
		GD.PrintErr(socketError);
	}

	private static void OnNetworkReceive(
		NetPeer peer,
		NetPacketReader reader,
		byte channelNumber,
		DeliveryMethod deliveryMethod
	)
	{
		PacketProcessor.ReadAllPackets(reader, peer);
	}
}
