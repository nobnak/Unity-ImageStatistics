using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Unity_ImageStatistics {

    public static class Utils {

        public static string DebugString<T>(this IList<T> l){
            var tmp = new StringBuilder($"<{l.GetType().Name} [");
            for (var i = 0; i < l.Count; i++)
                tmp.Append($"{i}:{l[i]}, ");
            tmp.Append($"]>");
            return tmp.ToString();
        }
    }
}
