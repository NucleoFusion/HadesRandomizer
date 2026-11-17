using System.IO;
using EldenRingParamsEditor;
using Hades.Constants;
using Hades.Services;

namespace Hades.Formats;

class ShowdownOfTheErdtree3 : IRandomizerFormat
{
    public string Id => "sote3";
    public string DisplayName => "Showdown Of The Erdtree 3";
    public string Me3File => "sote3.me3";
    private EldenRingLauncherService _launcherService = new EldenRingLauncherService();

    public void Exec(
        string baseSeed,
        Action<string>? statusCallback = null,
        Action<string>? seedCallback = null,
        IRandomizerOptions? options = null
    )
    {
        if (baseSeed == "")
        {
            var seed = Utils.RandoUtils.GenerateRandomString();
            seedCallback?.Invoke(seed);
            baseSeed = seed;
        }

        statusCallback?.Invoke($"seeding...: {baseSeed}");

        // Formatting seed
        baseSeed = $"sote3_{baseSeed}_{Utils.RandoUtils.GetVersion()}";

        var regulationFilepath = Path.Combine(
            GlobalConstants.ModEngineWorkingDirectory,
            Id,
            "bingo",
            "regulation.bin"
        );
        if (!File.Exists(regulationFilepath))
        {
            var msg = $"regulation.bin not found at {Path.GetFullPath(regulationFilepath)}";
            throw new RandomizerException(msg);
        }
        var swTotal = System.Diagnostics.Stopwatch.StartNew();
        var editor = ParamsEditor.ReadFromRegulationPath(regulationFilepath);

        // Randomization Logic

        // Shop Talisman
        var talismanResult = getTalismans(baseSeed);
        randomizeTalismans(editor, talismanResult);

        // Shop AoW
        var aowResult = getRandomAoW(baseSeed);
        randomizeAoW(editor, aowResult);

        // Classes
        var classResult = ArmorRandomizerService.GetRandomArmoredClasses(
            baseSeed,
            ArmorLocation.Base
        );
        randomizeClasses(editor, classResult);

        editor.WriteToRegulationPath(regulationFilepath);

        statusCallback?.Invoke("Successfully Randomized: " + baseSeed);
    }

    public void Launch()
    {
        _launcherService.LaunchEldenRingFromMe3File(Me3File);
    }

    private AoWResults getRandomAoW(string seed)
    {
        return new AoWResults
        {
            amazingAoW = Utils.RandoUtils.GetRandomNumber(
                seed + "_amazing",
                SOTE3Constants.AmazingAoW.Count()
            ),
            goodAoW = Utils.RandoUtils.GetRandomNumber(
                seed + "_good",
                SOTE3Constants.GoodAoW.Count()
            ),
            weakAoW = Utils.RandoUtils.GetRandomNumber(
                seed + "_weak",
                SOTE3Constants.WeakAoW.Count()
            ),
        };
    }

    private TalismanResults getTalismans(string seed)
    {
        var fillerTali_1 = Utils.RandoUtils.GetRandomNumber(
            seed + "_filler1",
            SOTE3Constants.FillerTalismans.Count()
        );
        var fillerTali_2 = Utils.RandoUtils.GetRandomNumber(
            seed + "_filler2",
            SOTE3Constants.FillerTalismans.Count()
        );
        if (fillerTali_1 == fillerTali_2)
        {
            if (fillerTali_2 == SOTE3Constants.FillerTalismans.Count() - 1)
                fillerTali_2 = 0;
            else
                fillerTali_2++;
        }

        var goodTali_1 = Utils.RandoUtils.GetRandomNumber(
            seed + "_good1",
            SOTE3Constants.GoodTalismans.Count()
        );
        var goodTali_2 = Utils.RandoUtils.GetRandomNumber(
            seed + "_good2",
            SOTE3Constants.GoodTalismans.Count()
        );
        if (goodTali_1 == goodTali_2)
        {
            if (goodTali_2 == SOTE3Constants.GoodTalismans.Count() - 1)
                goodTali_2 = 0;
            else
                goodTali_2++;
        }

        return new TalismanResults
        {
            fillerTalisman_1 = fillerTali_1,
            fillerTalisman_2 = fillerTali_2,
            goodTalisman_1 = goodTali_1,
            goodTalisman_2 = goodTali_2,
        };
    }

    private void randomizeAoW(ParamsEditor editor, AoWResults aowResults)
    {
        editor.SetShopLineupEquipId(
            SOTE3Constants.shopLineupMap["amazingAoW"],
            SOTE3Constants.AmazingAoW[aowResults.amazingAoW]
        );
        editor.SetShopLineupEquipId(
            SOTE3Constants.shopLineupMap["goodAoW"],
            SOTE3Constants.GoodAoW[aowResults.goodAoW]
        );
        editor.SetShopLineupEquipId(
            SOTE3Constants.shopLineupMap["weakAoW"],
            SOTE3Constants.WeakAoW[aowResults.weakAoW]
        );
    }

    private void randomizeClasses(ParamsEditor editor, ClassArmorResults results)
    {
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Vagabond"], results.Vagabond);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Warrior"], results.Warrior);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Hero"], results.Hero);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Astrologer"], results.Astrologer);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Prophet"], results.Prophet);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Confessor"], results.Confessor);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Bandit"], results.Bandit);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Samurai"], results.Samurai);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Prisoner"], results.Prisoner);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Wretch"], results.Wretch);
        randomizeClass(
            editor,
            GlobalConstants.CharaInitClassMap["Heavy Knight"],
            results.HeavyKnight
        );
        randomizeClass(
            editor,
            GlobalConstants.CharaInitClassMap["Idus Knight"],
            results.IdusKnight
        );
    }

    private void randomizeClass(ParamsEditor editor, int classId, ArmorResults result)
    {
        editor.SetInitialEquipArm(
            classId,
            Armors.Get(Gauntlets.All, ArmorLocation.Base).ElementAt(result.Arms).Id
        );
        editor.SetInitialEquipHelm(
            classId,
            Armors.Get(Helms.All, ArmorLocation.Base).ElementAt(result.Helm).Id
        );
        editor.SetInitialEquipLeg(
            classId,
            Armors.Get(Greaves.All, ArmorLocation.Base).ElementAt(result.Legs).Id
        );
        editor.SetInitialEquipTorso(
            classId,
            Armors.Get(ChestArmor.All, ArmorLocation.Base).ElementAt(result.Chest).Id
        );
    }

    private void randomizeTalismans(ParamsEditor editor, TalismanResults results)
    {
        editor.SetShopLineupEquipId(
            SOTE3Constants.shopLineupMap["fillerTalisman_1"],
            SOTE3Constants.FillerTalismans[results.fillerTalisman_1]
        );
        editor.SetShopLineupEquipId(
            SOTE3Constants.shopLineupMap["fillerTalisman_2"],
            SOTE3Constants.FillerTalismans[results.fillerTalisman_2]
        );
        editor.SetShopLineupEquipId(
            SOTE3Constants.shopLineupMap["goodTalisman_1"],
            SOTE3Constants.GoodTalismans[results.goodTalisman_1]
        );
        editor.SetShopLineupEquipId(
            SOTE3Constants.shopLineupMap["goodTalisman_2"],
            SOTE3Constants.GoodTalismans[results.goodTalisman_2]
        );
    }
}
