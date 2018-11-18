using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OROMDataBinToJson
{
    public class DataScheme
    {
        public enum SectionKind { Human, Item, MagicItem, Parameter, Shapes, Spell, Structure, Unit, Unknown }
        
        public struct ItemInfo
        {
            public abstract class AbstractEffect{}

            public class ParameterModifier : AbstractEffect
            {
                public string Type => "parameter";
                public int ParameterId;
                public int Value;
            }
            
            public class AttachedSpell : AbstractEffect
            {
                public string Type => "spell";
                public int SpellId;
                public int Force;
            }

            public int MaterialId;
            public int SlotId;
            public int RarityId;
            public int ItemOrderId;

            public AbstractEffect[] Effects;

            public int ItemId =>
                ItemOrderId +
                (RarityId & 0b111) * (0b100000) +
                (SlotId  & 0b1111) * (0b1_00000000) +
                (MaterialId & 0b1111) * (0b10000_00000000);
        }

        public readonly Dictionary<string, ItemInfo> ItemInfoLookup;

        public readonly HumanSection HumanData;
        public readonly ItemSection ItemData;
        public readonly MagicItemSection MagicItemData;
        public readonly ParameterSection ParameterData;
        public readonly ShapesSection ShapesData;
        public readonly SpellSection SpellData;
        public readonly StructureSection StructureData;
        public readonly UnitSection UnitData;
        
        private static Regex _modifiersClauseRegex = new Regex(@"\{([\sa-zA-Z0-9_:=,]*)\}", RegexOptions.Compiled);
        
        private static Regex _spellExpr = new Regex(@"castSpell=([a-zA-Z_]*):([0-9]+)", RegexOptions.Compiled);
        private static Regex _dupletExpr = new Regex(@"([a-zA-Z_]*)=([0-9]+)", RegexOptions.Compiled);

        public ItemInfo GetMagicItemInfo(string name) =>
            MagicItemData.MagicItems.FoundToken(x => x.Name, name, out var entryId) ? 
                new ItemInfo{ ItemOrderId = entryId + 1, SlotId = 0x0E} : default(ItemInfo);

        private ItemInfo.AbstractEffect GetEffectFromModifierString(string modifierString)
        {
            if (_spellExpr.IsMatch(modifierString))
            {
                var match = _spellExpr.Match(modifierString);
                var spellName = match.Groups[1].Value.Replace('_', ' ');
                var spellForce = int.Parse(match.Groups[2].Value);
                if (SpellData.SpellDefinitions.FoundToken(x => x.Name, spellName, out var spellEntryId))
                    return new ItemInfo.AttachedSpell{ SpellId = spellEntryId + 1, Force = spellForce};
            }
            else if (_dupletExpr.IsMatch(modifierString))
            {
                var match = _dupletExpr.Match(modifierString);
                var paramName = match.Groups[1].Value;
                var modifierValue = int.Parse(match.Groups[2].Value);
                if (ParameterData.Parameters.FoundToken(x => x.Name, paramName, out var paramEntryId))
                    return new ItemInfo.ParameterModifier { ParameterId = paramEntryId + 1, Value = modifierValue} ;
            }
            return null;
        }
        
        public ItemInfo GetItemInfo(string name)
        {
            var info = new ItemInfo();

            if(ShapesData.RarityDefinitions.FoundToken(x => x.Name, name, out var rarityEntryId))
                info.RarityId = rarityEntryId;

            if(ShapesData.MaterialDefinitions.FoundToken(x => x.Name, name, out var materialEntryId))
                info.MaterialId = materialEntryId;

            if (_modifiersClauseRegex.IsMatch(name))
            {
                info.Effects = _modifiersClauseRegex
                    .Match(name)
                    .Groups[1]
                    .Value
                    .Replace(" ","")
                    .Split(',')
                    .Select(GetEffectFromModifierString)
                    .ToArray();
            }
            
                        
            if (ItemData.Shields.FoundToken(x => x.Name, name, out var shieldEntryId))
            {
                info.SlotId = ItemData.Shields[shieldEntryId].Record.Slot;
                info.ItemOrderId = shieldEntryId + 1;
                return info;
            }
                        
            if (ItemData.Weapons.FoundToken(x => x.Name, name, out var weaponEntryId))
            {
                info.SlotId = ItemData.Weapons[weaponEntryId].Record.Slot;
                info.ItemOrderId = weaponEntryId + 1;
                return info;
            }
                        
            if (ItemData.Wieldables.FoundToken(x => x.Name, name, out var wieldableEntryId))
            {
                info.SlotId = ItemData.Wieldables[wieldableEntryId].Record.Slot;
                info.ItemOrderId = wieldableEntryId + 1;
            }

            return info;
        }
        
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
            
            var itemNamesHashSet = new HashSet<string>();
            
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
                        HumanData = new HumanSection(stream, buffer, itemNamesHashSet);
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
                        UnitData = new UnitSection(stream, buffer, itemNamesHashSet);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                set.Remove(currentSection);
            }

            ItemInfoLookup =
                itemNamesHashSet
                .Select(name => Make.Pair(name, GetItemInfo(name)))
                .ToDictionary(entry => entry.Key, entry => entry.Value);
            
            for (var i = 0; i < MagicItemData.MagicItems.Length; ++i)
            {
                ItemInfoLookup.Add(
                    MagicItemData.MagicItems[i].Name,
                    new ItemInfo{ ItemOrderId = i, SlotId = 0x0E});
            }
        }
    }
}