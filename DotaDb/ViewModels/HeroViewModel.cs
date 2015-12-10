using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class HeroViewModel : BaseHeroViewModel
    {
        private const int baseHealth = 150;
        private const int healthPerStrength = 19;
        private const int baseMana = 0;
        private const int manaPerIntellect = 13;

        public IReadOnlyCollection<HeroAbilityViewModel> Abilities { get; set; }

        public double GetBaseHealth()
        {
            return GetHealth(0);
        }

        public double GetBaseMana()
        {
            return GetMana(0);
        }

        public double GetHealth(int level)
        {
            return Math.Round(baseHealth + (healthPerStrength * (BaseStrength + (level * StrengthGain))));
        }

        public double GetMana(int level)
        {
            return Math.Round(baseMana + (manaPerIntellect * (BaseIntelligence + (level * IntelligenceGain))));
        }

        public double GetMinDamage(int level)
        {
            if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.STRENGTH.Key)
            {
                return Math.Round(BaseDamageMin + (BaseStrength + (level * StrengthGain)));
            }
            else if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.INTELLECT.Key)
            {
                return Math.Round(BaseDamageMin + (BaseIntelligence + (level * IntelligenceGain)));
            }
            else if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.AGILITY.Key)
            {
                return Math.Round(BaseDamageMin + (BaseAgility + (level * AgilityGain)));
            }
            else
            {
                return 0;
            }
        }

        public double GetMaxDamage(int level)
        {
            if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.STRENGTH.Key)
            {
                return Math.Round(BaseDamageMax + (BaseStrength + (level * StrengthGain)));
            }
            else if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.INTELLECT.Key)
            {
                return Math.Round(BaseDamageMax + (BaseIntelligence + (level * IntelligenceGain)));
            }
            else if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.AGILITY.Key)
            {
                return Math.Round(BaseDamageMax + (BaseAgility + (level * AgilityGain)));
            }
            else
            {
                return 0;
            }
        }

        public double GetArmor(int level)
        {
            return Math.Round(BaseStrength + (level * StrengthGain));
        }

        public double GetStrength(int level)
        {
            return Math.Round(BaseStrength + (level * StrengthGain));
        }
        public double GetAgility(int level)
        {
            return Math.Round(BaseAgility + (level * AgilityGain));
        }
        public double GetIntelligence(int level)
        {
            return Math.Round(BaseIntelligence + (level * IntelligenceGain));
        }
    }
}