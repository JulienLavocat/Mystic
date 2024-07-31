using System;
using Godot;
using ImGuiNET;
using LiteNetLib;
using Mystic.Shared;

namespace Mystic.Server.Networking;

[GlobalClass]
public partial class Server : Node
{
    public static Server Instance { get; private set; }

    [Signal] public delegate void ServerTickEventHandler(int currentTick); 
    
    private readonly ServerHost _host = new();

    [Export]
    private NetworkClock _clock;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        GD.Print("Starting server");
        _host.Start();
    }

    public override void _Process(double delta)
    {
        _host.Process();
        DisplayDebugInfo();
    }

    public override void _PhysicsProcess(double delta)
    {
        _clock.ProcessTick();
    }
    
    public void SendToClient<T>(int peerId, T packet, LiteNetLib.DeliveryMethod method, byte channel = 0) where T : class, new()
    {
        _host.SendToClient(peerId, packet, method, channel);
    }

    public void SubscribeToPacket<T>(Action<T, NetPeer> action) where T : class, new()
    {
        _host.SubscribeToPacket(action);
    }

    private void DisplayDebugInfo()
    {
        ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero);
        if (ImGui.Begin("Server", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Framerate {Engine.GetFramesPerSecond()}fps");
            ImGui.Text($"Physics Tick {Engine.PhysicsTicksPerSecond}hz");
            _host.DrawDebugInfo();
            _clock.DrawDebugInfo();
            ImGui.End();
        }
    }
}
