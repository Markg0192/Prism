using System.Collections.Generic;

namespace Prism
{
    public class SteelComponentComparer : IEqualityComparer<MyAssembly>
    {
        public bool Equals(MyAssembly x, MyAssembly y)
        {
            return x.Name == y.Name && x.Length == y.Length;
        }

        public int GetHashCode(MyAssembly obj)
        {
            return obj.Name.GetHashCode() ^ obj.Length.GetHashCode();
        }
    }
}
