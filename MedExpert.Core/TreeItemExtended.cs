namespace MedExpert.Core
{
    public class TreeItemExtended<T, TExtended>:TreeItem<T>
    {
        public TExtended ExtendedProperty { get; set; }
    }
}