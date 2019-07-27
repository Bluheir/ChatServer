#pragma warning disable IDE0032
#pragma warning disable IDE0044

using Newtonsoft.Json;

namespace ChatServer.Messaging
{
	public sealed class TextMessageObject
	{
		[JsonProperty("content")]
		private readonly string content;
		[JsonProperty("client_id")]
		private readonly int client_id;

		[JsonIgnore]
		public string Content => content;
		[JsonIgnore]
		public int ClientId => client_id;

		public TextMessageObject(string content, int clientId)
		{
			this.content = content;
			client_id = clientId;
		}
	}
}
