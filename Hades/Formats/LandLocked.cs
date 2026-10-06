using System.IO;
using System.Linq;
using EldenRingParamsEditor;
using Hades.Constants;
using Hades.Services;

namespace Hades.Formats;

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
        Action<string>? statusCallback = null,
        Action<string>? seedCallback = null
    )
    {
        // TODO: add Land Locked randomization logic here.
        try
        {
            if (baseSeed == "")
            {
                var seed = Utils.RandoUtils.GenerateRandomString();
                seedCallback?.Invoke(seed);
                baseSeed = seed;
            }

            statusCallback?.Invoke($"seeding...: {baseSeed}");

            // Formatting seed
            baseSeed = $"landlocked_{baseSeed}";

            if (SquaresToGenerate < 1)
            {
                var msg = $"Invalid square count {SquaresToGenerate}. Expected at least 1.";
                statusCallback?.Invoke(msg);
                System.Windows.MessageBox.Show(
                    msg,
                    "Land Locked Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
                return;
            }

            if (!string.IsNullOrWhiteSpace(JsonFilePath) && !File.Exists(JsonFilePath))
            {
                var msg = $"JSON file not found at {Path.GetFullPath(JsonFilePath)}";
                statusCallback?.Invoke(msg);
                System.Windows.MessageBox.Show(
                    msg,
                    "Land Locked Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
                return;
            }

            // SquaresToGenerate (square count) and JsonFilePath are now available here, e.g.:
            // var count = SquaresToGenerate;
            // var json = string.IsNullOrWhiteSpace(JsonFilePath) ? null : File.ReadAllText(JsonFilePath);

            var regulationFilepath = Path.Combine(
                GlobalConstants.ModEngineWorkingDirectory,
                Id,
                "bingo",
                "regulation.bin"
            );
            if (!File.Exists(regulationFilepath))
            {
                var msg = $"regulation.bin not found at {Path.GetFullPath(regulationFilepath)}";
                statusCallback?.Invoke(msg);
                System.Windows.MessageBox.Show(
                    msg,
                    "Land Locked Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
                return;
            }

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

            statusCallback?.Invoke("successfully randomized!");
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(
                    "Hades.crash.log",
                    $"[{DateTime.Now}] LandLocked Exec crash: {ex}\n"
                );
            }
            catch { }
            statusCallback?.Invoke($"crashed: {ex.Message}");
            System.Windows.MessageBox.Show(
                $"Land Locked randomization crashed:\n{ex.Message}\n\nCheck Hades.crash.log for details.\n\n{ex}",
                "Hades Crash",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error
            );
        }
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
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Heavy Knight"], results.HeavyKnight);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Idus Knight"], results.IdusKnight);
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

    private static int LLStatRandomizer(string seed)
    {
        // stat to be between [10,16]
        return Utils.RandoUtils.GetRandomNumber(seed, 7) + 10;
    }
}
