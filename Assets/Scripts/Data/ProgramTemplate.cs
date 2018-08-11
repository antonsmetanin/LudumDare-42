using UnityEngine;

namespace Data
{
	[CreateAssetMenu]
	public class ProgramTemplate : ScriptableObject
	{
		public int InitialSize;
		public Color Color;
		public Color32 TextColor;
		public string Name;
	}
}
