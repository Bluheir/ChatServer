using ChatServer.ServerTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;


namespace ChatServer
{
	class Program
	{
		static void Main(string[] args)
		=> new Program().MainAsync().GetAwaiter().GetResult();

		private async Task MainAsync()
		{
			WebSocketListener server = new WebSocketListener(IPAddress.Parse("127.0.0.1"), 8080);
			server.OnReceive += Server_OnReceive;
			server.StartListening();
			await Task.Delay(-1);
		}

		private Task Server_OnReceive(WSClient arg1, string arg2, byte[] arg3)
		{
			Console.WriteLine(arg2);
			return Task.CompletedTask;
		}
	}
}
