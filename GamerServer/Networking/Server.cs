using System.Collections.Generic;
using Godot;
using ImGuiNET;
using LiteNetLib;
using Mystic.Shared.Utils.Monitoring;
using Vector2 = System.Numerics.Vector2;

namespace Mystic.GamerServer.Networking;

public static class Server
{
	public delegate void TicksSubscription(double delta);

	private static readonly Host Host = new();
	private static readonly Dictionary<int, Client> Clients = new();

	public static uint CurrentTick { get; private set; }

	public static event TicksSubscription OnTick;

	public static void Start()
	{
		Host.PeerConnectedEvent += peer =>
		{
			using var activity = Instrumentation.Measure("PlayerConnected");
			Metrics.IncrementPlayersCounter();
			Clients.Add(peer.Id, new Client(peer.Id, peer));
			GD.Print($"Client #{peer.Id} connected");
		};

		Host.PeerDisconnectedEvent += (peer, _) =>
		{
			using var activity = Instrumentation.Measure("PlayerDisconnected");
			Metrics.DecrementPlayersCounter();
			Clients.Remove(peer.Id);
			GD.Print($"Client #{peer.Id} disconnected");
		};

		Host.Start();
		GD.Print($"Server started on port {Configuration.Port} ");
	}

	public static bool GetClient(int id, out Client client) => Clients.TryGetValue(id, out client);

	public static void Send<T>(int clientId, T packet, DeliveryMethod method, byte channel = 0)
		where T : class, new()
	{
		if (Clients.TryGetValue(clientId, out var client))
			client.Send(packet, method, channel);
		;
	}

	public static void SubscribeToTicks(TicksSubscription handler)
	{
		OnTick += handler;
	}

	public static void UnsubscribeFromTicks(TicksSubscription handler)
	{
		OnTick -= handler;
	}

	public static void Subscribe<T>(PacketsDispatcher.PacketsSubscription<T> action)
		where T : class, new()
	{
		Host.Subscribe(action);
	}

	public static void Subscribe<T>(int id, PacketsDispatcher.ClientPacketsSubscription<T> action)
		where T : class, new()
	{
		Host.Subscribe(id, action);
	}

	public static void Unsubscribe<T>(PacketsDispatcher.PacketsSubscription<T> action)
		where T : class, new()
	{
		Host.Unsubscribe(action);
	}

	public static void Unsubscribe<T>(int id, PacketsDispatcher.ClientPacketsSubscription<T> action)
		where T : class, new()
	{
		Host.Unsubscribe(id, action);
	}

	public static void Tick(double delta)
	{
		using var activity = Instrumentation.Measure("Tick");
		Metrics.IncrementTicks();
		CurrentTick++;
		Host.Tick();
		OnTick?.Invoke(delta);
	}

	private static void DisplayDebugInfo()
	{
		ImGui.SetNextWindowPos(Vector2.Zero);

		if (!ImGui.Begin("Server", ImGuiWindowFlags.AlwaysAutoResize))
			return;

		ImGui.Text($"Tick rate {Engine.PhysicsTicksPerSecond}hz");
		ImGui.Text($"Current Tick {CurrentTick}");

		Host.DrawDebugInfo();

		ImGui.End();
	}
}