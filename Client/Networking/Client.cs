using System.Net;
using System.Net.Sockets;
using Godot;
using LiteNetLib;

namespace Mystic.Client.Networking {
    
    [GlobalClass]
    public partial class Client : Node, INetEventListener
    {
        private NetManager _netManager;

        public override void _Ready()
        {
            _netManager = new NetManager(this);
            Connect();
        }

        public override void _Process(double delta)
        {
            _netManager.PollEvents();
        }

        public void Connect()
        {
            _netManager.Start();
            _netManager.Connect("localhost", 9050, "");
        }
        
        public void OnPeerConnected(NetPeer peer)
        {
            GD.Print("Connected to server");
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            GD.PrintErr(socketError);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
        }
    }
}
