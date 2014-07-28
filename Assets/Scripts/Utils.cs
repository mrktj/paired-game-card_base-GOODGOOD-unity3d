using System;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts
{
    public class Utils
    {
        private static readonly Random Random = new Random();

        public static void Shuffle<T>(T[] array)
        {
            var random = Random;
            for (var i = array.Length; i > 1; i--)
            {
                var j = random.Next(i);
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }
        }
         public static bool NearlyEqual(float a, float b, float epsilon) {
            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);

            if (a == b) // shortcut, handles infinities
            { 
                return true;
            } 
            else if (a == 0 || b == 0 || diff < float.MinValue)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * float.MinValue);
            } 
            else // use relative error
            { 
                return diff / (absA + absB) < epsilon;
            }
        }
    }
}
