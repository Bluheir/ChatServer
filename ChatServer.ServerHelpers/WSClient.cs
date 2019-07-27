using System.Net.Sockets;
using ChatServer.ServerHelpers.Methods;
using System.Threading.Tasks;
using System;
using System.Net;
using Newtonsoft.Json;
using ChatServer.Messaging;

namespace ChatServer.ServerHelpers
{
	[Serializable]
	public class WSClient : IDisposable
	{
		private readonly NetworkStream _stream;
		private readonly TcpClient _client;
		private bool _disposed;
		private IPEndPoint _ep;

		public IPEndPoint Endpoint
		{
			get
			{
				if(_ep == null)
				{
					_ep = ((IPEndPoint)_client.Client.RemoteEndPoint);
				}
				return _ep;
			}
		}

		internal WSClient(TcpClient client)
		{
			_client = client;
			_stream = client.GetStream();
		}

		public async Task SendMessageAsync(String message)
		{
			byte[] bytes = Helpers.GetFrameFromString(message);
			await _stream.WriteAsync(bytes, 0, bytes.Length);
		}
		public async Task SendPacketAsync(string eventType, object packet)
		{
			byte[] bytes = Helpers.GetFrameFromString(JsonConvert.SerializeObject(new MessageObject(eventType, packet)));
			await _stream.WriteAsync(bytes, 0, bytes.Length);
		}

		public void Dispose()
		{
			if(!_disposed)
			{
				_disposed = true;
				_stream.Dispose();
			}
		}

	}
}
