#pragma warning disable IDE0032

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Reflection;
using System.Dynamic;
using ChatServer.Messaging;
using Concentus.Enums;
using Newtonsoft.Json.Linq;
using System.Collections;
using NAudio.Wave;
using System.Collections.Concurrent;
using Concentus.Structs;
using NAudio.Wave.SampleProviders;

namespace ChatServer.ChatServerClient
{
	public class ChatClient : IDisposable
	{
		private readonly Encoding enc;
		private readonly UdpClient _udp;
		private readonly ClientWebSocket _ws;
		private readonly WaveInEvent _audioIn;
		private readonly OpusEncoder _encoder;
		private readonly OpusDecoder _decoder;
		private readonly IPEndPoint udpEndpoint;
		private readonly CancellationTokenSource tokenSource;
		private readonly MixingSampleProvider _sampleProvider;
		private readonly WaveOutEvent _audioOut;
		private readonly SampleToWaveProvider16 _provider;
		private readonly ConcurrentDictionary<uint, BufferedWaveProvider> _providers;
		private readonly Uri wsEndpoint;
		private readonly int _frames;
		private readonly object _lock;
		private bool playing;
		private UInt32 _voiceId;
		private bool disposed;
		private int clientId;

		public ChatClient(Uri wsEndpoint, IPEndPoint udpEndpoint, Encoding encoding)
		{
			enc = encoding;
			_lock = new object();
			this.udpEndpoint = udpEndpoint;
			this.wsEndpoint = wsEndpoint;
			tokenSource = new CancellationTokenSource();
			
			_providers = new ConcurrentDictionary<uint, BufferedWaveProvider>();


			_decoder = new OpusDecoder(48000, 2);
			
			_audioIn = new WaveInEvent()
			{
				BufferMilliseconds = 100,
				WaveFormat = new WaveFormat(48000, 2),
			};
			_encoder = new OpusEncoder(48000, 2, OpusApplication.OPUS_APPLICATION_VOIP)
			{
				Bitrate = 522240
			};
			
			_audioIn.DataAvailable += AudioDataAvailable;
			
			_udp = new UdpClient()
			{
				EnableBroadcast = true,
			};
			_ws = new ClientWebSocket();
			_sampleProvider = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(_audioIn.WaveFormat.SampleRate, _audioIn.WaveFormat.Channels));
			_provider = new SampleToWaveProvider16(_sampleProvider);
			_audioOut = new WaveOutEvent()
			{ };
			
			_audioOut.Init(_provider);
			_frames = 960;
		}

		public ChatClient(Uri wsEndpoint, IPEndPoint udpEndpoint) : this(wsEndpoint, udpEndpoint, Encoding.Default) { }

		private async void AudioDataAvailable(object sender, WaveInEventArgs e)
		{
			
			short[] shorts = new short[e.BytesRecorded / 2];
			byte[] outBytes = new byte[_frames];
			Buffer.BlockCopy(e.Buffer, 0, shorts, 0, e.BytesRecorded);
			int size = _encoder.Encode(shorts, 0, _frames, outBytes, 0, _frames);
		

			var bitbytes = BitConverter.GetBytes(_voiceId);
			outBytes = bitbytes.Concat(SubArray(outBytes, 0, size)).ToArray();
	
			await _udp.SendAsync(outBytes, outBytes.Length);
		}

		public Uri WSUri => wsEndpoint;
		public IPEndPoint UDPEndpoint => udpEndpoint;

		public async Task StartAsync()
		{
			await _ws.ConnectAsync(wsEndpoint, CancellationToken.None);

			StartReceives();
			StartSends();

			await Task.Delay(-1);
		}
		private async void StartReceives()
		{
			while(true)
			{
				byte[] b = new byte[4096];
				await _ws.ReceiveAsync(new ArraySegment<byte>(b), CancellationToken.None);
				string t = enc.GetString(b).TrimEnd('\0');
				var packet = JsonConvert.DeserializeObject<MessageObject>(t);

				if(packet.EventType == "MESSAGE")
				{
					TextMessageObject message = ConvertToType<TextMessageObject>(packet.Value as JObject);
					Console.WriteLine($"<{message.ClientId}> {message.Content}");
				}
				else if(packet.EventType == "CONNECT")
				{
					ConnectObject message = ConvertToType<ConnectObject>(packet.Value as JObject);
					Console.WriteLine(message.Content);
					clientId = message.Id;
				}
				else if(packet.EventType == "VOICE_ACCEPT")
				{
					var message = ConvertToType<VoiceConnectionObject>(packet.Value as JObject);
					_voiceId = message.Id;
					Console.WriteLine(_voiceId);

					_udp.Connect(udpEndpoint);
					_audioIn.StartRecording();
					_audioOut.Play();

					_ = Task.Factory.StartNew(async () =>
					{
						while(true)
						{
							if(tokenSource.IsCancellationRequested)
							{
								break;
							}
							
							var bb = await _udp.ReceiveAsync();
							
							HandleBytes(bb.Buffer);
						}
					}, tokenSource.Token);
				}
			}
		}
		private async void StartSends()
		{
			while(true)
			{
				string text = Console.ReadLine().Replace("\n", "\r\n");
				if(text == "/connect")
				{
					byte[] send = Encoding.Default.GetBytes(JsonConvert.SerializeObject(new MessageObject("VOICE_CONNECT", null)));
					await _ws.SendAsync(new ArraySegment<byte>(send), WebSocketMessageType.Text, true, CancellationToken.None);
					continue;
				}
				var packet = new MessageObject("SEND", new STextMessageObject(text));
				text = JsonConvert.SerializeObject(packet);
				byte[] bytes = enc.GetBytes(text);
				await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}
		public void Dispose()
		{
			if(!disposed)
			{
				_udp.Dispose();
				_ws.Dispose();
				_audioIn.Dispose();
				_audioOut.Dispose();
				tokenSource.Dispose();
				
				disposed = true;
			}
		}
		
		public void HandleBytes(byte[] buffer)
		{
			try
			{
				var id = BitConverter.ToUInt32(SubArray(buffer, 0, 4), 0);
				

				var provider = _providers.GetOrAdd(id, xx =>
				{
					var temp = new BufferedWaveProvider(_audioIn.WaveFormat);
					_sampleProvider.AddMixerInput(temp);
					return temp;
				});

				short[] m = new short[_frames];
				int dd = _decoder.Decode(buffer, 3, buffer.Length - 4, m, 0, _frames);
				buffer = new byte[dd * 2];

				Buffer.BlockCopy(m, 0, buffer, 0, dd);
				if (!playing)
				{
					playing = true;
					_audioOut.Play();
				}
				provider.AddSamples(buffer, 0, buffer.Length);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}
		public static T[] SubArray<T>(T[] data, int index, int length)
		{
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
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
	}
}
