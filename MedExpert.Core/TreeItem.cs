using System.Collections;
using System.Collections.Generic;

namespace MedExpert.Core
{
    public class TreeItem<T>
    {
        public T Item { get; set; }
        
        public IEnumerable<TreeItem<T>> Children { get; set; }
    }
}