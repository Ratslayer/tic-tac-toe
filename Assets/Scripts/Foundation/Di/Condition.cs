using System;
using System.Collections.Generic;
using System.Linq;
namespace BB
{
    public delegate bool Condition();
    public static class ConditionMethods
    {
        public static Condition Inverse(this Condition condition)
        {
            return () => !condition();
        }
    }
}