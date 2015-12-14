namespace DotaDb.Utilities
{
    public static class HeroExperience
    {
        public static int ToReachLevel(int level)
        {
            switch (level)
            {
                case 1: return 0; 
                case 2: return 200; 
                case 3: return 500; 
                case 4: return 900; 
                case 5: return 1400; 
                case 6: return 2000; 
                case 7: return 2600; 
                case 8: return 3200; 
                case 9: return 4400; 
                case 10: return 5400; 
                case 11: return 6000; 
                case 12: return 8200; 
                case 13: return 9000; 
                case 14: return 10400; 
                case 15: return 11900; 
                case 16: return 13500; 
                case 17: return 15200; 
                case 18: return 17000; 
                case 19: return 18900; 
                case 20: return 20900; 
                case 21: return 23000; 
                case 22: return 25200; 
                case 23: return 27500; 
                case 24: return 29900; 
                case 25: return 32400; 
                default: return 0;
            }
        }
    }
}