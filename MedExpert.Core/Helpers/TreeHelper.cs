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
                    Children = !p.Children?.Any() ?? true 
                        ? null : 
                        p.Children.GetMatched(matcher, itemsToMatch)
                })
                //.Where(x => x.Children == null || x.Children.Any())
                .ToList();
        }

        //TODO: test it
        public static IList<TreeItem<TEntity>> GetMatchedBranch<TEntity, TMatchItem>(
            this IEnumerable<TreeItem<TEntity>> tree, Func<IEnumerable<TEntity>, TMatchItem> matcher,
            HashSet<TMatchItem> itemsToMatch)
        {
            return tree.CollectBranches(matcher, itemsToMatch).VisitAndConvert(x => x);
        }

        private static IList<TreeItemExtended<TEntity, KeyValuePair<IEnumerable<TEntity>, TMatchItem>>> CollectBranches<TEntity, TMatchItem>(
            this IEnumerable<TreeItem<TEntity>> tree, Func<IEnumerable<TEntity>, TMatchItem> matcher, HashSet<TMatchItem> itemsToMatch)
        {
            return tree
                .Select(p => new TreeItemExtended<TEntity, KeyValuePair<IEnumerable<TEntity>, TMatchItem>>
                {
                    Item = p.Item,
                    ExtendedProperty = p is TreeItemExtended<TEntity, KeyValuePair<IEnumerable<TEntity>, TMatchItem>> pExtended
                        ? pExtended.ExtendedProperty
                        : new KeyValuePair<IEnumerable<TEntity>, TMatchItem>(
                            Enumerable.Repeat(p.Item, 1),
                            default),
                    Children = !p.Children?.Any() ?? true
                        ? null
                        : p.Children.Select(c =>
                        {
                            var cResult =
                                new TreeItemExtended<TEntity, KeyValuePair<IEnumerable<TEntity>, TMatchItem>>
                                {
                                    Item = c.Item,
                                    Children = c.Children,
                                    ExtendedProperty = new KeyValuePair<IEnumerable<TEntity>, TMatchItem>(
                                        p is TreeItemExtended<TEntity, KeyValuePair<IEnumerable<TEntity>, TMatchItem>> p2Extended
                                            ? p2Extended.ExtendedProperty.Key.Append(c.Item)
                                            : Enumerable.Repeat(p.Item, 1).Append(c.Item),
                                        default)
                                };

                            if (cResult.Children == null || !cResult.Children.Any())
                            {
                                cResult.ExtendedProperty = new KeyValuePair<IEnumerable<TEntity>, TMatchItem>(
                                    cResult.ExtendedProperty.Key,
                                    matcher(cResult.ExtendedProperty.Key));
                            }

                            return cResult;
                        }).CollectBranches(matcher, itemsToMatch)
                })
                .Where(p => 
                    EqualityComparer<TMatchItem>.Default.Equals(p.ExtendedProperty.Value, default) || 
                    itemsToMatch.Contains(p.ExtendedProperty.Value))
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

        public static IList<TreeItem<TResult>> VisitAndConvert<TEntity, TResult>(this IEnumerable<TreeItem<TEntity>> tree,
            Func<TEntity, IEnumerable<TEntity>, TResult> converter)
        {
            return tree?.Select(p => new TreeItem<TResult>
            {
                Children = VisitAndConvert(p.Children, converter),
                Item = converter(p.Item, p.Children?.Select(x => x.Item))
            }).ToList();
        }

        public static IList<TEntity> MakeFlat<TEntity>(this IEnumerable<TreeItem<TEntity>> tree)
        {
            return tree?.SelectMany(p => (p.Children ?? Enumerable.Empty<TreeItem<TEntity>>())
                    .MakeFlat()
                    .Prepend(p.Item))
                .Where(x => x != null)
                .ToList();
        }

        public static IList<TreeItem<TEntity>> VisitAndSort<TEntity, TResult>(
            this IEnumerable<TreeItem<TEntity>> tree,
            Func<TEntity, TResult> sortFunc, bool sortByAsc)
        {
            var result = tree?.Select(p => new TreeItem<TEntity>
            {
                Item = p.Item,
                Children = VisitAndSort(p.Children, sortFunc, sortByAsc)
            });

            return result == null
                ? null
                : sortByAsc
                    ? result.OrderBy(x => sortFunc(x.Item)).ToList()
                    : result.OrderByDescending(x => sortFunc(x.Item)).ToList();
        }
    }
}