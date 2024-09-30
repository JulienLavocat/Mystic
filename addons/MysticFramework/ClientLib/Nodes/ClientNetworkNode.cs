using System;
using Godot;
using LiteNetLib;
using MysticFramework.ClientLib.Networking;
using MysticFramework.Shared.Packets;

namespace MysticFramework.ClientLib.Nodes;

public abstract partial class ClientNetworkNode : Node
{
	protected bool NetworkReady { get; private set; }

	protected virtual void OnProcessTick(int currentTick, int currentRemoteTick)
	{
	}

	public override void _Ready()
	{
		Client.OnClientTick += OnProcessTick;

		if (Client.IsNetworkReady)
			OnNetworkReady();
		else
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

	protected void SubscribeToEntitySnapshot(Action<EntitySnapshot, uint> cb)
	{
	}
}
