namespace App.Root._Binary;
using System.Collections;
using System.Text;
using System.Reflection;
using IBinaryWriter = System.IO.BinaryWriter;

public class BinaryWriter : IDisposable {
    private MemoryStream stream;
    private IBinaryWriter writer;
    private bool disposed = false;

    public BinaryWriter() {
        stream = new MemoryStream();
        writer = new IBinaryWriter(stream);
    }

    // Get Bytes
    public byte[] GetBytes() {
        byte[] val = stream.ToArray();
        return val;
    }

    // Dispose
    public void Dispose() {
        if(!disposed) {
            writer.Dispose();
            stream.Dispose();
            disposed = true;
        }
    }

    /**
     *
     * Write
     *
     */
    public void Write(bool value) => writer.Write(value);
    public void Write(byte value) => writer.Write(value);
    public void Write(short value) => writer.Write(value);
    public void Write(ushort value) => writer.Write(value);
    public void Write(int value) => writer.Write(value);
    public void Write(uint value) => writer.Write(value);
    public void Write(long value) => writer.Write(value);
    public void Write(ulong value) => writer.Write(value);
    public void Write(float value) => writer.Write(value);
    public void Write(double value) => writer.Write(value);
    public void Write(decimal value) => writer.Write(value);
    public void Write(byte[] value) {
        writer.Write(value.Length);
        writer.Write(value);
    }
    public void Write(string value) {
        if(string.IsNullOrEmpty(value)) {
            writer.Write(0);
            return;
        }

        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }
    public void Write(DateTime value) {
        writer.Write(value.Ticks);
    }
    public void Write(TimeSpan value) {
        writer.Write(value.Ticks);
    }
    public void Write(Guid value) {
        writer.Write(value.ToByteArray());
    }

    /**
     *
     * Write Object
     *
     */
    public void WriteObject(object? obj) {
        if(obj == null) {
            writer.Write((byte)0);
            Console.WriteLine("obj null!");
            return;
        }

        writer.Write((byte)1);

        var type = obj.GetType();

        if(type == typeof(bool)) { writer.Write((byte)TypeCode.Boolean); Write((bool)obj); return; }
        if(type == typeof(byte)) { writer.Write((byte)TypeCode.Byte); Write((byte)obj); return; }
        if(type == typeof(short)) { writer.Write((byte)TypeCode.Int16); Write((short)obj); return; }
        if(type == typeof(int)) { writer.Write((byte)TypeCode.Int32); Write((int)obj); return; }
        if(type == typeof(long)) { writer.Write((byte)TypeCode.Int64); Write((long)obj); return; }
        if(type == typeof(float)) { writer.Write((byte)TypeCode.Single); Write((float)obj); return; }
        if(type == typeof(double)) { writer.Write((byte)TypeCode.Double); Write((double)obj); return; }
        if(type == typeof(decimal)) { writer.Write((byte)TypeCode.Decimal); Write((decimal)obj); return; }
        if(type == typeof(string)) { writer.Write((byte)TypeCode.String); Write((string)obj); return; }
        if(type == typeof(DateTime)) { writer.Write((byte)TypeCode.DateTime); Write((DateTime)obj); return; }
        if(type == typeof(TimeSpan)) { writer.Write((byte)TypeCode.Object); Write("TimeSpan"); Write((TimeSpan)obj); return; }
        if(type == typeof(Guid)) { writer.Write((byte)TypeCode.Object); Write("Guid"); Write((Guid)obj); return; }
        if(type == typeof(byte[])) { writer.Write((byte)TypeCode.Object); Write("ByteArray"); Write((byte[])obj); return; }

        if(obj is IEnumerable en) {
            writer.Write((byte)TypeCode.Object);
            Write("List");

            var list = en.Cast<object>().ToList();
            writer.Write(list.Count);
            foreach(var item in list) {
                WriteObject(item);
            }

            return;
        }
        if(obj is IDictionary dict) {
            writer.Write((byte)TypeCode.Object);
            Write("Dictioanry");
            
            writer.Write(dict.Keys.Count);

            foreach(var key in dict.Keys) {
                WriteObject(key);
                WriteObject(dict[key]);
            }
        }

        writer.Write((byte)TypeCode.Object);
        Write(type.FullName ?? type.Name);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => f.GetCustomAttribute<SyncFieldAttribute>() != null).ToList();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.GetCustomAttribute<SyncFieldAttribute>() != null).ToList();
        writer.Write(fields.Count + properties.Count);

        foreach(var field in fields) {
            Write(field.Name);
            var value = field.GetValue(obj);
            WriteObject(value);
        }
        foreach(var prop in properties) {
            Write(prop.Name);
            var value = prop.GetValue(obj);
            WriteObject(value);
        }
    }
}