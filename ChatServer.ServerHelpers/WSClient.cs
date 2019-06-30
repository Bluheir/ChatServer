using System.Net.Sockets;
using System.Text;
using ChatServer.ServerHelpers.Methods;
using System.Threading.Tasks;
using System;

namespace ChatServer.ServerHelpers
{
	[Serializable]
	public class WSClient : IDisposable
	{
		private readonly NetworkStream _stream;
		private readonly TcpClient _client;
		private bool _disposed;

		
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
