using System.IO;
using System.Linq;
using EldenRingParamsEditor;
using Hades.Constants;
using Hades.Services;

namespace Hades.Formats;

public sealed record LandLockedOptions(int SquaresToGenerate, string JsonFilePath)
    : IRandomizerOptions;

class LandLocked : IRandomizerFormat
{
    public string Id => "landlocked";
    public string DisplayName => "Land Locked";
    public string Me3File => "ll.me3";
    private EldenRingLauncherService _launcherService = new EldenRingLauncherService();

    public int SquaresToGenerate { get; set; } = 25;
    public string JsonFilePath { get; set; } = "";

    public void Exec(
        string baseSeed,
        Action<string>? statusCallback,
        Action<string>? seedCallback,
        IRandomizerOptions? options
    )
    {
        var opts =
            options as LandLockedOptions
            ?? throw new ArgumentException(
                "LandLocked requires LandLockedOptions.",
                nameof(options)
            );

        if (baseSeed == "")
        {
            baseSeed = Utils.RandoUtils.GenerateRandomString();
            seedCallback?.Invoke(baseSeed);
        }

        statusCallback?.Invoke($"seeding...: {baseSeed}");

        // Formatting seed
        baseSeed = $"landlocked_{baseSeed}";

        var regulationFilepath = Path.Combine(
            GlobalConstants.ModEngineWorkingDirectory,
            Id,
            "bingo",
            "regulation.bin"
        );
        if (!File.Exists(regulationFilepath))
            throw new RandomizerException(
                $"regulation.bin not found at {Path.GetFullPath(regulationFilepath)}"
            );

        var editor = ParamsEditor.ReadFromRegulationPath(regulationFilepath);

        var urr = new UniversalReplacementRandomizer.OptimizedReplacementRandomizer(
            "landlocked",
            Utils.RandoUtils.GetStableSeed(baseSeed)
        );

        // Class Armor
        var classResult = ArmorRandomizerService.GetRandomArmoredClasses(
            baseSeed,
            ArmorLocation.Both
        );
        randomizeClasses(editor, classResult);

        // Starting Stats (10-16 each) + level 1
        StartingClassService.RandomizeStats(editor, baseSeed + "_statrando", LLStatRandomizer);
        for (int i = 0; i < ParamsEditor.TotalStartingClasses; i++)
        {
            editor.SetInitialRuneLevel(ParamsEditor.VagabondCharaInitId + i, 1);
        }

        // Starting Weapons (wieldable only) + deficit description
        StartingClassService.RandomizeAllStartingWeaponsWieldableWithDescriptions(
            editor,
            baseSeed + "_classweapons",
            Id
        );

        editor.WriteToRegulationPath(regulationFilepath);

        statusCallback?.Invoke("Successfully Randomized: " + baseSeed);
    }

    public void Launch()
    {
        _launcherService.LaunchEldenRingFromMe3File(Me3File);
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
            Armors.Get(Gauntlets.All, ArmorLocation.Both).ElementAt(result.Arms).Id
        );
        editor.SetInitialEquipHelm(
            classId,
            Armors.Get(Helms.All, ArmorLocation.Both).ElementAt(result.Helm).Id
        );
        editor.SetInitialEquipLeg(
            classId,
            Armors.Get(Greaves.All, ArmorLocation.Both).ElementAt(result.Legs).Id
        );
        editor.SetInitialEquipTorso(
            classId,
            Armors.Get(ChestArmor.All, ArmorLocation.Both).ElementAt(result.Chest).Id
        );
    }

    // Improve Stat Logic to provide stats from given pool (str,dex,arc have a pool of 55, 30 for int/fai, and rest have a normal 13x3)
    private static StartingStats LLStatRandomizer(string className, string seed)
    {
        const int statusPool = 39; // vigor, mind, endurance
        const int physicalPool = 55; // strength, dexterity, arcane
        const int spellPool = 30; // intelligence, faith

        var status = DistributePool(seed + "_status_" + className, statusPool, 3);
        var physical = DistributePool(seed + "_physical_" + className, physicalPool, 3);
        var spell = DistributePool(seed + "_spell_" + className, spellPool, 2);

        return new StartingStats
        {
            Vigor = status[0],
            Mind = status[1],
            Endurance = status[2],
            Strength = physical[0],
            Dexterity = physical[1],
            Arcane = physical[2],
            Intelligence = spell[0],
            Faith = spell[1],
            RuneLevel = 1,
        };
    }

    // Some Probability Distribution i dont a 100% understand
    private static int[] DistributePool(string seed, int pool, int count)
    {
        const int statMin = 10;
        const double sigma = 4.0; // spread around the mean; higher = more variance

        if (pool < statMin * count)
            throw new ArgumentOutOfRangeException(
                nameof(pool),
                $"Pool {pool} is too small for {count} stats with a minimum of {statMin}."
            );

        var mean = (double)pool / count;
        var values = new int[count];

        // Sample each stat from N(mean, sigma), floored at statMin
        for (var i = 0; i < count; i++)
        {
            var z = NextGaussian(seed + "_stat" + i);
            values[i] = Math.Max(statMin, (int)Math.Round(mean + z * sigma));
        }

        // Nudge random stats by 1 until the total matches the pool exactly
        var fix = 0;
        while (values.Sum() != pool)
        {
            var needMore = values.Sum() < pool;
            var eligible = Enumerable
                .Range(0, count)
                .Where(i => needMore || values[i] > statMin)
                .ToList();

            var pick = Utils.RandoUtils.GetRandomNumber(seed + "_fix" + fix++, eligible.Count);
            values[eligible[pick]] += needMore ? 1 : -1;
        }

        return values;
    }

    // Deterministic standard normal sample (Box-Muller) from the seed
    private static double NextGaussian(string seed)
    {
        const int Resolution = 1_000_000;
        var u1 =
            (Utils.RandoUtils.GetRandomNumber(seed + "_u1", Resolution) + 1) / (Resolution + 1.0);
        var u2 =
            (Utils.RandoUtils.GetRandomNumber(seed + "_u2", Resolution) + 1) / (Resolution + 1.0);
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}
