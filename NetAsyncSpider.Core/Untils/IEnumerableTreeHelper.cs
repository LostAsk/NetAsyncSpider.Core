using System;
using System.Collections.Generic;
using System.Text;

namespace NetAsyncSpider.Core.Untils
{
    public static class IEnumerableTreeHelper
    {
       public static IEnumerable<T> DepthFirstTravel<T>(T root, Func<T, IEnumerable<T>> getChildren)
        {

            if (getChildren == null)
            {
                throw new ArgumentNullException(nameof(getChildren));
            }

            var nodeStack = new Stack<T>();
            nodeStack.Push(root);
            while (nodeStack.Count != 0)
            {
                var node = nodeStack.Pop();
                foreach (var child in getChildren(node))
                {
                    nodeStack.Push(child);
                }

                yield return node; 
            }
        }
        public static IEnumerable<T> BreadthFirstTravel<T>(T root, Func<T, IEnumerable<T>> getChildren)
        {
            if (getChildren == null)
            {
                throw new ArgumentNullException(nameof(getChildren));
            }

            var nodeQueue = new Queue<T>();
            nodeQueue.Enqueue(root);
            while (nodeQueue.Count != 0)
            {
                T node = nodeQueue.Dequeue();
                foreach (var child in getChildren(node))
                {
                    nodeQueue.Enqueue(child);
                }

                yield return node;
            }
        }

    }
}
