using System;
using System.Collections.Generic;
using System.Linq;

namespace Kekcell
{
    static class MathFunctions
    {
        public static double Median(double[] arr)
        {
            Array.Sort(arr);
            if (arr.Length % 2 == 0)
                return (arr[(arr.Length - 1) / 2] + arr[arr.Length / 2]) / 2;
            return arr[arr.Length / 2];
        }

        public static double Mode(double[] arr)
        {
            double mode = arr[0];
            int maxCount = 0;
            Dictionary<double, int> counter = new();
            foreach (double value in arr)
            {
                if (counter.ContainsKey(value))
                {
                    int count = ++counter[value];
                    if (count > maxCount)
                    {
                        mode = value;
                        maxCount = count;
                    }
                }
                else
                {
                    counter.Add(value, 0);
                }
            }

            return mode;
        }

        public static double Sum(double[] arr)
        {
            double result = arr.Sum();
            if (double.IsInfinity(result))
                throw new OverflowException();
            return result;
        }

        public static double Product(double[] arr)
        {
            double result = arr.Aggregate((x, y) => x * y);
            if (double.IsInfinity(result))
                throw new OverflowException();
            return result;
        }
    }
}
