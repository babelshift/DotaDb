namespace DotaDb.ViewModels
{
    public class ItemAutographViewModel
    {
        public string Name { get; set; }
        public string Autograph { get; set; }
        public ulong? WorkshopLink { get; set; }
        public uint Language { get; set; }
        public string IconPath { get; set; }
        public string Modifier { get; set; }
    }
}