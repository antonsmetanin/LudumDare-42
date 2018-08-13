using UnityEngine;

namespace Data
{
	[CreateAssetMenu]
	public class GameProgressTemplate : ScriptableObject
	{
		public int DataCollected;
		public int Wood;
		public ProgramTemplate[] AvailablePrograms;

		public int MaxData;
	}
}
