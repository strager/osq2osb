using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace osq {
    public static class MyExtensions {
        public static void AddMany<T>(this ICollection<T> self, IEnumerable<T> other) {
            foreach(var otherItem in other) {
                self.Add(otherItem);
            }
        }
    }
}
