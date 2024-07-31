using System;
using Godot;
using ImGuiNET;
using LiteNetLib;
using LiteNetLib.Utils;
using Mystic.Shared;

namespace Mystic.Server.Networking;

public class ServerHost
{
    private readonly NetManager _netManager;
    private readonly NetPacketProcessor _packetProcessor = new();
    private readonly NetDataWriter _writer = new();

    public ServerHost()
    {
        var listener = new EventBasedNetListener();
        _netManager = new NetManager(listener)
        {
            EnableStatistics = true
        };

        listener.PeerConnectedEvent += OnPeerConnected;
        listener.PeerDisconnectedEvent += OnPeerDisconnected;
        listener.ConnectionRequestEvent += OnConnectionRequest;
        listener.NetworkReceiveEvent += OnNetworkReceive;
    }


    public void Start()
    {
        _netManager.Start(9050);
        GD.Print("Server listening on 9050");
    }

    public void Process()
    {
        _netManager.PollEvents();
    }

    public void SendToClient<T>(int peerId, T packet, DeliveryMethod method, byte channel = 0) where T : class, new()
    {
        _writer.Reset();
        _packetProcessor.Write(_writer, packet);
        _netManager.ConnectedPeerList[peerId].Send(_writer, channel, method);
    }

    public void SubscribeToPacket<T>(Action<T, NetPeer> action) where T : class, new()
    {
        _packetProcessor.SubscribeReusable(action);
    }
    
    public void DrawDebugInfo()
    {
        if (!ImGui.CollapsingHeader("Network")) return;
        ImGui.Text($"Online: {_netManager.ConnectedPeersCount}");
        ImGui.Text(
            $"Bytes received / Sent: {_netManager.Statistics.BytesReceived}b / {_netManager.Statistics.BytesSent}b"
        );
        ImGui.Text(
            $"Packets received / Sent: {_netManager.Statistics.PacketsReceived} / {_netManager.Statistics.PacketsSent}"
        );
        ImGui.Text(
            $"Lost packets: {_netManager.Statistics.PacketLoss} ({_netManager.Statistics.PacketLossPercent}%)"
        );
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        request.Accept();
    }

    private void OnPeerConnected(NetPeer peer)
    {
        GD.Print("Peer connected");
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        GD.Print("Peer disconnected");
    }
    
    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        _packetProcessor.ReadAllPackets(reader, peer);
    }
}
