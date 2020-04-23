namespace DotaDb.Models
{
    public class HeroTalentChoiceViewModel
    {
        public HeroTalentChoiceViewModel()
        {
            HeroTalentChoice1 = new HeroTalentViewModel();
            HeroTalentChoice2 = new HeroTalentViewModel();
        }

        public HeroTalentViewModel HeroTalentChoice1 { get; set; }
        public HeroTalentViewModel HeroTalentChoice2 { get; set; }
    }
}