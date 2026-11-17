using System.IO;
using System.Linq;
using EldenRingParamsEditor;
using Hades.Constants;
using Hades.Utils;
using UniversalReplacementRandomizer;

namespace Hades.Services;

public static class StartingClassService
{
    private static readonly string[] ClassNames =
    {
        "Vagabond",
        "Warrior",
        "Hero",
        "Bandit",
        "Astrologer",
        "Prophet",
        "Confessor",
        "Samurai",
        "Prisoner",
        "Wretch",
        "Heavy Knight",
        "Idus Knight",
    };

    public static void RandomizeAllStartingWeapons(ParamsEditor editor, string seed)
    {
        foreach (var className in ClassNames)
        {
            RandomizeStartingWeapons(editor, className, seed + "_" + className);
        }
    }

    // formatId: the format's folder under ModEngineWorkingDirectory; the menu is written there.
    public static void RandomizeAllStartingWeaponsWithDescriptions(
        ParamsEditor editor,
        string seed,
        string formatId
    )
    {
        RandomizeAllStartingWeapons(editor, seed);
        WriteStartingWeaponDescriptions(editor, formatId);
    }

    public static void WriteStartingWeaponDescriptions(ParamsEditor editor, string formatId)
    {
        MenuBndEditorService? menuEditor = null;
        var candidates = new[]
        {
            Path.Combine(
                GlobalConstants.ModEngineWorkingDirectory,
                formatId,
                "bingo",
                "msg",
                "engus",
                "menu_dlc02.msgbnd.dcx"
            ),
            Path.Combine(
                GlobalConstants.ModEngineWorkingDirectory,
                formatId,
                "msg",
                "engus",
                "menu_dlc02.msgbnd.dcx"
            ),
        };
        var menuOut = Path.Combine(
            GlobalConstants.ModEngineWorkingDirectory,
            formatId,
            "bingo",
            "msg",
            "engus",
            "menu_dlc02.msgbnd.dcx"
        );
        var altIn = Path.Combine("Resources", "Bnd", "vanilla", "menu_dlc02.msgbnd.dcx");
        string? menuIn = candidates.FirstOrDefault(File.Exists);
        if (menuIn != null)
            menuEditor = MenuBndEditorService.ReadFromMenuBndFilePath(menuIn);
        else if (File.Exists(altIn))
            menuEditor = MenuBndEditorService.ReadFromMenuBndFilePath(altIn);
        else
            return;
        if (menuEditor == null)
            return;
        for (int i = 0; i < ParamsEditor.TotalStartingClasses; i++)
        {
            int charaInitId = ParamsEditor.VagabondCharaInitId + i;
            int[] wepIds =
            {
                editor.GetInitialEquipWepRight(charaInitId, 0),
                editor.GetInitialEquipWepRight2(charaInitId),
                editor.GetInitialEquipWepLeft(charaInitId, 0),
                editor.GetInitialEquipWepLeft2(charaInitId),
            };
            int[] buffed = GetBuffedStats(editor, charaInitId);
            var descs = new System.Collections.Generic.List<string>();
            foreach (int wep in wepIds)
            {
                if (wep == -1)
                    continue;
                var entry = Weapons.AllWeapons.FirstOrDefault(w => w.Id == wep);
                string name = entry.Id != 0 ? entry.Name : $"Wep{wep}";
                descs.Add(GetWeaponDescriptionWithDeficit(editor, wep, name, buffed));
            }
            if (descs.Count == 0)
                continue;
            menuEditor.SetClassDescription(i, string.Join(", ", descs));
        }
        for (int i = 10; i < ParamsEditor.TotalStartingClasses && i < ClassNames.Length; i++)
            menuEditor.SetClassName(i, ClassNames[i]);
        menuEditor.WriteToMenuBndFilePath(menuOut);
    }

    public static void RandomizeStartingWeapons(ParamsEditor editor, string className, string seed)
    {
        int charaInitId = GlobalConstants.CharaInitClassMap[className];

        int wep_right_1 = editor.GetInitialEquipWepRight(charaInitId, 0);
        int wep_right_2 = editor.GetInitialEquipWepRight2(charaInitId);
        int wep_left_1 = editor.GetInitialEquipWepLeft(charaInitId, 0);
        int wep_left_2 = editor.GetInitialEquipWepLeft2(charaInitId);

        editor.SetInitialEquipWepLeft(
            charaInitId,
            0,
            GetRandomStartingWeapon(wep_left_1, seed + "_wep_left_1")
        );
        editor.SetInitialEquipWepLeft(
            charaInitId,
            1,
            GetRandomStartingWeapon(wep_left_2, seed + "_wep_left_2")
        );
        editor.SetInitialEquipWepRight(
            charaInitId,
            0,
            GetRandomStartingWeapon(wep_right_1, seed + "_wep_right_1")
        );
        editor.SetInitialEquipWepRight(
            charaInitId,
            1,
            GetRandomStartingWeapon(wep_right_2, seed + "_wep_right_2")
        );
    }

