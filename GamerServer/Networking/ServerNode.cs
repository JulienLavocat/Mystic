using Godot;

namespace Mystic.GamerServer.Networking;

public partial class ServerNode : Node
{
    public override void _Ready()
    {
        Server.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        Server.Tick(delta);
    }
}