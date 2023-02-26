namespace BB
{
    public static class Rand
    {
        public static void SetSeed(int seed)
		{
            UnityEngine.Random.InitState(seed);
        }
        public static float GetValue(int seed)
        {
            var state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);
            var result = Value;
            UnityEngine.Random.state = state;
            return result;
        }
        public static bool BoolValue => Value >= 0.5f;
        public static float Value => UnityEngine.Random.value;
        public static float Sign => BoolValue ? 1f : -1f;
        public static int GetInt(int minInclusive, int maxExclusive) => UnityEngine.Random.Range(minInclusive, maxExclusive);
        public static float GetFloat(float min, float max) => UnityEngine.Random.Range(min, max);

    }
}