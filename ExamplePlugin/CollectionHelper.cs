using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RiskOfCodePlugin
{
    internal static class CollectionHelper
    {
        public static List<List<T>> SplitIntoChunks<T>(this List<T> list, int numberOfChunks)
        {
            if (numberOfChunks <= 0)
                throw new ArgumentException("Number of chunks must be greater than zero.");

            var chunks = new List<List<T>>();
            int totalItems = list.Count;

            // Calculate the base size of each chunk
            int baseChunkSize = totalItems / numberOfChunks;
            // Calculate how many chunks need to be larger by one item
            int remainder = totalItems % numberOfChunks;

            int start = 0;
            for (int i = 0; i < numberOfChunks; i++)
            {
                int chunkSize = baseChunkSize + (i < remainder ? 1 : 0);
                var chunk = list.GetRange(start, chunkSize);
                chunks.Add(chunk);
                start += chunkSize;
            }

            return chunks;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
