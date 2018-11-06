using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OROMDataBinToJson
{
    public class SpellSection
    {
        private const int HEADER_SIZE = 0x14E;
        private const int DATA_HEAD_OFFSET = 0x02;
        private const int DATA_SIZE = 0x58;

        public readonly byte[] Header;
        public readonly SpellDef[] SpellDefinitions;
        
        public struct SpellDef
        {
            public string Name;
            public SpellRec Record;
            public string TextualInfo;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct SpellRec
        {
            public int ComplicationLevel;   public int ManaCost; 
            public int Sphere;              public int Item; 
            public int SpellTarget;         public int DeliverySystem; 
            public int MaxRange;            public int SpellEffectSpeed; 
            public int DistributionSystem;  public int Radius; 
            public int AreaEffectAffect;    public int AreaEffectDuration; 
            public int AreaEffectFrequency; public int ApplyOnUnitMethod; 
            public int SpellDuratuion;      public int SpellFrequency; 
            public int DamageMin;           public int DamageMax; 
            public int Defensive;           public int SkillOffset; 
            public int ScrollCost;          public int BookCost;
        }
        
        private static unsafe SpellDef LoadSpellDef(Stream stream, byte[] buffer)
        {
            var nameLength = stream.ReadByte();
            var name = "";
            if (nameLength > 0)
            {
                stream.Read(buffer, 0, nameLength);
                name = Encoding.ASCII.GetString(buffer, 0, nameLength);
            }
            
            SpellRec rec;
            stream.Seek(DATA_HEAD_OFFSET, SeekOrigin.Current);
            stream.Read(buffer, 0, DATA_SIZE);
            fixed (void* data = buffer)
                rec = *((SpellRec*) data);
            
            var infoLength = (byte) stream.ReadByte();
            if (infoLength <= 0)
                return new SpellDef {Name = name, Record = rec, TextualInfo = ""};
            
            stream.Read(buffer, 0, infoLength);
            return new SpellDef
            {
                Name = name,
                Record = rec, 
                TextualInfo = Encoding.ASCII.GetString(buffer, 0, infoLength)
            };
        }
        
        public SpellSection(Stream stream, byte[] buffer)
        {
            stream.Read(buffer, 0, HEADER_SIZE);
            Header = new byte[HEADER_SIZE];
            for (var i = 0; i < HEADER_SIZE; ++i)
                Header[i] = buffer[i];
            
            var entryCount = stream.ReadByte();
            entryCount += stream.ReadByte() * 0x100;
            entryCount += stream.ReadByte() * 0x10000;
            entryCount += stream.ReadByte() * 0x1000000;
            
            SpellDefinitions = new SpellDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                SpellDefinitions[i] = LoadSpellDef(stream, buffer);
         
        }
    }
}