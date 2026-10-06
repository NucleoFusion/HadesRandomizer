using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Hades.ViewModels;

public class LandLockedViewModel : INotifyPropertyChanged
{
    public string Title { get; set; } = "";
    private string _seed = "";
    public string Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            OnPropertyChanged();
        }
    }
    private string _statusText = "";
    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged();
        }
    }

    private int _squaresToGenerate = 25;
    public int SquaresToGenerate
    {
        get => _squaresToGenerate;
        set
        {
            _squaresToGenerate = value;
            OnPropertyChanged();
        }
    }

    private string _jsonFilePath = "";
    public string JsonFilePath
    {
        get => _jsonFilePath;
        set
        {
            _jsonFilePath = value;
            OnPropertyChanged();
        }
    }

    public ICommand RandomizeCommand { get; set; } = null!;
    public ICommand LaunchCommand { get; set; } = null!;
    public ICommand BrowseJsonCommand { get; set; } = null!;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
