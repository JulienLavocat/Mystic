using System.Diagnostics.Metrics;
using LiteNetLib;
using Mystic.Shared.Utils.Monitoring;

namespace Mystic.GamerServer;

public static class Metrics
{
	private static readonly Counter<long> Ticks = Instrumentation.Counter<long>("ticks");
	private static readonly UpDownCounter<long> Players = Instrumentation.UpDownCounter<long>("online_players");

	private static readonly Counter<long> UnknownPacketsReceived =
		Instrumentation.Counter<long>("unknown_packets_received");

	private static NetStatistics _netStatistics = new();

	static Metrics()
	{
		Instrumentation.ObservableCounter("bytes_sent", () => _netStatistics.BytesSent);
		Instrumentation.ObservableCounter("bytes_received", () => _netStatistics.BytesReceived);
		Instrumentation.ObservableCounter("packet_loss_percent", () => _netStatistics.PacketLossPercent);
		Instrumentation.ObservableCounter("packets_sent", () => _netStatistics.PacketsSent);
		Instrumentation.ObservableCounter("packets_received", () => _netStatistics.PacketsReceived);
	}

	public static void IncrementPlayersCounter(long delta = 1) => Players.Add(delta);

	public static void DecrementPlayersCounter(long delta = -1) => Players.Add(delta);

	public static void IncrementUnknownPacketsReceived(long delta = 1) => UnknownPacketsReceived.Add(delta);

	public static void IncrementTicks() => Ticks.Add(1);

	public static void SetNetStatistics(NetStatistics statistics)
	{
		_netStatistics = statistics;
	}
}