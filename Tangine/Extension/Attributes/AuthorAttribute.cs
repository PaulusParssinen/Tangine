﻿using Sulakore.Habbo;

namespace Tangine.Extension;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AuthorAttribute : Attribute
{
    public string Name { get; init; }

    public HHotel Hotel { get; set; }
    public string HabboName { get; set; }

    public string ResourceUrl { get; set; }
    public string ResourceName { get; set; }

    public AuthorAttribute(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}