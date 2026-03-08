using GorillaInfoWatch.Models.Widgets;
using GorillaLibrary.Extensions;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GorillaInfoWatch.Models.Configuration;

internal abstract class ConfigurableWrapper
{
    public abstract Type SettingType { get; }
    public abstract string Entry { get; }
    public abstract string Description { get; }
    public abstract ValueValidator AcceptableValues { get; }

    public abstract object BoxedValue { get; set; }

    public virtual string UnsupportedText => "This setting does not support editing here";

    private readonly string _descriptionFormat = "<line-height=45%>{0}<br><size=60%>{1}";

    public virtual void WriteLines(LineBuilder lines)
    {
        Type settingType = SettingType;

        string typeName = settingType.IsEnum ? "Enumeration" : settingType.Name switch
        {
            "String" => "Text",
            "Int16" or "Int32" or "Int64" => "Integer",
            "UInt16" or "UInt32" or "UInt64" => "Unsigned Integer",
            "Byte" => "Byte",
            "SByte" => "Signed Byte",
            "Single" or "Double" or "Decimal" => "Floating Point",
            "Char" => "Character",
            _ => settingType.Name
        };

        string value = BoxedValue.ToString();

        List<Widget_Base> widgets = [];

        Action postDefineFunction = null;

        ValueValidator acceptableValues = AcceptableValues;

        List<Widget_Base> CreateIncrementalButtons(Action<int> action)
        {
            List<Widget_Base> widgets = [];

            void InvokeCustomAction(object[] parameters)
            {
                action.Invoke((int)parameters[0]);
                InfoScreen.LoadedScreen?.SetText();
            }

            widgets.Add(new Widget_PushButton(InvokeCustomAction, -1)
            {
                Colour = ColourPalette.Red,
                Symbol = new Symbol(Content.Shared.Symbols["Minus"])
                {
                    Colour = Color.black
                }
            });

            widgets.Add(new Widget_PushButton(InvokeCustomAction, 1)
            {
                Colour = ColourPalette.Green,
                Symbol = new Symbol(Content.Shared.Symbols["Plus"])
                {
                    Colour = Color.black
                }
            });

            return widgets;
        }

        if (settingType == typeof(short))
        {
            if (acceptableValues is not ValueRange<short> shortRange) goto WriteLines;

            widgets.AddRange(CreateIncrementalButtons(increment =>
            {
                short value = (short)shortRange.EnsureValid(Convert.ToInt16(BoxedValue) + increment);
                BoxedValue = value;
            }));

            goto WriteLines;
        }

        if (settingType == typeof(int))
        {
            if (acceptableValues is not ValueRange<int> intRange) goto WriteLines;

            widgets.AddRange(CreateIncrementalButtons(increment =>
            {
                int value = (int)intRange.EnsureValid(Convert.ToInt32(BoxedValue) + increment);
                BoxedValue = value;
            }));

            goto WriteLines;
        }

        if (settingType == typeof(long))
        {
            if (acceptableValues is not ValueRange<long> longRange) goto WriteLines;

            widgets.AddRange(CreateIncrementalButtons(increment =>
            {
                long value = (long)longRange.EnsureValid(Convert.ToInt64(BoxedValue) + increment);
                BoxedValue = value;
            }));

            goto WriteLines;
        }

        if (settingType == typeof(bool))
        {
            value = value.ToTitleCase();

            widgets.Add(new Widget_Switch(BoxedValue.ToString().ToLower() == "true", (bool value) =>
            {
                BoxedValue = value;
                InfoScreen.LoadedScreen?.SetText();
            }));

            goto WriteLines;
        }

        if (settingType.IsEnum)
        {
            if (settingType.GetCustomAttributes(typeof(FlagsAttribute), true).Any())
            {
                IEnumerable<Enum> flags = Enum.GetValues(settingType).Cast<Enum>()
                .Where(flag =>
                {
                    ulong flagLong = Convert.ToUInt64(flag);
                    return flagLong != 0 && (flagLong & (flagLong - 1)) == 0;
                });

                ulong valueLong = Convert.ToUInt64(BoxedValue);

                LineBuilder flagLines = new();

                foreach (Enum flag in flags)
                {
                    ulong flagLong = Convert.ToUInt64(flag);
                    bool hasFlag = (valueLong & flagLong) == flagLong;

                    MemberInfo memberInfo = SettingType.GetMember(flag.ToString())?.FirstOrDefault(m => m.DeclaringType == SettingType);
                    DescriptionAttribute description = (DescriptionAttribute)memberInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)?.FirstOrDefault();

                    flagLines.Add(description != null ? string.Format(_descriptionFormat, flag.ToString(), description.Description) : flag.ToString(), new Widget_PushButton(() =>
                    {
                        valueLong = Convert.ToUInt64(BoxedValue);
                        ulong result = valueLong ^ flagLong;
                        BoxedValue = Enum.ToObject(settingType, result);
                        InfoScreen.LoadedScreen?.SetContent();
                    })
                    {
                        Colour = hasFlag ? ColourPalette.Green : ColourPalette.Red,
                        Symbol = Content.Shared.Symbols[hasFlag ? "Flag Green" : "Flag Red"],
                        Alignment = WidgetAlignment.Left
                    });
                }

                postDefineFunction = delegate ()
                {
                    flagLines.Lines.ForEach(line => lines.Add(line.Text, widgets: line.Widgets));
                };

                goto WriteLines;
            }

            string[] names = Enum.GetNames(settingType);
            if (names.Length < 2) goto WriteLines;

            if (names.Length == 2)
            {
                Enum[] values = [.. Enum.GetValues(settingType).Cast<Enum>().OrderBy(Convert.ToInt64)];
                Enum falseValue = values.First();
                Enum trueValue = values.Last();

                widgets.Add(new Widget_Switch((Convert.ToInt64(BoxedValue) & Convert.ToInt64(trueValue)) == Convert.ToInt64(trueValue), value =>
                {
                    BoxedValue = value ? trueValue : falseValue;
                    InfoScreen.LoadedScreen?.SetText();
                }));

                goto WriteLines;
            }

            widgets.AddRange(CreateIncrementalButtons(increment =>
            {
                BoxedValue = Enum.ToObject(settingType, Mathf.Min(Enum.GetValues(SettingType).Length - 1, Mathf.Max(0, Convert.ToInt32(BoxedValue) + increment)));
            }));

            goto WriteLines;
        }

