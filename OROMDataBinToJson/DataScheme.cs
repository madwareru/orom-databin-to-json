using System;
using System.Collections.Generic;
using System.IO;

namespace OROMDataBinToJson
{
    public class DataScheme
    {
        public enum SectionKind { Human, Item, MagicItem, Parameter, Shapes, Spell, Structure, Unit, Unknown }

        public readonly HumanSection HumanData;
        public readonly ItemSection ItemData;
        public readonly MagicItemSection MagicItemData;
        public readonly ParameterSection ParameterData;
        public readonly ShapesSection ShapesData;
        public readonly SpellSection SpellData;
        public readonly StructureSection StructureData;
        public readonly UnitSection UnitData;
        
        public DataScheme(Stream stream)
        {
            var buffer = new byte[8192 * 8192];
            var preFetchedBytes = new byte[20];
            var set = new HashSet<SectionKind>
            {
                SectionKind.Human,
                SectionKind.Item,
                SectionKind.MagicItem,
                SectionKind.Parameter,
                SectionKind.Shapes,
                SectionKind.Spell,
                SectionKind.Structure,
                SectionKind.Unit
            };
            
            while (set.Count > 0)
            {
                var currentSection = SectionKind.Unknown;
                stream.Read(preFetchedBytes, 0, 20);
                if (preFetchedBytes[0x03] == 'S' &&
                    preFetchedBytes[0x04] == 'h' &&
                    preFetchedBytes[0x05] == 'a' &&
                    preFetchedBytes[0x06] == 'p' &&
                    preFetchedBytes[0x07] == 'e')
                    currentSection = SectionKind.Shapes;
                else if (preFetchedBytes[0x03] == 'P' &&
                         preFetchedBytes[0x04] == 'a' &&
                         preFetchedBytes[0x05] == 'r' &&
                         preFetchedBytes[0x06] == 'a' &&
                         preFetchedBytes[0x07] == 'm')
                    currentSection = SectionKind.Parameter;
                else if (preFetchedBytes[0x03] == 'I' &&
                         preFetchedBytes[0x04] == 't' &&
                         preFetchedBytes[0x05] == 'e' &&
                         preFetchedBytes[0x06] == 'm')
                    currentSection = SectionKind.Item;
                else if (preFetchedBytes[0x03] == 'M' &&
                         preFetchedBytes[0x04] == 'a' &&
                         preFetchedBytes[0x05] == 'g' &&
                         preFetchedBytes[0x06] == 'i' &&
                         preFetchedBytes[0x07] == 'c')
                    currentSection = SectionKind.MagicItem;
                else if (preFetchedBytes[0x03] == 'U' &&
                         preFetchedBytes[0x04] == 'n' &&
                         preFetchedBytes[0x05] == 'i' &&
                         preFetchedBytes[0x06] == 't')
                    currentSection = SectionKind.Unit;
                else if (preFetchedBytes[0x03] == 'H' &&
                         preFetchedBytes[0x04] == 'u' &&
                         preFetchedBytes[0x05] == 'm' &&
                         preFetchedBytes[0x06] == 'a' &&
                         preFetchedBytes[0x07] == 'n')
                    currentSection = SectionKind.Human;
                else if (preFetchedBytes[0x03] == 'B' &&
                         preFetchedBytes[0x04] == 'u' &&
                         preFetchedBytes[0x05] == 'i' &&
                         preFetchedBytes[0x06] == 'l' &&
                         preFetchedBytes[0x07] == 'd')
                    currentSection = SectionKind.Structure;
                else if (preFetchedBytes[0x03] == 'S' &&
                         preFetchedBytes[0x04] == 'p' &&
                         preFetchedBytes[0x05] == 'e' &&
                         preFetchedBytes[0x06] == 'l' &&
                         preFetchedBytes[0x07] == 'l')
                    currentSection = SectionKind.Spell;
                stream.Seek(-20, SeekOrigin.Current);

                switch (currentSection)
                {
                    case SectionKind.Human:
                        HumanData = new HumanSection(stream, buffer);
                        break;
                    case SectionKind.Item:
                        ItemData = new ItemSection(stream, buffer);
                        break;
                    case SectionKind.MagicItem:
                        MagicItemData = new MagicItemSection(stream, buffer);
                        break;
                    case SectionKind.Parameter:
                        ParameterData = new ParameterSection(stream, buffer);
                        break;
                    case SectionKind.Shapes:
                        ShapesData = new ShapesSection(stream, buffer);
                        break;
                    case SectionKind.Spell:
                        SpellData = new SpellSection(stream, buffer);
                        break;
                    case SectionKind.Structure:
                        StructureData = new StructureSection(stream, buffer);
                        break;
                    case SectionKind.Unit:
                        UnitData = new UnitSection(stream, buffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                set.Remove(currentSection);
            }
        }
    }
}