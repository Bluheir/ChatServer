using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ChatServer.ServerHelpers
{
	[Serializable]
	public class UDPListener : IDisposable
	{
		private readonly UdpClient _server;
		private bool _disposed;

		public IPEndPoint Endpoint { get; }

		public event Func<byte[], IPEndPoint, Task> BytesReceived = (x, y) => { return Task.CompletedTask; };

		public UDPListener(IPEndPoint hostEndpoint)
		{
			_server = new UdpClient();
			Endpoint = hostEndpoint;
			_server.Connect(Endpoint);
		}
		public UDPListener(IPAddress addr, int port) : this(new IPEndPoint(addr, port)) { }

		public async void StartListening()
		{
			while (true)
			{
				var b = await _server.ReceiveAsync();
				await BytesReceived(b.Buffer, b.RemoteEndPoint);
			}
		}

		public async Task<int> SendAsync(byte[] dgram, int bytes, IPEndPoint ep)
		=> await _server.SendAsync(dgram, bytes, ep);

		public async Task<int> SendAsync(byte[] dgram, IPEndPoint ep)
		=> await SendAsync(dgram, dgram.Length, ep);		

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				_server.Dispose();
			}
		}
	}
}
