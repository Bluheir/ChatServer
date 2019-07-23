#pragma warning disable IDE0032

using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChatServer.ServerHelpers.Methods;
using System.Linq;
using System.Collections;

namespace ChatServer.ServerHelpers
{
	[Serializable]
	public class WebSocketListener : IDisposable, IEnumerable<WSClient>
	{
		private readonly TcpListener _listener;
		private readonly IPEndPoint _ep;
		private static Encoding enc = Encoding.UTF8;
		private bool _disposed;
		private int _bSize;
		private List<WSClient> _clients;
		private Func<WSClient, string, byte[], Task> _onReceive;
		private Func<WSClient, Task> _onConnect;
		private Func<WSClient, Task> _onDisconnect;


		public event Func<WSClient, string, byte[], Task> OnReceive
		{
			add
			{
				if (value == null)
					return;
				if(_onReceive == null)
				{
					_onReceive = value;
					return;
				}
				_onReceive += value;
			}
			remove
			{
				if (value == null)
					return;
				if (_onReceive != null)
				{
					_onReceive -= value;
				}
			}
		}
		public event Func<WSClient, Task> OnConnect
		{
			add
			{
				if (value == null)
					return;
				if (_onConnect == null)
				{
					_onConnect = value;
					return;
				}
				_onConnect += value;
			}
			remove
			{
				if (value == null)
					return;
				if (_onConnect != null)
				{
					_onConnect -= value;
				}
			}
		}
		public event Func<WSClient, Task> OnDisconnect
		{
			add
			{
				if (value == null)
					return;
				if (_onDisconnect == null)
				{
					_onDisconnect = value;
					return;
				}
				_onDisconnect += value;
			}
			remove
			{
				if (value == null)
					return;
				if (_onDisconnect != null)
				{
					_onDisconnect -= value;
				}
			}
		}

		public int BufferSize { get => _bSize; set => _bSize = value; }
		public IReadOnlyList<WSClient> ConnectedClients => _clients.ToList();
		public WSClient this[int index]
		{
			get
			{
				return _clients[index];
			}
		}

		public WebSocketListener(IPEndPoint ep, int bufferSize = 4096)
		{
			_ep = ep;
			_clients = new List<WSClient>();
			_listener = new TcpListener(ep);
			_bSize = bufferSize;
		}
		public WebSocketListener(IPAddress address, int port) : this(new IPEndPoint(address, port)) { }

		public void Dispose()
		{
			if(!_disposed)
			{
				_disposed = true;
				_listener.Stop();
			}
		}

		public bool ContainsClient(WSClient client)
		{
			return _clients.Contains(client);
		}

		public async void StartListening()
		{
			_listener.Start();
			while(true)
			{
				try
				{
					var client = await _listener.AcceptTcpClientAsync();
					HandleStream(client);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}

		private async void HandleStream(TcpClient client)
		{
			var stream = client.GetStream();
			try
			{
				byte[] data = new byte[_bSize];
				var i = await stream.ReadAsync(data, 0, data.Length);
				string val = enc.GetString(data, 0, i);

				string requestKey = Helpers.GetHandshakeRequestKey(val);
				string response = (Helpers.GetHandshakeResponse(Helpers.HashKey(requestKey)));
				byte[] vs = enc.GetBytes(response);

				await stream.WriteAsync(vs, 0, vs.Length);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}

			var c = new WSClient(client);
			_clients.Add(c);
			if(_onConnect != null)
				await _onConnect(c);	

			while (true)
			{
				try
				{
					
					byte[] data = new byte[_bSize];
					var i = await stream.ReadAsync(data, 0, data.Length);
					var b = Helpers.GetFrameData(data);

					if(b.Opcode != EOpcodeType.Text)
					{
						if(_onReceive != null)
							await _onReceive(c, null, data);
					}
					else
					{
						if(_onReceive != null)
							await _onReceive(c, Helpers.GetDataFromFrame(b, data), data);
					}
				}
				catch
				{
					c.Dispose();
					_clients.Remove(c);
					if(_onDisconnect != null)
						await _onDisconnect(c);
					return;
				}
			}
		}
		
		public static bool GetBit(byte b, int bitNumber)
		{
			return (b & (1 << bitNumber)) != 0;
		}

		public IEnumerator<WSClient> GetEnumerator()
		{
			return _clients.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
