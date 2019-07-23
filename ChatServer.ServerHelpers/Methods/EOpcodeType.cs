namespace ChatServer.ServerHelpers.Methods
{
	public enum EOpcodeType
	{	
		Fragment = 0,
		Text = 1,
		Binary = 2,
		ClosedConnection = 8,
		Ping = 9,
		Pong = 10
	}
}
