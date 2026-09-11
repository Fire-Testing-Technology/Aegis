using System.ComponentModel;

namespace Aegis.Enums;

public enum LicenseType
{
    [DisplayName("Portable")]
    [Description("Perpetual (or dated) licence for a single user/installation. Not tied to a specific machine.")]
    Standard,

    [DisplayName("Concurrent")]
    [Description("Seat-based licence shared across users up to a maximum concurrent activations.")]
    Concurrent,

    [DisplayName("Instrument Locked")]
    [Description("Bound to one machine using the customer’s request code (hardware ID). Use this for ConeCalc and similar apps.")]
    NodeLocked,

    [DisplayName("Subscription")]
    [Description("Time-limited licence that must be renewed after the subscription period.")]
    Subscription,

    [DisplayName("Floating")]
    [Description("Network licence checked out while in use, returned when the client disconnects.")]
    Floating,

    [DisplayName("Trial")]
    [Description("Evaluation licence. Must be applied within 24 hours of issue. The trial period then starts on the customer’s first run.")]
    Trial
}
