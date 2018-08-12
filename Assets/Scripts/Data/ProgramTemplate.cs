using UnityEngine;

namespace Data
{
	[CreateAssetMenu]
	public class ProgramTemplate : ScriptableObject
	{
		public ProgramVersion[] Versions;
		public PatchTemplate[] AvailablePatches;
		public Color Color;
		public Color32 TextColor;
		public string Name;
	}
}
