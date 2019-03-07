using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BestSockets.Internal
{
    internal class ObjectSerializer : IObjectSerializer
    {
        public byte[] Serialize(object obj)
        {
            var serializer = new BinaryFormatter();
            var stream = new MemoryStream();

            serializer.Serialize(stream, obj);

            return stream.ToArray();
        }

        public object Deserialize(byte[] bytes)
        {
            var serializer = new BinaryFormatter();
            var stream = new MemoryStream();

            stream.Write(bytes);
            stream.Seek(0, SeekOrigin.Begin);

            return serializer.Deserialize(stream);
        }
    }
}
