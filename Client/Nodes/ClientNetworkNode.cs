using System;
using Godot;
using LiteNetLib;
using Mystic.Shared.Packets;

namespace Mystic.Client.Nodes;

public abstract partial class ClientNetworkNode : Node
{
	protected bool NetworkReady { get; private set; }

	protected virtual void OnProcessTick(int currentTick, int currentRemoteTick)
	{
	}

	public override void _Ready()
	{
		Networking.Client.OnClientTick += OnProcessTick;

		if (Networking.Client.IsNetworkReady)
			OnNetworkReady();
		else
			Networking.Client.OnNetworkReady += OnNetworkReady;
	}

	private void OnNetworkReady()
	{
		NetworkReady = true;
	}

	protected void SendToServer<T>(T packet, DeliveryMethod method, byte channel = 0) where T : class, new()
	{
		Networking.Client.Send(packet, method, channel);
	}

	protected void SubscribeToEntitySnapshot(Action<EntitySnapshot, uint> cb)
	{
	}
}