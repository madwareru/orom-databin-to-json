using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OROMDataBinToJson
{
    public class ParameterSection
    {
        public readonly byte[] Header;
        public readonly ParameterDef[] Parameters;
        
        private const int HEADER_SIZE = 0x123;
        private const int DATA_HEAD_OFFSET = 0x02;
        private const int DATA_SIZE = 0x70;
        
        public struct ParameterDef
        {
            public string Name;
            public ParameterRec Record;
        }

        private static ParameterDef LoadParameterDef(Stream stream, byte[] buffer)
        {
            var nameLength = stream.ReadByte();
            stream.Read(buffer, 0, nameLength);
            var name = nameLength <= 0 ? "" : Encoding.ASCII.GetString(buffer, 0, nameLength);
            return new ParameterDef
            {
                Name = name,
                Record = LoadParamRec(stream, buffer)
            };
        }
        
        private static unsafe ParameterRec LoadParamRec(Stream stream, byte[] buffer)
        {
            stream.Seek(DATA_HEAD_OFFSET, SeekOrigin.Current);
            stream.Read(buffer, 0, DATA_SIZE);
            fixed (void* data = buffer)
                return *((ParameterRec*) data);
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct ParameterRec
        {
            public int CostMP;         public int AffectMin;
            public int AffectMax;      public int UsableBy;
            public int InWeapon;       public int InShield;
            public int Nop1;           public int InRing;
            public int InAmulet;       public int InHelm;
            public int InMail;         public int InCuirass;
            public int InBracers;      public int InGauntlets;
            public int Nop2;           public int InBoots;
            public int InWeapon2;      public int Nop3;
            public int Nop4;           public int InRing2;
            public int InAmulet2;      public int InHat;
            public int InRobe;         public int InCloak;
            public int Nop5;           public int InGloves;
            public int Nop6;           public int InShoes;
        }
        
        public ParameterSection(Stream stream, byte[] buffer)
        {
            stream.Read(buffer, 0, HEADER_SIZE);
            Header = new byte[HEADER_SIZE];
            for (var i = 0; i < HEADER_SIZE; ++i)
                Header[i] = buffer[i];
            
            var entryCount = stream.ReadByte();
            entryCount += stream.ReadByte() * 0x100;
            entryCount += stream.ReadByte() * 0x10000;
            entryCount += stream.ReadByte() * 0x1000000;
            
            Parameters = new ParameterDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                Parameters[i] = LoadParameterDef(stream, buffer);
        }
    }
}