#pragma warning disable IDE0032
#pragma warning disable IDE0044

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatServer.Messaging
{
	public sealed class MessageObject
	{
		[JsonProperty("value")]
		private object value;
		[JsonProperty("event_type")]
		private string event_type;

		[JsonIgnore]
		public object Value => value;
		[JsonIgnore]
		public string EventType => event_type;

		public MessageObject(string eventType, object value)
		{
			event_type = eventType;
			this.value = value;
		}
	}
}
