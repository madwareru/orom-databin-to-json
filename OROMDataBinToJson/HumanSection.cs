using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace OROMDataBinToJson
{
    public class HumanSection
    {
        private static readonly Regex _humanUnitNameRegexp = new Regex(
            "^(?:PC|NPC|NPC\\d{1,3}|.|M\\d{1,3}|Man.*)_.*", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        
        private const int HEADER_SIZE = 0x014F;
        private const int DATA_HEAD_OFFSET = 0x02;
        private const int DATA_SIZE = 0x68;

        public readonly byte[] Header;
        public readonly HumanDef[] HumanDefinitions;
        
        public struct HumanDef
        {
            public string Name;
            public HumanRec Record;
            public string[] ItemsWearing;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct HumanRec
        {
            public int Body;            public int Reaction; 
            public int Mind;            public int Spirit; 
            public int HealthMax;       public int ManaMax; 
            public int Speed;           public int RotationSpeed; 
            public int ScanRange;       public int Defence; 
            public int SkillGeneral;    public int SkillBladeFire; 
            public int SkillAxeWater;   public int SkillBludgeonAir; 
            public int SkillPikeEarth;  public int SkillShootingAstral; 
            public int TypeId;          public int Face; 
            public int Gender;          public int AttackChargeTime; 
            public int AttackRelaxTime; public int TokenSize; 
            public int MovementType;    public int DyingTime; 
            public int ServerId;        public int KnownSpells;
        }
        
        private static unsafe HumanDef LoadHumanDef(Stream stream, byte[] buffer, HashSet<string> itemNamesHashSet)
        {
            var checkerByte = stream.ReadByte();
            while (checkerByte == 00)
                checkerByte = stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);
            
            var nameLength = stream.ReadByte();
            var name = "";
            if (nameLength > 0)
            {
                stream.Read(buffer, 0, nameLength);
                name = Encoding.ASCII.GetString(buffer, 0, nameLength);
            }
            
            var lookAheadByte = stream.ReadByte();
            while (lookAheadByte == 00)
                lookAheadByte = stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);

            HumanRec rec;
            stream.Seek(DATA_HEAD_OFFSET, SeekOrigin.Current);
            stream.Read(buffer, 0, DATA_SIZE);
            fixed (void* data = buffer)
                rec = *((HumanRec*) data);
            
            var itemsWearing = new string[10];

            for (var texInfoId = 0; texInfoId < 10; ++texInfoId)
            {
                var infoLength = (byte) stream.ReadByte();
                if (infoLength <= 0 )
                    continue;
                stream.Read(buffer, 0, infoLength);
                
                var textualInfo = Encoding.ASCII.GetString(buffer, 0, infoLength);
                if (!string.IsNullOrEmpty(textualInfo) && _humanUnitNameRegexp.IsMatch(textualInfo))
                {
                    stream.Seek(-infoLength - 1, SeekOrigin.Current);
                    break;
                }

                var itemName = Encoding.ASCII.GetString(buffer, 0, infoLength);
                itemsWearing[texInfoId] = itemName;
                if (!itemNamesHashSet.Contains(itemName))
                    itemNamesHashSet.Add(itemName);

                lookAheadByte = stream.ReadByte();
                if (lookAheadByte != 0)
                {
                    stream.Seek(-1, SeekOrigin.Current);
                    continue;
                }
                lookAheadByte = stream.ReadByte();
                if (lookAheadByte != 0)
                {
                    stream.Seek(-1, SeekOrigin.Current);
                    continue;
                }
                lookAheadByte = stream.ReadByte();
                if (lookAheadByte != 0)
                {
                    stream.Seek(-1, SeekOrigin.Current);
                    continue;
                }
                break;
            }
            
            return new HumanDef
            {
                Name = name,
                Record = rec, 
                ItemsWearing = itemsWearing
            };
        }
        
        public HumanSection(Stream stream, byte[] buffer, HashSet<string> itemNamesHashSet)
        {
            Header = new byte[HEADER_SIZE];
            stream.Read(Header, 0, HEADER_SIZE);
            
            stream.Seek(4, SeekOrigin.Current);
            const int entryCount = 0xD2;
            
            HumanDefinitions = new HumanDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                HumanDefinitions[i] = LoadHumanDef(stream, buffer, itemNamesHashSet);

            while (stream.LookAhead() == 00)
                stream.ReadByte();
        }
    }
}