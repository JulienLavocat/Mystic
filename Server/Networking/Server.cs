using Godot;
using ImGuiNET;
using Mystic.Server.Character.Player;
using Vector2 = System.Numerics.Vector2;


namespace Mystic.Server.Networking;

[GlobalClass]
public partial class Server : Node
{
    [Signal]
    public delegate void ServerTickEventHandler(int currentTick);

    private readonly ServerHost _host = new();
    [Export] private Node _actors;
    
    [Export] private NetClock _clock;

    public static Server Instance { get; private set; }
    public static ServerHost Host => Instance._host;

    public override void _EnterTree()
    {
        Instance = this;
        _host.PeerConnectedEvent += peer =>
        {
            var actor = new PlayerActor();
            Actors.LinkToPeer(actor.Id, peer.Id, actor);
            _actors.AddChild(actor);
        };

        _host.PeerDisconnectedEvent += (peer, info) =>
        {
            if (!Actors.TryGetByPeerId(peer.Id, out var actor)) return;
            Actors.UnlinkActor(actor.Id, peer.Id);
            actor.QueueFree();
        };
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

    private void DisplayDebugInfo()
    {
        ImGui.SetNextWindowPos(Vector2.Zero);

        if (!ImGui.Begin("Server", ImGuiWindowFlags.AlwaysAutoResize)) return;

        ImGui.Text($"Framerate {Engine.GetFramesPerSecond()}fps");
        ImGui.Text($"Physics Tick {Engine.PhysicsTicksPerSecond}hz");
        _host.DrawDebugInfo();
        _clock.DrawDebugInfo();
        ImGui.End();
    }
}