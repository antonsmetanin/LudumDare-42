using UnityEngine;

namespace Data
{
	[CreateAssetMenu]
	public class ProgramVersion : ScriptableObject
	{
		public int ProduceSpeed;
		public int LeakSpeed;
		public int Size;
		public string Description;
		public int Price;
	}
}
