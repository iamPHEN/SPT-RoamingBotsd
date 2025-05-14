using System.Collections.Generic;

namespace RoamingBots.Interop
{
    public enum Brain
    {
        ArenaFighter,
        BossBully,
        BossGluhar,
        Knight,
        BossKojaniy,
        BossSanitar,
        Tagilla,
        BossTest,
        //BossZryachiy,
        Obdolbs,
        ExUsec,
        BigPipe,
        BirdEye,
        FollowerBully,
        FollowerGluharAssault,
        FollowerGluharProtect,
        FollowerGluharScout,
        FollowerKojaniy,
        FollowerSanitar,
        TagillaFollower,
        //Fl_Zraychiy,
        Gifter,
        Killa,
        Marksman,
        PMC,
        PmcUsec,
        PmcBear,
        SectantPriest,
        SectantWarrior,
        CursAssault,
        Assault,
    }

    public static class AIBrains
    {
        public static readonly List<Brain> Scavs = new()
        {
            Brain.CursAssault,
            Brain.Assault,
        };

        public static readonly List<Brain> Goons = new()
        {
            Brain.Knight,
            Brain.BirdEye,
            Brain.BigPipe,
        };

        public static readonly List<Brain> Others = new()
        {
            Brain.Obdolbs,
        };

        public static readonly List<Brain> Bosses = new()
        {
            Brain.BossBully,
            Brain.BossGluhar,
            Brain.BossKojaniy,
            Brain.BossSanitar,
            Brain.Tagilla,
            Brain.BossTest,
            //Brain.BossZryachiy,
            Brain.Gifter,
            Brain.Killa,
            Brain.SectantPriest,
        };

        public static readonly List<Brain> Followers = new()
        {
            Brain.BossBully,
            Brain.FollowerBully,
            Brain.FollowerGluharAssault,
            Brain.FollowerGluharProtect,
            Brain.FollowerGluharScout,
            Brain.FollowerKojaniy,
            Brain.FollowerSanitar,
            Brain.TagillaFollower,
            //Brain.Fl_Zraychiy,
        };
    }
}
