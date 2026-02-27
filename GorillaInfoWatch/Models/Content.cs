using GorillaInfoWatch.Models.Significance;
using System;
using System.Collections.ObjectModel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GorillaInfoWatch.Models;

public class Content
{
    public static Content Shared { get; internal set; }

    internal GameObject WatchPrefab;

    internal GameObject MenuPrefab;

    internal GameObject KeyboardPrefab;

    internal ReadOnlyCollection<FigureSignificance> Figures;

    internal ReadOnlyCollection<ItemSignificance> Cosmetics;

    public ReadOnlyDictionary<string, Symbol> Symbols;

    public ReadOnlyDictionary<Enum, Object> Sounds;
}
