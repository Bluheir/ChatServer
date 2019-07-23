using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer.ServerHelpers.Methods
{
	public struct SFrameMaskData
	{
		public int DataLength, KeyIndex, TotalLenght;
		public EOpcodeType Opcode;

		public SFrameMaskData(int DataLength, int KeyIndex, int TotalLenght, EOpcodeType Opcode)
		{
			this.DataLength = DataLength;
			this.KeyIndex = KeyIndex;
			this.TotalLenght = TotalLenght;
			this.Opcode = Opcode;
		}
	}
}
