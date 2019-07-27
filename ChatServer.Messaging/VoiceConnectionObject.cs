using Newtonsoft.Json;


namespace ChatServer.Messaging
{
	public class VoiceConnectionObject
	{
		[JsonProperty("id")]
		private uint id;

		[JsonIgnore]
		public uint Id => id;

		public VoiceConnectionObject(uint id)
		{
			this.id = id;
		}
	}
}
