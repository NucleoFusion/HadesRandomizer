using System.IO;
using EldenRingParamsEditor;
using Hades.Constants;
using Hades.Services;

namespace Hades.Formats;

class TwoWorldsCollide : IRandomizerFormat
{
    public string Id => "twc";
    public string DisplayName => "Two Worlds Collide";
    public string Me3File => "twc.me3";
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
        baseSeed = $"twc_{baseSeed}";

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

        var editor = ParamsEditor.ReadFromRegulationPath(regulationFilepath);

        // Randomization Logic
        var swTotal = System.Diagnostics.Stopwatch.StartNew();

        var urr = new UniversalReplacementRandomizer.OptimizedReplacementRandomizer(
            "twc",
            Utils.RandoUtils.GetStableSeed(baseSeed)
        );

        // Weapons
        var allNormalWeapons = Weapons.GuaranteedWeapons.Concat(Weapons.ChanceWeapons).ToList();

        var weaponCategories = allNormalWeapons
            .GroupBy(w => w.Category)
            .OrderBy(g => g.Key.ToString())
            .ToList();
        foreach (var group in weaponCategories)
        {
            var category = group.Key;
            var weaponsInCategory = group.ToList();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            WeaponRandomizerService.RandomizeWeaponWorldGroup(
                editor,
                urr,
                $"weapon_category_{category}",
                weaponsInCategory,
                weaponsInCategory
            );
            sw.Stop();
        }

        // Remembrance Weapons
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ShopRandomizerService.RandomizeRemembranceWeaponShops(
                editor,
                urr,
                "shop_remembrance_weapons"
            );
            sw.Stop();
        }

        // Spells
        var worldSpells = Spells.WorldSpells;
        var remSpells = Spells.RemembranceSpells;
        var spellGroups = worldSpells.GroupBy(s => s.Type).OrderBy(g => g.Key.ToString()).ToList();
        foreach (var group in spellGroups)
        {
            var spellType = group.Key;
            var spellsInType = group.ToList();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            SpellRandomizerService.RandomizeSpellWorldGroup(
                editor,
                urr,
                $"spell_type_{spellType}",
                spellsInType,
                spellsInType
            );
            sw.Stop();
        }

        // Weapons in Shop
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ShopRandomizerService.RandomizeWeaponShopsByCategory(
                editor,
                urr,
                "shop_weapon_category_",
                includeRemembranceInPool: false
            );
            sw.Stop();
        }
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ShopRandomizerService.RandomizeSpellShopsByCategory(editor, urr);
            sw.Stop();
        }
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ShopRandomizerService.RandomizeRemembranceSpellShops(editor, urr);
            sw.Stop();
        }

        // Class Armor
        var classResult = ArmorRandomizerService.GetRandomArmoredClasses(
            baseSeed,
            ArmorLocation.Both
        );
        randomizeClasses(editor, classResult);

        // Starting Stats
        StartingClassService.RandomizeStats(editor, baseSeed + "_statrando", StatRandomizer);

        // Class Weapons + deficit description (WeaponName (-X) with 2H Str buff)
        StartingClassService.RandomizeAllStartingWeaponsWithDescriptions(
            editor,
            baseSeed + "_classweapons",
            Id
        );

        // Twin Maiden Shop
        RandomizeTwinMaidenShop(editor, baseSeed + "_twinmaiden");

        editor.WriteToRegulationPath(regulationFilepath);

        statusCallback?.Invoke("Successfully Randomized: " + baseSeed);
    }

    public void Launch()
    {
        _launcherService.LaunchEldenRingFromMe3File(Me3File);
    }

    public void RandomizeTwinMaidenShop(ParamsEditor editor, string seed)
    {
        // Talismans
        {
            var fillerTalis = TWCConstants.FillerTalismans;
            var goodTalis = TWCConstants.GoodTalismans;

            for (int i = 0; i < 2; i++)
            {
                var shopID = TWCConstants.ShopLineupMap[$"talisman_{i + 1}"];

                var ind = Utils.RandoUtils.GetRandomNumber(
                    seed + $"_filler_talisman_{i + 1}",
                    fillerTalis.Count()
                );

                editor.SetShopLineupEquipId(shopID, fillerTalis[ind]);
                fillerTalis.RemoveAt(ind);
            }

            for (int i = 2; i < 4; i++)
            {
                var shopID = TWCConstants.ShopLineupMap[$"talisman_{i + 1}"];

                var ind = Utils.RandoUtils.GetRandomNumber(
                    seed + $"_good_talisman_{i + 1}",
                    goodTalis.Count()
                );

                editor.SetShopLineupEquipId(shopID, goodTalis[ind]);
                goodTalis.RemoveAt(ind);
            }
        }

        //  Physick Tear
        {
            var tears = TWCConstants.PhysickTearPool;

            for (int i = 0; i < 4; i++)
            {
                var shopID = TWCConstants.ShopLineupMap[$"physick_{i + 1}"];

                var ind = Utils.RandoUtils.GetRandomNumber(
                    seed + $"_physick_{i + 1}",
                    tears.Count()
                );

                editor.SetShopLineupEquipId(shopID, tears[ind]);
                tears.RemoveAt(ind);
            }
        }
    }

    public StartingStats StatRandomizer(string className, string seed)
    {
        static int GenerateStat(string seed, int min, int max, double mean, double stdDev)
        {
            const int maxAttempts = 300;
            for (var attempt = 0; ; attempt++)
            {
                var s = seed + "_stat" + attempt;

                var u1 = (Utils.RandoUtils.GetRandomNumber(s + "_u1", 10000) + 1) / 10001.0;
                var u2 = Utils.RandoUtils.GetRandomNumber(s + "_u2", 10000) / 10000.0;

                var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                var stat = (int)Math.Round(mean + stdDev * z);

                if (stat >= min && stat <= max)
                    return stat;
                if (attempt >= maxAttempts)
                    throw new RandomizerException(
                        "Failed to generate stats, attempt maximum reached."
                    );
            }
        }

        int total = 120;
        int statMin = 8;
        int statMax = 23;
        double mean = 15.0;
        double stdDev = 5.0;
        const int maxAttempts = 300;

        string[] keys = { "vigor", "mind", "end", "str", "dex", "int", "fai", "arc" };
        int[] stats = new int[keys.Length];

        for (var attempt = 0; ; attempt++)
        {
            var sum = 0;
            for (var i = 0; i < keys.Length; i++)
            {
                string classedSeed = $"{seed}_{className}_{keys[i]}_{attempt}";
                stats[i] = GenerateStat(classedSeed, statMin, statMax, mean, stdDev);
                sum += stats[i];
            }
            if (sum == total)
                break;
            if (attempt >= maxAttempts)
                throw new RandomizerException("Failed to generate stats, attempt maximum reached.");
        }

        int vigorMod = 5;
        int mindMod = 5;

        return new StartingStats
        {
            Vigor = stats[0] + vigorMod,
            Mind = stats[1] - mindMod,
            Endurance = stats[2],
            Strength = stats[3],
            Dexterity = stats[4],
            Intelligence = stats[5],
            Faith = stats[6],
            Arcane = stats[7],
        };
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
}
