using System;

namespace GorillaInfoWatch.Models.Widgets
{
    public interface IWidgetValue<T>
    {
        public T Value { get; set; }
        public object[] Parameters { get; set; }
        public Action<T, object[]> Command { get; set; }
    }
}