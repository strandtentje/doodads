using System;
using System.Collections.Generic;
using System.Linq;

namespace Ziewaar.RAD.Doodads.Data.MemoryStore;

internal class StringArrayComparer(StringComparer baseComparer) : IComparer<string[]>
{
    public int Compare(string[] x, string[] y)
    {
        int position = 0;
        while (true)
        {
            var xElement = x.ElementAtOrDefault(position);
            var yElement = y.ElementAtOrDefault(position);
            if (xElement != null && yElement != null)
            {
                var comparison = baseComparer.Compare(xElement, yElement);
                if (comparison == 0)
                    position++;
                else
                    return comparison;
            }
            else if (xElement != null && yElement == null)
            {
                return -1;
            }
            else if (xElement == null && yElement != null)
            {
                return 1;
            } else
            {
                return 0;
            }
        }
    }
}