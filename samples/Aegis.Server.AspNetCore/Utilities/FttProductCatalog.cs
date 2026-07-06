namespace Aegis.Server.AspNetCore.Utilities;

public static class FttProductCatalog
{
    public sealed record Entry(string BaseName, int MajorVersion, string Slug, string[] DefaultFeatures)
    {
        public string Name => $"{BaseName} {MajorVersion}";

        public string SoftwareUrn =>
            global::Aegis.Server.AspNetCore.Utilities.SoftwareUrn.Format(Slug, MajorVersion);
    }

    public static IReadOnlyList<Entry> All { get; } =
    [
        new("ConeCalc", 7, "conecalc", ["Test", "HeatFlux", "CFactor", "PrintReport"]),
        new("SBICalc", 3, "sbicalc", ["Test", "Reports"]),
        new("CableSoft", 3, "cablesoft", ["Core"]),
        new("IMOSoft", 3, "imosoft", ["Core"])
    ];

    public static Entry? FindByName(string name) =>
        All.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
