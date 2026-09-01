using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Coordinates;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.CartridgeLoader.Cartridges;
//TODO
//Normal icons and inhands

public sealed partial class BadgePrintCartridgeSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
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

        if (!CanPrintBadge(badgePrintCartridgeComponent, dept, user))
        {
            _audio.PlayPredicted(badgePrintCartridgeComponent.DenySound, badgePrintCartridgeUid, user);
            return;
        }

        PrintBadge(badgePrintCartridgeUid, badgePrintCartridgeComponent, user, dept, timer);
    }

    private bool CanPrintBadge(BadgePrintCartridgeComponent badgePrintCartridgeComponent, SelectedDepartment dept, EntityUid user)
    {
        if (_timing.CurTime < badgePrintCartridgeComponent.NextPrintAllowedAfter)
            return false;

        var accessTagName = GetAccessTagName(dept);
        if (accessTagName == null)
            return false;

        var accessItems = _accessReader.FindPotentialAccessItems(user);
        var userAccessTag = _accessReader.FindAccessTags(user, accessItems);

        if (!userAccessTag.Contains(new ProtoId<AccessLevelPrototype>(accessTagName)))
        {
            return false;
        }

        return true;
    }

    private void PrintBadge(EntityUid badgePrintCartridgeUid, BadgePrintCartridgeComponent badgePrintCartridgeComponent, EntityUid user, SelectedDepartment dept, SelectedBadgeTimer timer)
    {
        var badgePrototype = GetDepartmentPrototype(dept);
        if (badgePrototype == null)
        {
            Log.Error($"Invalid department selected for badge printing: {dept}");
            return;
        }

        var badge = PredictedSpawnAtPosition(badgePrototype, badgePrintCartridgeUid.ToCoordinates());
        _hands.PickupOrDrop(user, badge);

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
    }

    //region: maping functions

    /// <summary>
    /// Maps the UI's department enum to the access tags
    /// </summary>
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
    //endregion
}
