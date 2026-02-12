using System.Collections.Generic;

namespace StrategyGame.Utils {
    public static class ListUtils {
        public static void Swap<T>(List<T> list, int i, int j)
        {
            (list[i], list[j]) = (list[j], list[i]);
        }

    }
}
