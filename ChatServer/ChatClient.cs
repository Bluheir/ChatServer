using ChatServer.ServerHelpers;

namespace ChatServer
{
	public class ChatClient
	{
		public WSClient Client { get; }
		public int ClientId { get; }

		public ChatClient(WSClient client, int clientId)
		{
			Client = client;
			ClientId = clientId;
		}
	}
}
