using System.Net;
using System.Net.Sockets;
using Godot;
using LiteNetLib;

namespace Mystic.Server.Networking;

[GlobalClass]
public partial class Server : Node, INetEventListener
{
	public static Server Instance { get; private set; }

	private NetManager _netManager;

	public override void _Ready()
	{
		Instance = this;
		_netManager = new NetManager(this);
		_netManager.Start(9050);
		GD.Print("Server listening on 9050");
	}

	public override void _Process(double delta)
	{
		_netManager.PollEvents();
	}

	public void OnPeerConnected(NetPeer peer)
	{
		GD.Print("Peer connected");
	}

	public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) { }

	public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }

	public void OnNetworkReceive(
		NetPeer peer,
		NetPacketReader reader,
		byte channelNumber,
		DeliveryMethod deliveryMethod
	) { }

	public void OnNetworkReceiveUnconnected(
		IPEndPoint remoteEndPoint,
		NetPacketReader reader,
		UnconnectedMessageType messageType
	) { }

	public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

	public void OnConnectionRequest(ConnectionRequest request)
	{
		GD.Print("Got connection request");
		request.Accept();
	}
}
