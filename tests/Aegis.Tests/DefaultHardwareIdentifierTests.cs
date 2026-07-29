using Aegis.Utilities;

namespace Aegis.Tests;

public class DefaultHardwareIdentifierTests
{
    [Fact]
    public void GetHardwareIdentifier_uses_machine_name_prefix()
    {
        var identifier = new DefaultHardwareIdentifier().GetHardwareIdentifier();

        Assert.StartsWith($"{Environment.MachineName}-", identifier);
    }

    [Fact]
    public void GetHardwareIdentifier_is_stable_across_calls()
    {
        var identifier = new DefaultHardwareIdentifier();

        Assert.Equal(identifier.GetHardwareIdentifier(), identifier.GetHardwareIdentifier());
    }

    [Fact]
    public void ValidateHardwareIdentifier_matches_current_machine()
    {
        var identifier = new DefaultHardwareIdentifier();
        var hardwareId = identifier.GetHardwareIdentifier();

        Assert.True(identifier.ValidateHardwareIdentifier(hardwareId));
        Assert.False(identifier.ValidateHardwareIdentifier($"{hardwareId}-changed"));
    }
}