    public static void RandomizeAllStartingWeaponsWieldable(ParamsEditor editor, string seed)
    {
        foreach (var className in ClassNames)
        {
            RandomizeStartingWeaponsWieldable(editor, className, seed + "_" + className);
        }
    }

    public static void RandomizeAllStartingWeaponsWieldableWithDescriptions(
        ParamsEditor editor,
        string seed,
        string modId
    )
    {
        RandomizeAllStartingWeaponsWieldable(editor, seed);
        WriteStartingWeaponDescriptions(editor, modId);
    }

    public static int[] GetClassStats(ParamsEditor editor, int charaInitId)
    {
        return new[]
        {
            (int)editor.GetInitialVigor(charaInitId),
            (int)editor.GetInitialMind(charaInitId),
            (int)editor.GetInitialEndurance(charaInitId),
            (int)editor.GetInitialStrength(charaInitId),
            (int)editor.GetInitialDexterity(charaInitId),
            (int)editor.GetInitialIntelligence(charaInitId),
            (int)editor.GetInitialFaith(charaInitId),
            (int)editor.GetInitialArcane(charaInitId),
        };
    }

    public static int[] GetBuffedStats(ParamsEditor editor, int charaInitId)
    {
        // Str-first order to match GetWeaponDescriptionWithDeficit (j = 0..4).
        int[] stats =
        {
            (int)editor.GetInitialStrength(charaInitId),
            (int)editor.GetInitialDexterity(charaInitId),
            (int)editor.GetInitialIntelligence(charaInitId),
            (int)editor.GetInitialFaith(charaInitId),
            (int)editor.GetInitialArcane(charaInitId),
        };
        int[] armorIds =
        {
            editor.GetInitialEquipHelm(charaInitId),
            editor.GetInitialEquipTorso(charaInitId),
            editor.GetInitialEquipArm(charaInitId),
            editor.GetInitialEquipLeg(charaInitId),
        };
        foreach (int armorId in armorIds)
        {
            if (armorId <= 0)
                continue;
            int spEffectId;
            try
            {
                spEffectId = editor.GetEquipProtectorResidentSpEffectId(armorId);
            }
            catch
            {
                continue;
            }
            if (spEffectId == -1)
                continue;
            for (int j = 0; j < 5; j++)
            {
                try
                {
                    stats[j] += editor.GetSpEffectAddStat(spEffectId, j + 3);
                }
                catch { }
            }
        }
        return stats;
    }

    public static void RandomizeStartingWeaponsWieldable(
        ParamsEditor editor,
        string className,
        string seed
    )
    {
        int charaInitId = GlobalConstants.CharaInitClassMap[className];
        int[] buffed = GetClassStats(editor, charaInitId);

        int wep_right_1 = editor.GetInitialEquipWepRight(charaInitId, 0);
        int wep_right_2 = editor.GetInitialEquipWepRight2(charaInitId);
        int wep_left_1 = editor.GetInitialEquipWepLeft(charaInitId, 0);
        int wep_left_2 = editor.GetInitialEquipWepLeft2(charaInitId);

        editor.SetInitialEquipWepLeft(
            charaInitId,
            0,
            GetRandomStartingWeaponWieldable(editor, wep_left_1, seed + "_wep_left_1", buffed)
        );
        editor.SetInitialEquipWepLeft(
            charaInitId,
            1,
            GetRandomStartingWeaponWieldable(editor, wep_left_2, seed + "_wep_left_2", buffed)
        );
        editor.SetInitialEquipWepRight(
            charaInitId,
            0,
            GetRandomStartingWeaponWieldable(editor, wep_right_1, seed + "_wep_right_1", buffed)
        );
        editor.SetInitialEquipWepRight(
            charaInitId,
            1,
            GetRandomStartingWeaponWieldable(editor, wep_right_2, seed + "_wep_right_2", buffed)
        );
    }

    public static int GetRandomStartingWeaponWieldable(
        ParamsEditor editor,
        int weaponId,
        string seed,
        int[] buffedStats
    )
    {
        if (weaponId == -1)
        {
            return -1;
        }

        // Staff
        if (WeaponUtils.IsStaff(weaponId))
        {
            return PickWieldableWeapon(editor, WeaponUtils.StaffIds, seed + "_staff", buffedStats);
        }

        // Seal
        if (WeaponUtils.IsSeal(weaponId))
        {
            return PickWieldableWeapon(editor, WeaponUtils.SealIds, seed + "_seal", buffedStats);
        }

        return PickWieldableWeapon(
            editor,
            Weapons.GetAllSmithingWeapons().Select(w => w.Id).ToList(),
            seed,
            buffedStats
        );
    }

