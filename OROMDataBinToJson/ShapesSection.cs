using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OROMDataBinToJson
{
    public class ShapesSection
    {
        private const int HEADER_SIZE = 0x66;
        private const int DATA_HEAD_OFFSET = 0x10;
        private const int DATA_SIZE = 0x38;
        
        public struct ShapeDef
        {
            public string Name;
            public ShapeRec Record;
        }

        private ShapeDef LoadShapeDef(Stream stream, byte[] buffer)
        {
            var nameLength = stream.ReadByte();
            stream.Read(buffer, 0, nameLength);
            var name = nameLength <= 0 ? "" : Encoding.ASCII.GetString(buffer, 0, nameLength);
            return new ShapeDef
            {
                Name = name,
                Record = LoadShapeRec(stream, buffer)
            };
        }
        
        private unsafe ShapeRec LoadShapeRec(Stream stream, byte[] buffer)
        {
            stream.Seek(DATA_HEAD_OFFSET, SeekOrigin.Current);
            stream.Read(buffer, 0, DATA_SIZE);
            fixed (void* data = buffer)
                return *((ShapeRec*) data);
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct ShapeRec
        {
            public double Price;
            public double Weight;
            public double Damage;
            public double ToHit;
            public double Defence;
            public double Absorption;
            public double MagCapLevel;
        }
        
        public readonly byte[] Header;
        public readonly ShapeDef[] RarityDefinitions;
        public readonly ShapeDef[] MaterialDefinitions;

        public ShapesSection(Stream stream, byte[] buffer)
        {
            stream.Read(buffer, 0, HEADER_SIZE);
            Header = new byte[HEADER_SIZE];
            for (var i = 0; i < HEADER_SIZE; ++i)
                Header[i] = buffer[i];

            var entryCount = stream.ReadByte();
            entryCount += stream.ReadByte() * 0x100;
            entryCount += stream.ReadByte() * 0x10000;
            entryCount += stream.ReadByte() * 0x1000000;
            
            RarityDefinitions = new ShapeDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                RarityDefinitions[i] = LoadShapeDef(stream, buffer);
            
            entryCount = stream.ReadByte();
            entryCount += stream.ReadByte() * 0x100;
            entryCount += stream.ReadByte() * 0x10000;
            entryCount += stream.ReadByte() * 0x1000000;
            
            MaterialDefinitions = new ShapeDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                MaterialDefinitions[i] = LoadShapeDef(stream, buffer);
        }
    }
}