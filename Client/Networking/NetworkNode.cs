using Godot;
using LiteNetLib;

namespace Mystic.Client.Networking;

public abstract partial class NetworkNode : Node
{
	protected bool NetworkReady { get; private set; }

	protected virtual void OnProcessTick(int currentTick, int currentRemoteTick)
	{
	}

	public override void _Ready()
	{
		Client.OnClientTick += OnProcessTick;
		Client.OnNetworkReady += OnNetworkReady;
	}

	private void OnNetworkReady()
	{
		NetworkReady = true;
	}

	protected void SendToServer<T>(T packet, DeliveryMethod method, byte channel = 0) where T : class, new()
	{
		Client.Send(packet, method, channel);
	}
}