using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OROMDataBinToJson
{
    public class MagicItemSection
    {
        private const int HEADER_SIZE = 0x23;
        private const int DATA_HEAD_OFFSET = 0x02;
        private const int DATA_SIZE = 0x08;
        private const int DATA_TAIL_OFFSET = 0x01;
        
        public readonly byte[] Header;
        public readonly MagicItemDef[] MagicItems;
        
        public struct MagicItemDef
        {
            public string Name;
            public MagicItemRec Record;
            public string TextualInfo;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct MagicItemRec
        {
            public int Price;
            public int Weight;
        }
        
        private static unsafe MagicItemDef LoadItemDef(Stream stream, byte[] buffer)
        {
            var nameLength = stream.ReadByte();
            stream.Read(buffer, 0, nameLength);
            var name = nameLength <= 0 ? "" : Encoding.ASCII.GetString(buffer, 0, nameLength);
            
            MagicItemRec rec;
            stream.Seek(DATA_HEAD_OFFSET, SeekOrigin.Current);
            stream.Read(buffer, 0, DATA_SIZE);
            fixed (void* data = buffer)
                rec = *((MagicItemRec*) data);
            stream.Seek(DATA_TAIL_OFFSET, SeekOrigin.Current);
            
            var infoLength = (byte) stream.ReadByte();
            if (infoLength <= 0)
                return new MagicItemDef {Name = name, Record = rec, TextualInfo = ""};
            
            stream.Read(buffer, 0, infoLength);
            return new MagicItemDef
            {
                Name = name,
                Record = rec, 
                TextualInfo = Encoding.ASCII.GetString(buffer, 0, infoLength)
            };
        }
        
        public MagicItemSection(Stream stream, byte[] buffer)
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
            
            MagicItems = new MagicItemDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                MagicItems[i] = LoadItemDef(stream, buffer);
         
        }
    }
}