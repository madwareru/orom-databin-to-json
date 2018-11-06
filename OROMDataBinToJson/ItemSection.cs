using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OROMDataBinToJson
{
    public class ItemSection
    {
        public readonly byte[] Header;
        public readonly ItemDef[] Wieldables;
        public readonly ItemDef[] Shields;
        public readonly ItemDef[] Weapons;

        private const int HEADER_SIZE = 0xAD;
        private const int DATA_HEAD_OFFSET = 0x02;
        private const int DATA_SIZE = 0x50;
        
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct ItemRec
        {
            public int Shape;            public int Material;
            public int Price;            public int Weight;
            public int Slot;             public int AttackType;
            public int PhysicalMin;      public int PhysicalMax;
            public int ToHit;            public int Defence;
            public int Absorption;       public int Range;
            public int Charge;           public int Relax;
            public int TwoHanded;        public int SuitableFor;
            public int OtherParameter;   public int MysteriousField0;
            public int MysteriousField1; public int MysteriousField2;
        }
        
        public struct ItemDef
        {
            public string Name;
            public ItemRec Record;
        }
        public ItemSection(Stream stream, byte[] buffer)
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
            
            Wieldables = new ItemDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                Wieldables[i] = LoadItemDef(stream, buffer);
            
            entryCount = stream.ReadByte();
            entryCount += stream.ReadByte() * 0x100;
            entryCount += stream.ReadByte() * 0x10000;
            entryCount += stream.ReadByte() * 0x1000000;
            --entryCount;
            
            Shields = new ItemDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                Shields[i] = LoadItemDef(stream, buffer);
            
            entryCount = stream.ReadByte();
            entryCount += stream.ReadByte() * 0x100;
            entryCount += stream.ReadByte() * 0x10000;
            entryCount += stream.ReadByte() * 0x1000000;
            --entryCount;
            
            Weapons = new ItemDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                Weapons[i] = LoadItemDef(stream, buffer);
        }
        
        private static unsafe ItemDef LoadItemDef(Stream stream, byte[] buffer)
        {
            var nameLength = stream.ReadByte();
            stream.Read(buffer, 0, nameLength);
            var name = nameLength <= 0 ? "" : Encoding.ASCII.GetString(buffer, 0, nameLength);
            ItemRec rec;
            stream.Seek(DATA_HEAD_OFFSET, SeekOrigin.Current);
            stream.Read(buffer, 0, DATA_SIZE);
            fixed (void* data = buffer)
                rec = *((ItemRec*) data);
            return new ItemDef {Name = name, Record = rec};
        }

    }
}