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

        public static void AddChild(GameObject parent, GameObject child)
        {
            if (child != null && parent != null)
            {
                var t = child.transform;
                t.parent = parent.transform;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                child.layer = parent.layer;
            }
        }
    }
}
