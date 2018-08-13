using UnityEngine;

namespace Data
{
	[CreateAssetMenu]
	public class ProgramVersion : ScriptableObject
	{
		public int ProduceBytesPerSecond;
		public int LeakBytesPerSecond;
		public int MemorySize;
		public string Description;
		public int Price;
	}
}
