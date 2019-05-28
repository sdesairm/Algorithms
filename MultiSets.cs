using System;
using System.Collections.Generic;
using System.Text;

namespace SDesaiRM.Algorithms
{
    public class MultiSets<T>
    {
        public static IEnumerable<List<T>> GetCombos(T[] A)
        {
            return printCombos(A, null, A.Length - 3);
        }
        static IEnumerable<List<T>> printCombos(T[] A, IEnumerable<List<T>> pfix, int startIndex)
        {
            if (pfix == null)
            {
                pfix = new List<List<T>>();
                pfix = new[]
                {
                    new List<T>() {A[A.Length - 2]},
                    new List<T>() {A[A.Length - 2], A[A.Length - 1]},
                    new List<T>() {A[A.Length - 1]}
                };
            }

            if (startIndex == 0)
                return pfixCombos(pfix, A[startIndex]);
            for (int i = startIndex; i >= 0; i--)
            {
                var v = pfixCombos(pfix, A[i]);
                return printCombos(A, v, startIndex - 1);
            }

            return new List<List<T>>();
        }

        static IEnumerable<List<T>> pfixCombos(IEnumerable<List<T>> combos, T p)
        {
            yield return new List<T>() {p};
            foreach (var v in combos)
            {
                var rtn = new List<T>() {p};
                    rtn.AddRange(v);
                yield return rtn;
            }
            
            foreach (var v in combos)
            {
                yield return v;
            }
        }
    }
}