    WriteLines:

        lines.Add($"{Entry}: {value}", widgets: widgets);

        postDefineFunction?.Invoke();
        if (widgets.Count == 0 && postDefineFunction == null) lines.BeginCentre().AppendColour(UnsupportedText, Color.red).EndAlign().AppendLine();

        lines.Skip().Add($"Description: {Description}", LineOptions.Wrapping);

        lines.Skip().Add($"Type: {typeName}");
    }
}

// formerly ConfigurableWrapper_Waow... i miss her.

internal class ConfigurableWrapper_Custom<T>(string key, string description, Func<T> getter, Action<T> setter) : ConfigurableWrapper
{
    public override Type SettingType => typeof(T);
    public override string Entry => key;
    public override string Description => description;
    public override ValueValidator AcceptableValues => null; // TODO: implement custom AcceptableValues

    public override object BoxedValue
    {
        get => getter();
        set => setter((T)value);
    }
}

internal class ConfigurableWrapper_ML(MelonPreferences_Entry entryBase) : ConfigurableWrapper
{
    public MelonPreferences_Entry EntryBase = entryBase;

    public override Type SettingType => EntryBase.GetReflectedType();
    public override string Entry => EntryBase.DisplayName;
    public override string Description => EntryBase.Description;
    public override ValueValidator AcceptableValues => EntryBase.Validator;

    public override object BoxedValue
    {
        get => EntryBase.BoxedValue;
        set
        {
            EntryBase.BoxedValue = value;
            EntryBase.Category.SaveToFile();
        }
    }

    public override string UnsupportedText
    {
        get
        {
            object file = AccessTools.Field(EntryBase.Category.GetType(), "File").GetValue(EntryBase.Category);
            string path = (string)AccessTools.Field(file.GetType(), "FilePath").GetValue(file);
            string name = Path.GetFileName(path);
            return $"This preference may be edited at {name}";
        }
    }
}