using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.CartridgeLoader.Cartridges;
//TODO bug inhand while spawning
//Normal icons and inhands
//extract canPrint() method

public sealed partial class BadgePrintCartridgeSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private AccessReaderSystem _accessReader = default!;

    [SubscribeLocalEvent]
    private void OnPrintMessage(EntityUid badgePrintCartridgeUid, BadgePrintCartridgeComponent badgePrintCartridgeComponent, CartridgeMessageEvent args)
    {
        if (args is not BadgePrintUiMessageEvent message)
            return;
        var dept = message.Dept;
        var timer = message.Timer;
        var user = args.User;
        if (_timing.CurTime < badgePrintCartridgeComponent.NextPrintAllowedAfter)
            return;

        var accessTagName = GetAccessTagName(dept);
        if (accessTagName == null)
            return;

        var accessItems = _accessReader.FindPotentialAccessItems(user);
        var tags = _accessReader.FindAccessTags(user, accessItems);

        // Wrap the string in a ProtoId so it matches the collection type
        //this can be done better propably
        if (!tags.Contains(new ProtoId<AccessLevelPrototype>(accessTagName)))
        {
            return;
        }

        PrintBadge(badgePrintCartridgeUid, badgePrintCartridgeComponent, user, dept, timer);
    }

    private static string? GetAccessTagName(SelectedDepartment dept)
    {
        return dept switch
        {
            SelectedDepartment.Bridge => "Command",
            SelectedDepartment.Security => "Security",
            SelectedDepartment.Medical => "Medical",
            SelectedDepartment.Engineering => "Engineering",
            SelectedDepartment.Science => "Research",
            SelectedDepartment.Cargo => "Cargo",
            SelectedDepartment.Service => "Service",
            SelectedDepartment.All => "Captain",
            _ => null
        };
    }

    private void PrintBadge(EntityUid badgePrintCartridgeUid, BadgePrintCartridgeComponent badgePrintCartridgeComponent, EntityUid user, SelectedDepartment dept, SelectedBadgeTimer timer)
    {
        var badgePrototype = GetDepartmentPrototype(dept);
        if (badgePrototype == null)
        {
            Log.Error($"Invalid department selected for badge printing: {dept}");
            return;
        }

        var coords = _transform.GetMapCoordinates(badgePrintCartridgeUid);
        var badge = Spawn(badgePrototype, coords);

        badgePrintCartridgeComponent.NextPrintAllowedAfter = _timing.CurTime + badgePrintCartridgeComponent.PrintDelay;
        Dirty(badgePrintCartridgeUid, badgePrintCartridgeComponent);

        _audio.PlayPredicted(badgePrintCartridgeComponent.PrintSound, badgePrintCartridgeUid, user);

        if (TryComp<TemporaryAccessComponent>(badge, out var tempAccess))
        {
            var duration = GetTimerSpan(timer);
            tempAccess.AccessExpireTime = duration;
            tempAccess.ExpireTime = _timing.CurTime + duration;

            Dirty(badge, tempAccess);
        }

        _hands.TryPickupAnyHand(user, badge);
    }

    /// <summary>
    /// Maps the UI's time enum to actual TimeSpans.
    /// </summary>
    private static TimeSpan GetTimerSpan(SelectedBadgeTimer timer)
    {
        return timer switch
        {
            SelectedBadgeTimer.Print5 => TimeSpan.FromMinutes(5),
            SelectedBadgeTimer.Print10 => TimeSpan.FromMinutes(10),
            SelectedBadgeTimer.Print15 => TimeSpan.FromMinutes(15),
            SelectedBadgeTimer.Print25 => TimeSpan.FromMinutes(25),
            _ => TimeSpan.FromMinutes(5)
        };
    }

    /// <summary>
    /// Maps the UI's department enum to the prototype IDs defined in access_badge.yml
    /// </summary>
    private static string? GetDepartmentPrototype(SelectedDepartment dept)
    {
        return dept switch
        {
            SelectedDepartment.All => "AllAccessAccessBadge",
            SelectedDepartment.Security => "SecurityAccessBadge",
            SelectedDepartment.Engineering => "EngineeringAccessBadge",
            SelectedDepartment.Medical => "MedicalAccessBadge",
            SelectedDepartment.Science => "ResearchAccessBadge",
            SelectedDepartment.Cargo => "CargoAccessBadge",
            SelectedDepartment.Bridge => "BridgeAccessBadge",
            SelectedDepartment.Service => "ServiceAccessBadge",
            _ => null
        };
    }
}
