using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MedExpert.Core.Helpers
{
    public static class TreeHelper
    {
        public static IList<TreeItem<TEntity>> GenerateTree<TEntity, TId>(
            this IList<TEntity> collection,
            Func<TEntity, TId> idSelector,
            Func<TEntity, TId> parentIdSelector,
            TId rootId = default)
        {
            return collection.Where(c => EqualityComparer<TId>.Default.Equals(parentIdSelector(c), rootId)).Select(c => new TreeItem<TEntity>
            {
                Item = c,
                Children = collection.GenerateTree(idSelector, parentIdSelector, idSelector(c))
            }).ToList();
        }
        //
        // public static IList<TreeItem<TResult>> ConvertTree<TEntity, TResult>(
        //     this IEnumerable<TreeItem<TEntity>> tree,
        //     Func<TEntity, TResult> converter)
        // {
        //     return tree.Select(p => new TreeItem<TResult>{ Item = converter(p.Item), Children = p.Children.ConvertTree<TEntity, TResult>()})
        // }

        public static IList<TreeItem<TEntity>> GetMatched<TEntity, TMatchItem>(
            this IEnumerable<TreeItem<TEntity>> tree, Func<TEntity, TMatchItem> matcher, HashSet<TMatchItem> itemsToMatch)
        {
            return tree
                .Where(p => itemsToMatch.Contains(matcher(p.Item)))
                .Select(p => new TreeItem<TEntity>
                {
                    Item = p.Item,
                    Children = !p.Children.Any() 
                        ? null : 
                        p.Children.GetMatched(matcher, itemsToMatch)
                })
                .Where(x => x.Children == null || x.Children.Any())
                .ToList();
        }

        public static IList<TreeItem<TResult>> VisitAndConvert<TEntity, TResult>(this IEnumerable<TreeItem<TEntity>> tree,
            Func<TEntity, TResult> converter)
        {
            return tree?.Select(p => new TreeItem<TResult>
            {
                Item = converter(p.Item), 
                Children = VisitAndConvert(p.Children, converter)
            }).ToList();
        }
    }
}