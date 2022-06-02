export interface TreeItem<T> {
  item: T;
  children: TreeItem<T>[];
}
