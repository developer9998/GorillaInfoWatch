using GorillaInfoWatch.Models;
using System;
using System.Collections.Generic;

namespace GorillaInfoWatch.Interfaces
{
    public interface IPage
    {
        object[] Parameters { get; set; }

        List<object> Lines { get; set; }

        HeaderLine Header { get; set; }

        Type CallerPage { get; set; }

        event Action<HeaderLine> OnSetHeaderRequest;

        event Action<object[]> OnSetLinesRequest, OnUpdateLinesRequest;

        event Action<Type, Type, object[]> OnPageSwitchRequest;

        void OnDisplay();
        void OnClose();
    }
}
