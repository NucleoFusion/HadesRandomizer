using CommunityToolkit.Mvvm.Input;
using Hades.Formats;
using Microsoft.Win32;

namespace Hades.ViewModels;

public sealed record LandLockedOptions(int SquaresToGenerate, string JsonFilePath)
    : IRandomizerOptions;

public class LandLockedViewModel : FormatViewModelBase
{
    public LandLockedViewModel(IRandomizerFormat format)
        : base(format)
    {
        BrowseJsonCommand = new RelayCommand(BrowseJson);
    }

    private int _squaresToGenerate = 25;
    public int SquaresToGenerate
    {
        get => _squaresToGenerate;
        set => SetProperty(ref _squaresToGenerate, value);
    }

    private string _jsonFilePath = "";
    public string JsonFilePath
    {
        get => _jsonFilePath;
        set => SetProperty(ref _jsonFilePath, value);
    }

    public IRelayCommand BrowseJsonCommand { get; }

    protected override IRandomizerOptions? Options =>
        new LandLockedOptions(SquaresToGenerate, JsonFilePath);

    private void BrowseJson()
    {
        var dlg = new OpenFileDialog { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*" };

        if (dlg.ShowDialog() == true)
            JsonFilePath = dlg.FileName;
    }
}
