using System;
using System.Collections.Generic;
using System.Text;

namespace NetAsyncSpider.Core.Untils
{
    public class AnonymousEqualityComparer<T> : IEqualityComparer<T>
    {
        //public delegate bool EqualsComparer<T>(T x, T y);
        private Func<T, T, bool> _equalsComparer;

        private Func<T, object> _hashComparer;
        //private EqualsComparer<T> _equalsComparer;

        public AnonymousEqualityComparer(Func<T, T, bool> equalsComparer, Func<T, object> hashComparer)
        {
            this._equalsComparer = equalsComparer;
            this._hashComparer = hashComparer;
        }

        public bool Equals(T x, T y)
        {
            if (null != this._equalsComparer)
                return this._equalsComparer(x, y);
            else
                return false;
        }

        public int GetHashCode(T obj)
        {
            if (obj != null && _hashComparer != null)
            {
                return _hashComparer(obj).GetHashCode();

            }
            else
            {
                return 0;
            }
        }
    }




    public class AnonymousComparer<T> : IComparer<T>
    {

        private Func<T, T, int> _Compare;
        /// <summary>
        /// 比较器
        /// </summary>
        /// <param name="compare">匿名func</param>
        public AnonymousComparer(Func<T, T, int> compare)
        {
            _Compare = compare;

        }

        public int Compare(T x, T y)
        {
            return _Compare(x, y);
        }

    }
}
