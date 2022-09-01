public static class Random_Extension
{
    public static void Shuffle<T>(this System.Random rng, params T[] array) {
        int n = array.Length;
        while (n > 1) {
            int k = rng.Next(n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }

    public static void Shuffle<T>(this System.Random rng, int length, params T[] array) {
        int n = length;
        while (n > 1) {
            int k = rng.Next(n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }

    public static bool ShuffleNotAdjacentRepeat<T>(this System.Random rng, params T[] array) where T : struct {
        int n = array.Length;
        T last = default;
        while (n >= 1) {
            int k = rng.Next(n--);

            int conflictTry = array.Length;
            while (!last.Equals(default(T)) && last.Equals(array[k])) {
                if (conflictTry-- < 0)
                    return false;

                k = rng.Next(n);
            }

            (array[n], array[k]) = (array[k], array[n]);
            last = array[n];
        }

        if (array.Length > 1 && array[0].Equals(array[array.Length - 1]))
            return false;

        return true;
    }

}