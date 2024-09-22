using Godot;
using ImGuiNET;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Mystic.GamerServer.Networking;

public class Host
{
	public delegate void OnPeerConnected(NetPeer peer);

	public delegate void OnPeerDisconnected(NetPeer peer, DisconnectInfo info);

	private readonly PacketsDispatcher _dispatcher = new();


	private readonly EventBasedNetListener _listener = new();
	private readonly NetManager _manager;

	public Host()
	{
		_manager = new NetManager(_listener)
		{
			EnableStatistics = true
		};

		_listener.PeerConnectedEvent += peer =>
			PeerConnectedEvent?.Invoke(peer);
		_listener.PeerDisconnectedEvent += (peer, info) => PeerDisconnectedEvent?.Invoke(peer, info);
		_listener.ConnectionRequestEvent += rq => rq.Accept();
		_listener.NetworkReceiveEvent += (peer, reader, channel, method) =>
		{
			try
			{
				_dispatcher.ReadAllPackets(reader, peer.Id);
			}
			catch (ParseException)
			{
				GD.PushWarning("received an invalid packet");
				peer.Disconnect();
				Metrics.IncrementUnknownPacketsReceived();
			}
		};
	}

	public event OnPeerConnected PeerConnectedEvent;
	public event OnPeerDisconnected PeerDisconnectedEvent;


	public void Start()
	{
		Metrics.SetNetStatistics(_manager.Statistics);
		_manager.Start(Configuration.Port);
	}

	public void Stop()
	{
		_manager.Stop();
	}

	public void Tick()
	{
		_manager.PollEvents();
	}

	public void Subscribe<T>(PacketsDispatcher.PacketsSubscription<T> action) where T : class, new()
	{
		_dispatcher.Subscribe(action);
	}

	public void Subscribe<T>(int id, PacketsDispatcher.ClientPacketsSubscription<T> action) where T : class, new()
	{
		_dispatcher.Subscribe(id, action);
	}

	public void Unsubscribe<T>(PacketsDispatcher.PacketsSubscription<T> action) where T : class, new()
	{
		_dispatcher.Unsubscribe(action);
	}

	public void Unsubscribe<T>(int id, PacketsDispatcher.ClientPacketsSubscription<T> action) where T : class, new()
	{
		_dispatcher.Unsubscribe(id, action);
	}

	public void RegisterNestedType<T>() where T : struct, INetSerializable => _dispatcher.RegisterType<T>();

	public void DrawDebugInfo()
	{
		if (!ImGui.CollapsingHeader("Network")) return;
		var stats = _manager.Statistics;
		ImGui.Text($"Online: {_manager.ConnectedPeersCount}");
		ImGui.Text(
			$"Bytes received / Sent: {stats.BytesReceived}b / {stats.BytesSent}b"
		);
		ImGui.Text(
			$"Packets received / Sent: {stats.PacketsReceived} / {stats.PacketsSent}"
		);
		ImGui.Text(
			$"Lost packets: {stats.PacketLoss} ({stats.PacketLossPercent}%)"
		);
	}
}