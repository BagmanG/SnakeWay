using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YG;

namespace Assets.Scripts
{
    public static class GlobalVars
    {
        public static int currentLevelID = 1;
        public static Biome currentBiome = Biome.Forest;
        public static String levelName = "Forest1";

        public static bool WinterPurchased()
        {
            return YG2.GetState("WinterBiomePurchased") == 1;
        }
    }
}
