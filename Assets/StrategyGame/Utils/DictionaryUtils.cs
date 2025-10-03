using System.Collections.Generic;
using System.Linq;

namespace StrategyGame.Utils {
    public static class DictionaryUtils {
        public static string FormatDictionary<K,V>(Dictionary<K,V> dictionary) {
            return string.Join(", ", dictionary.Select(pair => $"{pair.Key}={pair.Value}"));
        }
    }
}
