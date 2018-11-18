using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OROMDataBinToJson
{
    public class UnitSection
    {
        private const int HEADER_SIZE = 0x026B;
        private const int DATA_HEAD_OFFSET = 0x02;
        private const int DATA_SIZE = 0xDC;

        public readonly byte[] Header;
        public UnitDef[] UnitDefinitions;
        
        public struct UnitDef
        {
            public string Name;
            public UnitRec Record;
            public string TextualInfo;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct UnitRec
        {
            public int Body;              public int Reaction;          
            public int Mind;              public int Spirit;            
            public int HealthMax;         public int HPRegeneration;    
            public int ManaMax;           public int MPRegeneration;    
            
            public int Speed;             public int RotationSpeed;     
            public int ScanRange;         public int PhysicalMin;       
            public int PhysicalMax;       public int AttackKind;        
            public int ToHit;             public int Defence;           
            
            public int Absorption;        public int AttackChargeTime;
            public int AttackRelaxTime;   public int ProtectFire;       
            public int ProtectWater;      public int ProtectAir;        
            public int ProtectEarth;      public int ProtectAstral;     
            
            public int ResistBlade;       public int ResistAxe;         
            public int ResistBludgeon;    public int ResistPike;        
            public int ResistShooting;    public int TypeID;            
            public int Face;              public int TokenSize;         
            
            public int MovementType;      public int DyingTime;         
            public int Withdraw;          public int Wimpy;             
            public int Seeinvisible;      public int XPvalue;           
            public int Treasure1Gold;     public int TreasureMin1;      
            
            public int TreasureMax1;      public int Treasure2Item;     
            public int TreasureMin2;      public int TreasureMax2;      
            public int Treasure3Magic;    public int TreasureMin3;      
            public int TreasureMax3;      public int Power;             
            
            public int Spell1;            public int Probability1;      
            public int Spell2;            public int Probability2;      
            public int Spell3;            public int Probability3;
            public int SpellPower;
        }
        
        private static unsafe UnitDef LoadUnitDef(Stream stream, byte[] buffer, HashSet<string> itemNames)
        {
            var checkerByte = stream.ReadByte();
            while (checkerByte == 00)
                checkerByte = stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);
            
            var nameLength = stream.ReadByte();
            stream.Read(buffer, 0, nameLength);
            var name = nameLength <= 0 ? "" : Encoding.ASCII.GetString(buffer, 0, nameLength);
            
            var lookAheadByte = stream.ReadByte();
            while (lookAheadByte == 00)
                lookAheadByte = stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);

            UnitRec rec;
            stream.Seek(DATA_HEAD_OFFSET, SeekOrigin.Current);
            stream.Read(buffer, 0, DATA_SIZE);
            fixed (void* data = buffer)
                rec = *((UnitRec*) data);
            
            var infoLength = (byte) stream.ReadByte();
            if (infoLength <= 0)
                return new UnitDef {Name = name, Record = rec, TextualInfo = ""};
            
            stream.Read(buffer, 0, infoLength);
            var itemName = Encoding.ASCII.GetString(buffer, 0, infoLength);
            if (!itemNames.Contains(itemName))
                itemNames.Add(itemName);
            return new UnitDef
            {
                Name = name,
                Record = rec, 
                TextualInfo = itemName
            };
        }
        
        public UnitSection(Stream stream, byte[] buffer, HashSet<string> itemNames)
        {
            stream.Read(buffer, 0, HEADER_SIZE);
            Header = new byte[HEADER_SIZE];
            for (var i = 0; i < HEADER_SIZE; ++i)
                Header[i] = buffer[i];

            stream.Seek(4, SeekOrigin.Current);
            const int entryCount = 0x38;
            
            UnitDefinitions = new UnitDef[entryCount];
            
            for (var i = 0; i < entryCount; ++i)
                UnitDefinitions[i] = LoadUnitDef(stream, buffer, itemNames);

            while (stream.LookAhead() == 00)
                stream.ReadByte();
        }
    }
}