    private static int PickWieldableWeapon(
        ParamsEditor editor,
        IList<int> pool,
        string seed,
        int[] buffedStats
    )
    {
        var wieldable = pool.Where(id => GetWeaponDeficit(editor, id, buffedStats) == 0).ToList();
        if (wieldable.Count > 0)
        {
            return wieldable[RandoUtils.GetRandomNumber(seed, wieldable.Count)];
        }

        // Fallback: least deficit, ties broken by seed so it stays deterministic
        int min = pool.Min(id => GetWeaponDeficit(editor, id, buffedStats));
        var least = pool.Where(id => GetWeaponDeficit(editor, id, buffedStats) == min).ToList();
        return least[RandoUtils.GetRandomNumber(seed + "_fallback", least.Count)];
    }

    private static int GetWeaponDeficit(ParamsEditor editor, int weaponId, int[] buffedStats)
    {
        int deficit = 0;
        for (int j = 0; j < 5; j++)
        {
            int req;
            try
            {
                req = editor.GetEquipWeaponProperStat(weaponId, j);
            }
            catch
            {
                continue;
            }
            if (req == 0)
                continue;
            if (j == 0)
            {
                int num = req * 2;
                req = num % 3 > 0 ? num / 3 + 1 : num / 3;
            }
            if (req > buffedStats[j])
                deficit += req - buffedStats[j];
        }
        return deficit;
    }

    public static int GetRandomStartingWeapon(int weaponId, string seed)
    {
        if (weaponId == -1)
        {
            return -1;
        }

        // Staff
        if (WeaponUtils.IsStaff(weaponId))
        {
            return WeaponUtils.StaffIds[
                RandoUtils.GetRandomNumber(seed + "_staff", WeaponUtils.StaffIds.Count())
            ];
        }

        // Seal
        if (WeaponUtils.IsSeal(weaponId))
        {
            return WeaponUtils.SealIds[
                RandoUtils.GetRandomNumber(seed + "_seal", WeaponUtils.SealIds.Count())
            ];
        }

        return Weapons
            .GetAllSmithingWeapons()[
                RandoUtils.GetRandomNumber(seed, Weapons.GetAllSmithingWeapons().Count())
            ]
            .Id;
    }

    public static void RandomizeStats(ParamsEditor editor, string seed, ClassStatRoller getStats)
    {
        foreach (var className in ClassNames)
        {
            int charaInitId = GlobalConstants.CharaInitClassMap[className];
            var stats = getStats(className, seed);
            editor.SetInitialVigor(charaInitId, (byte)stats.Vigor);
            editor.SetInitialMind(charaInitId, (byte)stats.Mind);
            editor.SetInitialEndurance(charaInitId, (byte)stats.Endurance);
            editor.SetInitialStrength(charaInitId, (byte)stats.Strength);
            editor.SetInitialDexterity(charaInitId, (byte)stats.Dexterity);
            editor.SetInitialIntelligence(charaInitId, (byte)stats.Intelligence);
            editor.SetInitialFaith(charaInitId, (byte)stats.Faith);
            editor.SetInitialArcane(charaInitId, (byte)stats.Arcane);
            if (stats.RuneLevel is short level)
                editor.SetInitialRuneLevel(charaInitId, level);
        }
    }

    private static readonly System.Collections.Generic.HashSet<WeaponCategory> NoTwoHandBonusCategories =
        new() { WeaponCategory.Claw, WeaponCategory.Fist };

    private static bool AppliesTwoHandBonus(int weaponId)
    {
        var entry = Weapons.AllWeapons.FirstOrDefault(w => w.Id == weaponId);
        return entry.Id == 0 || !NoTwoHandBonusCategories.Contains(entry.Category);
    }

    public static string GetWeaponDescriptionWithDeficit(
        ParamsEditor editor,
        int weaponId,
        string weaponName,
        int[] buffedStats
    )
    {
        int deficit = 0;
        for (int j = 0; j < 5; j++)
        {
            int req;
            try
            {
                req = editor.GetEquipWeaponProperStat(weaponId, j);
            }
            catch
            {
                continue;
            }
            if (req == 0)
                continue;
            if (j == 0 && AppliesTwoHandBonus(weaponId))
            {
                int num = req * 2;
                req = num % 3 > 0 ? num / 3 + 1 : num / 3;
            }
            if (req > buffedStats[j])
                deficit += req - buffedStats[j];
        }
        return deficit > 0 ? $"{weaponName} (-{deficit})" : weaponName;
    }
}
