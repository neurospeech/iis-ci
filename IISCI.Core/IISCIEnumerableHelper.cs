using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISCI
{
    public static class IISCIEnumerableHelper
    {

        public static IEnumerable<IEnumerable<T>> Slice<T>(this IEnumerable<T> input, int size)
        {
            while (input.Any()) {
                yield return input.Take(size);
                input = input.Skip(size);
            }
        }

    }
}
