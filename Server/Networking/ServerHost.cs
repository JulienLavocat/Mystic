using System;
using Godot;
using ImGuiNET;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Mystic.Server.Networking;

public class ServerHost
{
    public delegate void OnPeerConnected(NetPeer peer);

    public delegate void OnPeerDisconnected(NetPeer peer, DisconnectInfo info);

    private readonly NetManager _netManager;
    private readonly PacketsProcessor _processor = new();
    private readonly NetDataWriter _writer = new();

    public ServerHost()
    {
        var listener = new EventBasedNetListener();
        _netManager = new NetManager(listener)
        {
            EnableStatistics = true
        };

        listener.PeerConnectedEvent += peer =>
            PeerConnectedEvent?.Invoke(peer);
        listener.PeerDisconnectedEvent += (peer, info) => PeerDisconnectedEvent?.Invoke(peer, info);
        listener.ConnectionRequestEvent += rq => rq.Accept();
        listener.NetworkReceiveEvent += (peer, reader, channel, method) => _processor.ReadAllPackets(reader, peer.Id);
    }

    public event OnPeerConnected PeerConnectedEvent;
    public event OnPeerDisconnected PeerDisconnectedEvent;


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
        _processor.Write(_writer, packet);
        _netManager.ConnectedPeerList[peerId].Send(_writer, channel, method);
    }

    public void SubscribeToPacket<T>(Action<T, ActorNode> action) where T : class, new()
    {
        _processor.Subscribe(action);
    }

    public void SubscribeToPacket<T>(uint id, Action<T> action) where T : class, new()
    {
        _processor.Subscribe(id, action);
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
}