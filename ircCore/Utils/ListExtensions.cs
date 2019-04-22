/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;
using System.Linq;

namespace ircCore.Utils
{
    public interface ICloneList<out T>
    {
        T Clone();
    }

    public static class ListExtensions
    {
        public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneList<T>
        {
            return listToClone.Select(item => item.Clone()).ToList();
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            var tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}
