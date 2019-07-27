using Newtonsoft.Json;

namespace ChatServer.Messaging
{
	public sealed class ConnectObject
	{
		[JsonProperty("content")]
		private string content;
		[JsonProperty("id")]
		private int id;

		[JsonIgnore]
		public string Content => content;
		[JsonIgnore]
		public int Id => id;

		public ConnectObject(string content, int id)
		{
			this.content = content;
			this.id = id;
		}
	}
}
