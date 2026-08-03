namespace App.Root._Binary;
using System.Text;
using IBinaryReader = System.IO.BinaryReader; 

public class BinaryReader : IDisposable {
    public MemoryStream stream;
    private IBinaryReader reader;
    
    private bool disposed = false;

    public BinaryReader(byte[] data) {
        stream = new MemoryStream(data);
        reader = new IBinaryReader(stream);
    }

    // Get IBinaryReader
    public IBinaryReader GetIBinaryReader() {
        IBinaryReader val = reader;
        return val;
    }

    // Dispose
    public void Dispose() {
        if(!disposed) {
            reader.Dispose();
            stream.Dispose();

            disposed = true;
        }
    }

    /**
     *
     * Read
     *
     */
    public bool ReadBool() => reader.ReadBoolean();
    public byte ReadByte() => reader.ReadByte();
    public short ReadShort() => reader.ReadInt16();
    public ushort ReadUShort() => reader.ReadUInt16();
    public int ReadInt() => reader.ReadInt32();
    public uint ReadUInt() => reader.ReadUInt32();
    public long ReadLong() => reader.ReadInt64();
    public ulong ReadULong() => reader.ReadUInt64();
    public float ReadFloat() => reader.ReadSingle();
    public double ReadDouble() => reader.ReadDouble();
    public decimal ReadDecimal() => reader.ReadDecimal();
    public DateTime ReadDateTime() => new DateTime(reader.ReadInt64());
    public TimeSpan ReadTimeSpan() => new TimeSpan(reader.ReadInt64());
    public Guid ReadGuid() => new Guid(reader.ReadBytes(16));
    public byte[] ReadBytes() {
        int length = reader.ReadInt32();
        return reader.ReadBytes(length);
    }
    public string ReadString() {
        int length = reader.ReadInt32();
        if(length == 0) return string.Empty;

        byte[] bytes = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    /**
     *
     * Read Object
     *
     */
    public object? ReadObject() {
        byte isNull = reader.ReadByte();
        if(isNull == 0) return null;

        TypeCode typeCode = (TypeCode)reader.ReadByte();
        switch(typeCode) {
            case TypeCode.Boolean: return ReadBool();
            case TypeCode.Byte: return ReadByte();
            case TypeCode.Int16: return ReadShort();
            case TypeCode.Int32: return ReadInt();
            case TypeCode.Int64: return ReadLong();
            case TypeCode.Single: return ReadFloat();
            case TypeCode.Double: return ReadDouble();
            case TypeCode.Decimal: return ReadDecimal();
            case TypeCode.String: return ReadString();
            case TypeCode.DateTime: return ReadDateTime();
            case TypeCode.Object:
                string typeName = ReadString();
                switch(typeName) {
                    case "TimeSpan": return ReadTimeSpan();
                    case "Guid": return ReadGuid();
                    case "ByteArray": return ReadBytes();
                    case "List":
                        int count = ReadInt();
                        
                        var list = new List<object?>();
                        for(int i = 0; i < count; i++) {
                            list.Add(ReadObject());
                        }

                        return list;
                    case "Dictionary":
                        int dictCount = ReadInt();

                        var dict = new Dictionary<object, object?>();
                        for(int i = 0; i < dictCount; i++) {
                            var key = ReadObject();
                            var value = ReadObject();
                            if(key != null) dict[key] = value;
                        }

                        return dict;
                    default:
                        var type = Type.GetType(typeName);
                        if(type == null) return null;

                        var obj = Activator.CreateInstance(type);

                        int memberCount = ReadInt();
                        for(int i = 0; i < memberCount; i++) {
                            string memberName = ReadString();
                            
                            var value = ReadObject();
                            if(value == null) continue;

                            var prop = type.GetProperty(memberName);
                            if(prop != null && prop.CanWrite) {
                                try {
                                    prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                                } catch {
                                    prop.SetValue(obj, value);
                                }

                                continue;
                            }

                            var field = type.GetField(memberName);
                            if(field != null) {
                                try {
                                    field.SetValue(obj, Convert.ChangeType(value, field.FieldType));
                                } catch {
                                    field.SetValue(obj, value);
                                }
                            }
                        }

                        return obj;
                }
                default:
                    return null;
        }
    }

    public T? ReadObject<T>() {
        var obj = ReadObject();
        if(obj is T typed) return typed;

        return default;
    }
}