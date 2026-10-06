using System.Security.Cryptography;
using System.Text;
using Hades.Commands;
using Hades.ViewModels;

namespace Hades.Formats;

public static class FormatRegistry
{
    public static IReadOnlyList<FormatEntry> AvailableFormats { get; } = BuildEntries();

    private static IReadOnlyList<FormatEntry> BuildEntries()
    {
        // SOTE3
        var sote3 = new ShowdownOfTheErdtree3();
        var sote3Vm = new ShowdownOfTheErdtree3ViewModel { Title = sote3.DisplayName };

        sote3Vm.RandomizeCommand = new RelayCommand(() =>
        {
            sote3.Exec(
                baseSeed: sote3Vm.Seed,
                statusCallback: status => sote3Vm.StatusText = status,
                seedCallback: seed => sote3Vm.Seed = seed
            );
        });
        sote3Vm.LaunchCommand = new RelayCommand(() =>
        {
            sote3.Launch();
        });

        // Two Worlds Collide
        var twc = new TwoWorldsCollide();
        var twcVm = new TwoWorldsCollideViewModel { Title = twc.DisplayName };

        twcVm.RandomizeCommand = new RelayCommand(() =>
        {
            twc.Exec(
                baseSeed: twcVm.Seed,
                statusCallback: status => twcVm.StatusText = status,
                seedCallback: seed => twcVm.Seed = seed
            );
        });
        twcVm.LaunchCommand = new RelayCommand(() =>
        {
            twc.Launch();
        });

        // Land Locked
        var landLocked = new LandLocked();
        var landLockedVm = new LandLockedViewModel { Title = landLocked.DisplayName };

        landLockedVm.RandomizeCommand = new RelayCommand(() =>
        {
            landLocked.SquaresToGenerate = landLockedVm.SquaresToGenerate;
            landLocked.JsonFilePath = landLockedVm.JsonFilePath;
            landLocked.Exec(
                baseSeed: landLockedVm.Seed,
                statusCallback: status => landLockedVm.StatusText = status,
                seedCallback: seed => landLockedVm.Seed = seed
            );
        });
        landLockedVm.LaunchCommand = new RelayCommand(() =>
        {
            landLocked.Launch();
        });
        landLockedVm.BrowseJsonCommand = new RelayCommand(() =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                CheckFileExists = true,
            };
            if (dialog.ShowDialog() == true)
            {
                landLockedVm.JsonFilePath = dialog.FileName;
            }
        });

        return new FormatEntry[] { new FormatEntry(sote3, sote3Vm), new FormatEntry(twc, twcVm), new FormatEntry(landLocked, landLockedVm) };
    }
}
