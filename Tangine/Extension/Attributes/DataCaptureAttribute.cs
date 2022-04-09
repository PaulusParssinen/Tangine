﻿using System.Reflection;

using Sulakore.Habbo;
using Sulakore.Network;
using Sulakore.Network.Buffers;

namespace Tangine.Extension;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class DataCaptureAttribute : Attribute, IEquatable<DataCaptureAttribute>
{
    public short Id { get; init; }
    public uint Hash { get; init; }
    public bool IsOutgoing { get; init; }

    internal object Target { get; set; }
    internal MethodInfo Method { get; set; }

    public static bool operator !=(DataCaptureAttribute left, DataCaptureAttribute right) => !(left == right);
    public static bool operator ==(DataCaptureAttribute left, DataCaptureAttribute right) => EqualityComparer<DataCaptureAttribute>.Default.Equals(left, right);

    public DataCaptureAttribute(short id, bool isOutgoing)
    {
        Id = id;
        IsOutgoing = isOutgoing;
    }
    public DataCaptureAttribute(uint hash, bool isOutgoing)
    {
        Hash = hash;
        IsOutgoing = isOutgoing;
    }

    internal void Invoke(DataInterceptedEventArgs args)
    {
        object[] parameters = CreateValues(args);
        object result = Method?.Invoke(Target, parameters);
        switch (result)
        {
            case bool isBlocked:
            {
                args.IsBlocked = isBlocked;
                break;
            }
        }
    }
    private object[] CreateValues(DataInterceptedEventArgs args)
    {
        ParameterInfo[] parameters = Method.GetParameters();
        var values = new object[parameters.Length];

        HPacketReader packetIn = args.Packet.AsReader();
        for (int i = 0; i < values.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            if (parameter.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                values[i] = args.Packet.Id;
            }
            values[i] = Type.GetTypeCode(parameter.ParameterType) switch
            {
                TypeCode.Byte => packetIn.Read<byte>(),
                TypeCode.Int32 => packetIn.Read<int>(),
                TypeCode.Int16 => packetIn.Read<short>(),
                TypeCode.String => packetIn.ReadUTF8(),
                TypeCode.Single => packetIn.Read<float>(),
                TypeCode.Double => packetIn.Read<double>(),
                TypeCode.Boolean => packetIn.Read<bool>(),
                _ => CreateUnknownValueType(ref packetIn, args, parameter.ParameterType)
            };
        }
        return values;
    }
    private object CreateUnknownValueType(ref HPacketReader packet, DataInterceptedEventArgs args, Type parameterType) => parameterType.Name switch
    {
        nameof(DataInterceptedEventArgs) => args,
        nameof(ReadOnlyMemory<byte>) => args.Packet,
        nameof(HPoint) => new HPoint(packet.Read<int>(), packet.Read<int>()),

        _ => null
    };

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Hash, IsOutgoing, Method);
    }
    public override bool Equals(object obj)
    {
        return Equals(obj as DataCaptureAttribute);

    }
    public bool Equals(DataCaptureAttribute other)
    {
        return other != null &&
            Id == other.Id &&
            Hash == other.Hash &&
            IsOutgoing == other.IsOutgoing &&
            EqualityComparer<MethodInfo>.Default.Equals(Method, other.Method);
    }
}