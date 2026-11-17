using Hades.ViewModels;

namespace Hades.Formats;

public static class FormatRegistry
{
    public static IReadOnlyList<FormatEntry> AvailableFormats { get; } = BuildEntries();

    private static IReadOnlyList<FormatEntry> BuildEntries()
    {
        var sote3 = new ShowdownOfTheErdtree3();
        var twc = new TwoWorldsCollide();
        var ll = new LandLocked();

        return new FormatEntry[]
        {
            new FormatEntry(sote3, new ShowdownOfTheErdtree3ViewModel(sote3)),
            new FormatEntry(twc, new TwoWorldsCollideViewModel(twc)),
            new FormatEntry(ll, new LandLockedViewModel(ll)),
        };
    }
}
