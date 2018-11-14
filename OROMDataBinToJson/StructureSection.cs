using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OROMDataBinToJson
{
    public class StructureSection
    {
        private const int HEADER_SIZE = 0x56;
        private const int DATA_HEAD_OFFSET = 0x02;
        private const int DATA_SIZE = 0x18;

        public readonly byte[] Header;
        public readonly StructureDef[] StructureDefinitions;
     
        public struct StructureDef
        {
            public string Name;
            public StructureRec Record;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StructureRec
        {
            public int SizeX;        public int SizeY; 
            public int ScanRange;    public short HealthMax; 
            public byte Passability; public byte BuildingPresent;
            public int StartID;      public short Tiles;
        }
        
        private static unsafe StructureDef LoadStructureDef(Stream stream, byte[] buffer)
        {
            var nameLength = stream.ReadByte();
            stream.Read(buffer, 0, nameLength);
            var name = nameLength <= 0 ? "" : Encoding.ASCII.GetString(buffer, 0, nameLength);
            
            StructureRec rec;
            stream.Seek(DATA_HEAD_OFFSET, SeekOrigin.Current);
            stream.Read(buffer, 0, DATA_SIZE);
            fixed (void* data = buffer)
                rec = *((StructureRec*) data);
            
            return new StructureDef
            {
                Name = name,
                Record = rec
            };
        }
        
        public StructureSection(Stream stream, byte[] buffer)
        {
            stream.Read(buffer, 0, HEADER_SIZE);
            Header = new byte[HEADER_SIZE];
            for (var i = 0; i < HEADER_SIZE; ++i)
                Header[i] = buffer[i];
            
            var entryCount = stream.ReadByte();
            entryCount += stream.ReadByte() * 0x100;
            entryCount += stream.ReadByte() * 0x10000;
            entryCount += stream.ReadByte() * 0x1000000;
            --entryCount;
            
            StructureDefinitions = new StructureDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                StructureDefinitions[i] = LoadStructureDef(stream, buffer);
         
        }
    }
}