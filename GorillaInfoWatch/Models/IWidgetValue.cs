using System;

namespace GorillaInfoWatch.Models
{
    public interface IWidgetValue<T>
    {
        public object[] Parameters { get; set; }

        public Action<T, object[]> Command { get; set; }

        public T Value { get; set; }
    }
}