using System.Collections.Generic;
using System.Linq;
using Godot;
using ImGuiNET;
using LiteNetLib;
using Mystic.Shared.Packets;
using Mystic.Shared.Utils;

namespace Mystic.Client.Networking;

public partial class NetworkClock : Node
{
    [Signal] public delegate void LatencyCalculatedEventHandler(int latencyAverageTicks, int jitterAverageTicks); 
    
    [Export] private double _syncRequestRateSec = 1;
    [Export] private int _sampleSize = 11;
    [Export] private int _minLatency = 50;
    [Export] private int _fixedTickMargin = 3;
    
    private Timer _timer;
    private int _currentTick;
    private int _immediateLatencyMs;
    private int _syncPacketsReceived;
    private int _averageOffsetInTicks;
    private int _averageLatencyInTicks;
    private int _lastOffset;
    private int _minLatencyInTicks;
    private int _jitterInTicks;

    private List<int> _offsetValues = new();
    private List<int> _latencyValues = new();

    public int GetCurrentTick() => _currentTick;
    public int GetCurrentRemoteTick() => _currentTick + _averageLatencyInTicks + _jitterInTicks + _fixedTickMargin;

    public override void _Ready()
    {
        Client.Instance.SubscribeToPacket<SyncClockPacket>(OnSyncReceived);
        
        _timer = new Timer();
        _timer.WaitTime = _syncRequestRateSec;
        _timer.Autostart = true;
        AddChild(_timer);
        _timer.Connect(Timer.SignalName.Timeout, Callable.From(SendSyncRequest));
        
        _minLatencyInTicks = PhysicsUtils.MsecToTick(_minLatency); 
    }

    public void ProcessTick()
    {
        _currentTick += 1 + _lastOffset;
        _lastOffset = 0;
    }
    
    private void OnSyncReceived(SyncClockPacket packet, NetPeer peer)
    {
        _immediateLatencyMs = ((int)Time.GetTicksMsec() - packet.ClientTime) / 2;
        var immediateLatencyInTicks = PhysicsUtils.MsecToTick(_immediateLatencyMs);
        var immediateOffsetInTicks = (packet.ServerTick - _currentTick) + immediateLatencyInTicks; // Time difference between our clock and the server clock accounting for latency
        
        _offsetValues.Add(immediateOffsetInTicks);
        _latencyValues.Add(immediateLatencyInTicks);

        if (_offsetValues.Count < _sampleSize) return;
        
        // Calculate average clock offset for the lasts n samples
        _averageOffsetInTicks = SimpleAverage(_offsetValues);
        _lastOffset = _averageOffsetInTicks; // To adjust the clock
            
        // Calculate average latency for the lasts n samples 
        _latencyValues.Sort();
        _jitterInTicks = _latencyValues[^1] - _latencyValues[0];
        _averageLatencyInTicks = SmoothAverage(_latencyValues, _minLatency);
        
        EmitSignal(SignalName.LatencyCalculated, _averageLatencyInTicks, _jitterInTicks); 
        
        GD.Print($"At tick {_currentTick}, latency calculations done. Avg. Latency {_averageLatencyInTicks} ticks, Jitter {_jitterInTicks} ticks, Clock Offset {_lastOffset} ticks"); 
            
        _offsetValues.Clear();
        _latencyValues.Clear();
    }

    private static int SmoothAverage(List<int> samples, int minValue)
    {
        var sampleSize = samples.Count;
        var middleValue = samples[samples.Count / 2];
        var sampleCount = 0;

        for (var i = 0; i < sampleSize; i++)
        {
            var value = samples[i];

            if (value > (2 * middleValue) && value > minValue)
            {
                samples.RemoveAt(i);
                sampleSize--;
            }
            else
            {
                sampleCount += value;
            }
        }

        return sampleCount / sampleSize;
    }

    private static int SimpleAverage(List<int> samples)
    {
        if (samples.Count <= 0) return 0;
        return samples.Sum() / samples.Count;
    }

    private static void SendSyncRequest()
    {
        Client.Instance.Send(new SyncClockPacket
        {
            ClientTime = (int)Time.GetTicksMsec(),
        }, DeliveryMethod.Unreliable);
    }
    
    public void DisplayDebugInfo()
    {
        if (!ImGui.CollapsingHeader("Network Clock Information")) return;
        
        ImGui.Text($"Synced Tick {GetCurrentRemoteTick()}");
        ImGui.Text($"Local Tick: {GetCurrentTick()}");
        ImGui.Text($"Immediate Latency: {_immediateLatencyMs}ms");
        ImGui.Text($"Average Latency {_averageLatencyInTicks} ticks");
        ImGui.Text($"Latency Jitter {_jitterInTicks} ticks");
        ImGui.Text($"Average Offset {_averageOffsetInTicks} ticks");
    } 
}
