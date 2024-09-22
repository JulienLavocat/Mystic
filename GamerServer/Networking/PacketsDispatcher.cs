using System;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Mystic.GamerServer.Networking;

public class PacketsDispatcher
{
	public delegate void ClientPacketsSubscription<in T>(T packet) where T : class, new();

	public delegate void PacketsSubscription<in T>(int clientId, T packet) where T : class, new();

	private readonly Dictionary<ulong, Dictionary<int, Delegate>> _clientSubscriptions = new();

	private readonly Dictionary<ulong, Delegate> _globalSubscriptions = new();

	private readonly NetPacketProcessor _processor = new();

	private ulong GetHash<T>() => HashCache<T>.Id;

	/*
	 * Subscribe to all packets of type T.
	 * It should only be used for objects that lives forever as global subscriptions aren't removed
	 */
	public void Subscribe<T>(PacketsSubscription<T> onReceived) where T : class, new()
	{
		RegisterNewHandler<T>();

		var hash = GetHash<T>();
		if (_globalSubscriptions.TryGetValue(hash, out var value))
			_globalSubscriptions[hash] = Delegate.Combine(value, onReceived);
		else
			_globalSubscriptions[hash] = onReceived;
	}

	/*
	 * Subscribe to any packets of type T coming from an Actor
	 */
	public void Subscribe<T>(int clientId, ClientPacketsSubscription<T> onReceived) where T : class, new()
	{
		RegisterNewHandler<T>();

		var hash = GetHash<T>();
		if (!_clientSubscriptions.TryGetValue(hash, out var subscriptions))
		{
			_clientSubscriptions[hash] = new Dictionary<int, Delegate> { { clientId, onReceived } };
			return;
		}

		if (subscriptions.TryGetValue(clientId, out var value))
			subscriptions[clientId] = Delegate.Combine(value, onReceived);
		else
			subscriptions[clientId] = onReceived;
	}

	private void RegisterNewHandler<T>() where T : class, new()
	{
		_processor.Subscribe<T, int>(
			(packet, clientId) =>
			{
				if (_globalSubscriptions.TryGetValue(GetHash<T>(), out var globalSubscription))
					((PacketsSubscription<T>)globalSubscription)(clientId, packet);


				if (!_clientSubscriptions.TryGetValue(GetHash<T>(), out var subscriptions)) return;
				foreach (var subscription in subscriptions.Values)
					((ClientPacketsSubscription<T>)subscription)(packet);
			}, () => new T());
	}

	public void Unsubscribe(int clientId)
	{
		foreach (var subscription in _clientSubscriptions.Values) subscription.Remove(clientId);
	}

	public void Unsubscribe<T>(int clientId, ClientPacketsSubscription<T> subscription) where T : class, new()
	{
		var hash = GetHash<T>();
		if (!_clientSubscriptions.TryGetValue(hash, out var subscriptions)) return;

		if (subscriptions.TryGetValue(clientId, out var value))
			subscriptions[clientId] = Delegate.Remove(value, subscription);
	}

	public void Unsubscribe<T>(PacketsSubscription<T> subscription) where T : class, new()
	{
		var hash = GetHash<T>();
		if (_globalSubscriptions.TryGetValue(hash, out var value))
			_globalSubscriptions[hash] = Delegate.Remove(value, subscription);
	}

	public void Write<T>(NetDataWriter writer, T packet) where T : class, new()
	{
		_processor.Write(writer, packet);
	}


	public void ReadAllPackets(NetDataReader reader, int peerId)
	{
		_processor.ReadAllPackets(reader, peerId);
	}

	public void RegisterType<T>() where T : struct, INetSerializable
	{
		_processor.RegisterNestedType<T>();
	}


	private static class HashCache<T>
	{
		public static readonly ulong Id;

		static HashCache()
		{
			var num1 = 14695981039346656037;
			foreach (ulong num2 in typeof(T).ToString())
				num1 = (num1 ^ num2) * 1099511628211UL;
			Id = num1;
		}
	}
}