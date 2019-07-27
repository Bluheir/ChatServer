using Newtonsoft.Json;

namespace ChatServer.Messaging
{
	public sealed class STextMessageObject
	{
		[JsonProperty("content")]
		private readonly string content;
		[JsonIgnore]
		public string Content => content;

		public STextMessageObject(string content)
		{
			this.content = content;
		}
	}
}
