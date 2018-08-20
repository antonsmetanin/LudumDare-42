using UnityEngine;

namespace Data
{
	[CreateAssetMenu]
	public class RobotTemplate : ScriptableObject
	{
		public int InitialMemorySize;
		public int MemoryUpgradeSize;
        public int UploadSpeedBytesPerSecond;
	}
}
