using System;
using UnityEngine;

namespace Empress.UI {

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class VisualElementPickerAttribute : PropertyAttribute {

        public Type TargetType { get; set; }

        public bool SkipEmpty { get; set; }

        public VisualElementPickerAttribute(Type targetType = null, bool skipEmpty = false) {
            TargetType = targetType;
            SkipEmpty = skipEmpty;
        }
    }
}
