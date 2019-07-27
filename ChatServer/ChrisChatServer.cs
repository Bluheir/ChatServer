using ChatServer.ServerHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Sodium;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Reflection;
using System.Dynamic;
using System.Runtime.Serialization;
using ChatServer.Messaging;
using ChatServer.ServerHelpers.Collections;
using Newtonsoft.Json.Linq;

namespace ChatServer
{
	public sealed class ChrisChatServer
	{
		private readonly WebSocketListener _wsserver;
		private readonly UDPListener _udpserver;
		private readonly Map<WSClient, int> _clients;
		private readonly Map<uint, WSClient> _voiceClients;
		private readonly IPEndPoint _broadcast;
		private readonly object lockobj;


		public ChrisChatServer(IPEndPoint ep, IPEndPoint udpep)
		{
			lockobj = new object();
			_wsserver = new WebSocketListener(ep);
			_udpserver = new UDPListener(udpep);
			_clients = new Map<WSClient, int>();
			_voiceClients = new Map<uint, WSClient>();

			_broadcast = new IPEndPoint(IPAddress.Broadcast, _udpserver.Endpoint.Port);

			_wsserver.OnConnect += OnConnect;
			_wsserver.OnReceive += OnReceive;
			_wsserver.OnDisconnect += OnDisconnect;

			_udpserver.OnReceive += UdpBytesReceived;
		}

		private async Task UdpBytesReceived(byte[] bytes, IPEndPoint endpoints)
		{
			
			var id = BitConverter.ToUInt32(SubArray(bytes, 0, sizeof(uint)), 0);
			
			if(!_voiceClients.Contains(id))
			{
				return;
			}

			
			var clientId = _clients[_voiceClients[id]];
			
			bytes = BitConverter.GetBytes(clientId).Concat(SubArray(bytes, 3, bytes.Length - 4)).ToArray();
			
			await _udpserver.SendAsync(bytes, endpoints);
		}

		private Task OnDisconnect(WSClient arg)
		{
			_clients.Remove(arg);
			_voiceClients.Remove(arg);
			return Task.CompletedTask;
		}

		public void Start()
		{
			_wsserver.StartListening();
			_udpserver.StartListening();
		}

		private async Task OnConnect(WSClient client)
		{
			Console.WriteLine("Client connected");
			_clients.Add(client, _clients.Count);
			var b = new MessageObject("CONNECT", new ConnectObject($"You have been assigned a client id of {_clients.Count}", _clients.Count));
			await client.SendMessageAsync(JsonConvert.SerializeObject(b));

		}
		public static T[] SubArray<T>(T[] data, int index, int length)
		{
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}
		private async Task OnReceive(WSClient client, string content, byte[] arg3)
		{
			if(content == null)
			{
				return;
			}
			if(!TryDeserialize(content.TrimEnd('\0'), out MessageObject obj))
			{
				return;
			}

			var id = _clients[client];

			switch(obj.EventType)
			{
				case "SEND":
					STextMessageObject message = ConvertToType<STextMessageObject>(obj.Value as JObject);
					foreach(var item in _clients)
					{
						await item.Key.SendMessageAsync(JsonConvert.SerializeObject(new MessageObject("MESSAGE", new TextMessageObject(message.Content, _clients[client]))));
					}
					break;
				case "VOICE_CONNECT":
					if(_voiceClients.Contains(client))
					{
						return;
					}
					Random a = new Random();
					uint b;
					while(true)
					{
						if(_voiceClients.Contains(b = (uint)a.Next(0, int.MaxValue)))
						{
							continue;
						}
						_voiceClients.Add(b, client);
						break;
					}
					await client.SendPacketAsync("VOICE_ACCEPT", new VoiceConnectionObject(b));
					break;
					
			}
		}


		public static T ConvertToType<T>(JObject obj)
		{
			var type = typeof(T);

			if (type.IsAbstract || type.IsInterface || type == typeof(string))
				return default;


			T instance = (T)FormatterServices.GetUninitializedObject(type);


			foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy))
			{
				if (!obj.ContainsKey(field.Name))
				{
					continue;
				}
				JToken val = obj[field.Name];

				field.SetValue(instance, val.ToObject(field.FieldType));
			}
			return instance;
		}
		public bool TryDeserialize<T>(string text, out T value)
		{
			try
			{
				value = JsonConvert.DeserializeObject<T>(text);
				return true;
			}
			catch
			{
				value = default;
				return false;
			}
		}
	}
}
