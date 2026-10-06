namespace Hades.Formats;

public class LLRegion
{
    public string regionName;
    public List<string> adjacentRegions;
};

public class LLConstants
{
    public readonly List<string> startingRegions =
    [
        "caelid",
        "limgrave",
        "liurnia",
        "altus",
        "nokron",
    ];
    public readonly List<LLRegion> regions = new List<LLRegion>
    {
        new LLRegion
        {
            regionName = "limgrave",
            adjacentRegions = new List<string> { "caelid", "weeping", "liurnia", "siofra" },
        },
        new LLRegion
        {
            regionName = "weeping",
            adjacentRegions = new List<string> { "limgrave" },
        },
        new LLRegion
        {
            regionName = "caelid",
            adjacentRegions = new List<string> { "limgrave", "nokron" },
        },
        new LLRegion
        {
            regionName = "liurnia",
            adjacentRegions = new List<string> { "limgrave", "altus", "ainsel", "lakeofrot" },
        },
        new LLRegion
        {
            regionName = "altus",
            adjacentRegions = new List<string> { "liurnia", "gelmir" },
        },
        new LLRegion
        {
            regionName = "capital",
            adjacentRegions = new List<string>
            {
                "altus",
                "forbiddenlands",
                "mountaintops",
                "consecrated",
            },
        },
        new LLRegion
        {
            regionName = "mountaintops",
            adjacentRegions = new List<string>
            {
                "consecrated",
                "forbiddenlands",
                "farum",
                "mohgwyn",
                "haligtree",
            },
        },
        new LLRegion
        {
            regionName = "farum",
            adjacentRegions = new List<string> { "mountaintops", "forbiddenlands", "consecrated" },
        },
        new LLRegion
        {
            regionName = "consecrated",
            adjacentRegions = new List<string>
            {
                "mountaintops",
                "forbiddenlands",
                "haligtree",
                "mohgwyn",
            },
        },
        new LLRegion
        {
            regionName = "nokron",
            adjacentRegions = new List<string> { "deeproot", "caelid", "siofra" },
        },
        new LLRegion
        {
            regionName = "ainsel",
            adjacentRegions = new List<string>
            {
                "moonlightaltar",
                "lakeofrot",
                "siofra",
                "deeproot",
            },
        },
        new LLRegion
        {
            regionName = "siofra",
            adjacentRegions = new List<string> { "limgrave", "nokron" },
        },
        new LLRegion
        {
            regionName = "deeproot",
            adjacentRegions = new List<string> { "ainsel", "nokron" },
        },
    };
}
