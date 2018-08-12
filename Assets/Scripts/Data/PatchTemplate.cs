using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class PatchTemplate : ScriptableObject
    {
        public int SizeDelta;
        public int LeakDelta;
    }
}
