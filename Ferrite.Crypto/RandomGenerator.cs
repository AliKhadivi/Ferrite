﻿/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Numerics;
using System.Security.Cryptography;

namespace Ferrite.Crypto
{
    public class RandomGenerator : IRandomGenerator
    {
        private int[] generatedPrimes;
        public RandomGenerator()
        {
            int rangeEnd = RandomNumberGenerator.GetInt32(int.MaxValue / 4 * 3, int.MaxValue);
            generatedPrimes = SieveOfEratosthenesSegmented(rangeEnd - 5000000, rangeEnd);   
        }

        public int GetRandomPrime()
        {
            int rnd = RandomNumberGenerator.GetInt32(generatedPrimes.Length);
            return generatedPrimes[rnd];
        }
        /// <summary>
        /// Implements the basic version of the sieve with the odds-only
        /// optimization.
        /// </summary>
        /// <param name="toInclusive"></param>
        /// <returns></returns>
        public static int[] SieveOfEratosthenes(int toInclusive)
        {
            if (toInclusive < 2)
            {
                return new int[] { };
            }
            if (toInclusive == 2)
            {
                return new int[] { 2 };
            }
            List<int> result = new();
            bool[] a = new bool[toInclusive + 1];
            
            for (int i = 3; i * i <= toInclusive; i += 2)
            {
                if (!a[i])
                {
                    for (int j = i * i; j >= 0 && j <= toInclusive; j += i)
                    {
                        a[j] = true;
                    }
                }
            }
            result.Add(2);
            for (int i = 3; i <= toInclusive; i += 2)
            {
                if (!a[i])
                {
                    result.Add(i);
                }
            }
            return result.ToArray();
        }
        /// <summary>
        /// Implements the segmented version of the sieve.
        /// </summary>
        /// <param name="fromInclusive"></param>
        /// <param name="toInclusive"></param>
        /// <returns></returns>
        public static int[] SieveOfEratosthenesSegmented(int fromInclusive, int toInclusive)
        {
            List<int> result = new();
            int segmentSize = (int)Math.Sqrt(toInclusive);
            bool[] a = new bool[segmentSize + 1];
            int[] basePrimes = SieveOfEratosthenes(segmentSize + 1);
            int firstSegment = fromInclusive / segmentSize;
            int lastSegment = toInclusive / segmentSize + 1;
            foreach (int p in basePrimes)
            {
                if (p >= fromInclusive && p <= toInclusive)
                {
                    result.Add(p);
                }
            }
            for (int i = firstSegment; i <= lastSegment; i++)
            {
                int segmentStart = i * segmentSize;
                int segmentEnd = (i + 1) * segmentSize;
                for (int j = 0; j <= segmentSize; j++)
                {
                    a[j] = true;
                }
                foreach (int p in basePrimes)
                {
                    int x = segmentStart / p;
                    x *= p;
                    if (x < segmentStart)
                    {
                        x += p;
                    }
                    for (int j = x; j >= segmentStart && j <= segmentEnd; j += p)
                    {
                        a[j - segmentStart] = false;
                    }
                }
                
                for (int j = 0; j < a.Length; j++)
                {
                    if (a[j] && segmentStart + j <= toInclusive
                        && segmentStart + j >= fromInclusive)
                    {
                        result.Add(segmentStart + j);
                    }
                }
            }
            return result.ToArray();
        }

        private static int ModPow(int x, int y, int m)
        {
            long result = 1;
            for (int i = 1; i <= y; i++)
            {
                result = (result * x) % m;
            }
            return (int)result;
        }
        private static int Pow(int x, int y)
        {
            int result = 1;
            for (int i = 1; i <= y; i++)
            {
                result = (result * x);
            }
            return result;
        }
        /// <summary>
        /// Implements the Miller-Rabin primality test algorithm from
        /// https://en.wikipedia.org/wiki/Miller-Rabin_primality_test
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static bool MillerRabin(int n)
        {
            if (n % 2 == 0 || n < 2)
            {
                return n == 2;
            }
            int d = n - 1;
            int r = 0;
            while(d % 2 == 0)
            {
                d /= 2;
                r++;
            }
            int[] a32 = new int[] { 2, 3, 5, 7 };
            foreach (int a in a32)
            {
                long x = ModPow(a, d, n);

                if (x == 1 || x == n - 1)
                {
                    continue;
                }
                for (int i2 = 0; i2 < r - 1; i2++)
                {
                    x = x * x % n;
                    if (x == n - 1)
                    {
                        break;
                    }
                }
                if (x != n - 1)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetRandomNumber(int toExclusive)
        {
            return RandomNumberGenerator.GetInt32(toExclusive);
        }

        public byte[] GetRandomBytes(int count)
        {
            return RandomNumberGenerator.GetBytes(count);
        }

        public int GetRandomNumber(int fromInclusive, int toExclusive)
        {
            return RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);
        }

        
        public BigInteger GetRandomInteger(BigInteger min, BigInteger max)
        {
            RandomNumberGenerator gen = RandomNumberGenerator.Create();
            return RandomInRange(gen, min, max);
        }

        // Implementation was taken from
        // https://stackoverflow.com/a/48855115/2015348
        private static BigInteger RandomInRange(RandomNumberGenerator rng, BigInteger min, BigInteger max)
        {
            if (min > max)
            {
                var buff = min;
                min = max;
                max = buff;
            }

            // offset to set min = 0
            BigInteger offset = -min;
            min = 0;
            max += offset;

            var value = randomInRangeFromZeroToPositive(rng, max) - offset;
            return value;
        }

        private static BigInteger randomInRangeFromZeroToPositive(RandomNumberGenerator rng, BigInteger max)
        {
            BigInteger value;
            var bytes = max.ToByteArray();

            // count how many bits of the most significant byte are 0
            // NOTE: sign bit is always 0 because `max` must always be positive
            byte zeroBitsMask = 0b00000000;

            var mostSignificantByte = bytes[bytes.Length - 1];

            // we try to set to 0 as many bits as there are in the most significant byte, starting from the left (most significant bits first)
            // NOTE: `i` starts from 7 because the sign bit is always 0
            for (var i = 7; i >= 0; i--)
            {
                // we keep iterating until we find the most significant non-0 bit
                if ((mostSignificantByte & (0b1 << i)) != 0)
                {
                    var zeroBits = 7 - i;
                    zeroBitsMask = (byte)(0b11111111 >> zeroBits);
                    break;
                }
            }

            do
            {
                rng.GetBytes(bytes);

                // set most significant bits to 0 (because `value > max` if any of these bits is 1)
                bytes[bytes.Length - 1] &= zeroBitsMask;

                value = new BigInteger(bytes);

                // `value > max` 50% of the times, in which case the fastest way to keep the distribution uniform is to try again
            } while (value > max);

            return value;
        }
    }
}

