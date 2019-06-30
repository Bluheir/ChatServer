#pragma warning disable IDE0032

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace ChatServer.ServerHelpers
{
	[Serializable]
	public sealed class UDPWSServer
	{
		private readonly WebSocketListener _ws;
		private readonly UDPListener _udp;
		private readonly IPEndPoint _endPoint;

		public event Func<WSClient, Task> OnConnect;
		public event Func<WSClient, Task> OnDisconnect;
		public event Func<WSClient, string, byte[], Task> WSOnReceive;
		public event Func<byte[], IPEndPoint, Task> UDPOnReceive;

		public IPEndPoint EndPoint => _endPoint;

		public UDPWSServer(IPEndPoint ep)
		{
			_endPoint = ep;
			_ws = new WebSocketListener(ep);

			_ws.OnConnect += OnConnect;
			_ws.OnReceive += WSOnReceive;
			_ws.OnDisconnect += OnDisconnect;

			_udp = new UDPListener(ep);

			_udp.OnReceive += UDPOnReceive;
		}

		public void StartListening()
		{
			_udp.StartListening();
			_ws.StartListening();
		}

		public async Task<int> SendAsync(byte[] dgram, int bytes, IPEndPoint ep)
		=> await _udp.SendAsync(dgram, bytes, ep);

		public async Task<int> SendAsync(byte[] dgram, IPEndPoint ep)
		=> await SendAsync(dgram, dgram.Length, ep);
	}
}
