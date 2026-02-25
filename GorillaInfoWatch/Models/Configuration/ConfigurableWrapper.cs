using BepInEx.Configuration;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.Widgets;
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
    public abstract AcceptableValueBase AcceptableValues { get; }

    public abstract object BoxedValue { get; set; }
    public abstract string SerializedValue { get; set; }

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

        string value = SerializedValue;

        List<Widget_Base> widgets = [];

        Action postDefineFunction = null;

        AcceptableValueBase acceptableValues = AcceptableValues;

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
            if (acceptableValues is not AcceptableValueRange<short> shortRange) goto WriteLines;

            widgets.AddRange(CreateIncrementalButtons(increment =>
            {
                short value = (short)shortRange.Clamp((short)TomlTypeConverter.ConvertToValue(SerializedValue, SettingType) + increment);
                BoxedValue = value;
            }));

            goto WriteLines;
        }

        if (settingType == typeof(int))
        {
            if (acceptableValues is not AcceptableValueRange<int> intRange) goto WriteLines;

            widgets.AddRange(CreateIncrementalButtons(increment =>
            {
                int value = (int)intRange.Clamp((int)TomlTypeConverter.ConvertToValue(SerializedValue, SettingType) + increment);
                BoxedValue = value;
            }));

            goto WriteLines;
        }

        if (settingType == typeof(long))
        {
            if (acceptableValues is not AcceptableValueRange<long> longRange) goto WriteLines;

            widgets.AddRange(CreateIncrementalButtons(increment =>
            {
                long value = (long)longRange.Clamp((long)TomlTypeConverter.ConvertToValue(SerializedValue, SettingType) + increment);
                BoxedValue = value;
            }));

            goto WriteLines;
        }

        if (settingType == typeof(bool))
        {
            value = value.ToTitleCase();

            widgets.Add(new Widget_Switch(SerializedValue.ToLower() == "true", (bool value) =>
            {
                SerializedValue = value.ToString();
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

// woaw...
internal class ConfigurableWrapper_Woaw<T>(string key, string description, Func<T> getter, Action<T> setter) : ConfigurableWrapper
{
    public override Type SettingType => typeof(T);
    public override string Entry => key;
    public override string Description => description;
    public override AcceptableValueBase AcceptableValues => null; // TODO: implement custom AcceptableValues

    public override object BoxedValue
    {
        get => getter();
        set => setter((T)value);
    }
    public override string SerializedValue
    {
        get => TomlTypeConverter.ConvertToString(BoxedValue, SettingType);
        set => TomlTypeConverter.ConvertToValue(value, SettingType);
    }
}

internal class ConfigurableWrapper_BepInEntry(ConfigEntryBase entryBase) : ConfigurableWrapper
{
    public ConfigEntryBase EntryBase = entryBase;

    public override Type SettingType => EntryBase.SettingType;
    public override string Entry => EntryBase.Definition.Key;
    public override string Description => EntryBase.Description.Description;
    public override AcceptableValueBase AcceptableValues => EntryBase.Description.AcceptableValues;

    public override object BoxedValue
    {
        get => EntryBase.BoxedValue;
        set
        {
            EntryBase.BoxedValue = value;
            if (!EntryBase.ConfigFile.SaveOnConfigSet) EntryBase.ConfigFile.Save();
        }
    }

    public override string SerializedValue
    {
        get => EntryBase.GetSerializedValue();
        set
        {
            EntryBase.SetSerializedValue(value);
            if (!EntryBase.ConfigFile.SaveOnConfigSet) EntryBase.ConfigFile.Save();
        }
    }

    public override string UnsupportedText => $"This setting can be edited at {Path.GetFileName(EntryBase.ConfigFile.ConfigFilePath)}";
}