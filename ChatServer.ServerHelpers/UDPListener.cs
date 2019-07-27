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
		private const int SIO_UDP_CONNRESET = -1744830452;
		private bool _disposed;

		public IPEndPoint Endpoint { get; }

		public event Func<byte[], IPEndPoint, Task> OnReceive;

		public UDPListener(IPEndPoint hostEndpoint)
		{
			_server = new UdpClient(hostEndpoint)
			{
				EnableBroadcast = true,
				
			};
			
			Endpoint = hostEndpoint;
		}
		~UDPListener()
		{
			Dispose();
		}

		public UDPListener(IPAddress addr, int port) : this(new IPEndPoint(addr, port)) { }

		public async void StartListening()
		{
			while (true)
			{
				try
				{
					var b = await _server.ReceiveAsync();
					if (OnReceive != null)
						await OnReceive(b.Buffer, b.RemoteEndPoint);
				}
				catch
				{
					
				}
			}
		}

		public async Task<int> SendAsync(byte[] dgram, string host, int port)
		=> await _server.SendAsync(dgram, dgram.Length, host, port);
		
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
