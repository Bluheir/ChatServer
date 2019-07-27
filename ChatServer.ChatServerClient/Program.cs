using System;
using System.Net;
using System.Text;

namespace ChatServer.ChatServerClient
{
	class Program
	{
		static void Main(string[] args)
		=> new ChatClient
		(
			new Uri("ws://localhost"),
			new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080),
			Encoding.UTF8
		).StartAsync().GetAwaiter().GetResult();
	}
}
