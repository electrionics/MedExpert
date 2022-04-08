using System;
using System.Collections.Generic;
using System.Linq;

namespace MedExpert.Core.Helpers
{
    public static class TreeHelper
    {
        public static IEnumerable<TreeItem<TEntity>> GenerateTree<TEntity, TId>(
            this IList<TEntity> collection,
            Func<TEntity, TId> idSelector,
            Func<TEntity, TId> parentIdSelector,
            TId rootId = default)
        {
            foreach (var c in collection.Where(c => EqualityComparer<TId>.Default.Equals(parentIdSelector(c), rootId)))
            {
                yield return new TreeItem<TEntity>
                {
                    Item = c,
                    Children = collection.GenerateTree(idSelector, parentIdSelector, idSelector(c))
                };
            }
        }
    }
}