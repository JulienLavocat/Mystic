using System;
using Godot;
using ImGuiNET;
using LiteNetLib;
using Vector2 = System.Numerics.Vector2;

namespace Mystic.Server.NetV2;

public static class Server
{

    public delegate void TicksSubscription(uint tick);
    public delegate void PacketsSubscription<in T>(T packet) where T : class, new();
    
    private static readonly EventBasedNetListener _listener = new();

    private static readonly NetManager _manager = new(_listener)
    {
        EnableStatistics = true
    };
    
    public static void Start()
    {
        _manager.Start(Configuration.Port);
    }
    
    
    public static void Send<T>(int peerId, T packet, DeliveryMethod method, byte channel = 0)
        where T : class, new()
    {
    }
    
    public static void SubscribeToTicks(TicksSubscription handler) {}

    public static void SubscribeToAllPackets<T>(PacketsSubscription<T> handler) where T : class, new()
    {
    }

    public static void SubscribeToClientPackets<T>(int clientId, PacketsSubscription<T> handler) where T : class, new()
    {
    }

    public static void Tick()
    {
        _manager.PollEvents();
    }
    
    private static void DisplayDebugInfo()
    {
        ImGui.SetNextWindowPos(Vector2.Zero);

        if (!ImGui.Begin("Server", ImGuiWindowFlags.AlwaysAutoResize)) return;
        
        ImGui.Text($"Tickrate {Engine.PhysicsTicksPerSecond}hz");
        if (ImGui.CollapsingHeader("Network"))
        {
            ImGui.Text($"Online: {_manager.ConnectedPeersCount}");
            ImGui.Text(
                $"Bytes received / Sent: {_manager.Statistics.BytesReceived}b / {_manager.Statistics.BytesSent}b"
            );
            ImGui.Text(
                $"Packets received / Sent: {_manager.Statistics.PacketsReceived} / {_manager.Statistics.PacketsSent}"
            );
            ImGui.Text(
                $"Lost packets: {_manager.Statistics.PacketLoss} ({_manager.Statistics.PacketLossPercent}%)"
            );
        }
        
        // _clock.DrawDebugInfo();
        ImGui.End();
    }
    
    
}