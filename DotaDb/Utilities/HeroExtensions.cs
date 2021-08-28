using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;

namespace DotaDb.Utilities
{
    public static class HeroExtensions
    {
        public static IReadOnlyList<HeroRole> GetRoles(this HeroSchema hero)
        {
            if (hero == null)
            {
                return new List<HeroRole>().AsReadOnly();
            }

            if (String.IsNullOrEmpty(hero.Role) || String.IsNullOrEmpty(hero.RoleLevels))
            {
                return new List<HeroRole>();
            }

            string[] rolesSplit = hero.Role.Split(',');
            string[] roleLevelsSplit = hero.RoleLevels.Split(',');

            List<HeroRole> roleViewModels = new List<HeroRole>();
            for (int i = 0; i < rolesSplit.Length; i++)
            {
                roleViewModels.Add(new HeroRole()
                {
                    Name = rolesSplit[i],
                    Level = roleLevelsSplit[i]
                });
            }

            return roleViewModels.AsReadOnly();
        }

        public static string GetMinimapIconFilePath(this HeroSchema hero, string minimapIconsBaseUrl)
        {
            if (hero == null)
            {
                return String.Empty;
            }

            string fileName = !String.IsNullOrEmpty(hero.Url) ? String.Format("{0}_icon.png", hero.Url) : String.Empty;

            if (String.IsNullOrEmpty(fileName))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}", minimapIconsBaseUrl, fileName);
        }

        public static string GetAvatarImageFilePath(this HeroSchema hero, string heroAvatarsBaseUrl)
        {
            if (hero == null)
            {
                return String.Empty;
            }

            string fileName = GetHeroAvatarFileName(hero.Name);
            if (String.IsNullOrEmpty(fileName))
            {
                return String.Empty;
            }

            return String.Format("{0}{1}_full.png", heroAvatarsBaseUrl, fileName);
        }

        private static string GetHeroAvatarFileName(string heroName)
        {
            // if the player hasn't picked a hero yet, the 'base' hero will be shown, which basically is 'unknown'
            if (heroName == "npc_dota_hero_base")
            {
                return String.Empty;
            }
            else
            {
                return heroName.Replace("npc_dota_hero_", "");
            }
        }
    }
}