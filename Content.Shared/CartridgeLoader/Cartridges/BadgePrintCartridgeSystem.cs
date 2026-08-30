using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed partial class BadgePrintCartridgeSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

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

        PrintBadge(badgePrintCartridgeUid, badgePrintCartridgeComponent, user, dept, timer);
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
    private TimeSpan GetTimerSpan(SelectedBadgeTimer timer)
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
    private string? GetDepartmentPrototype(SelectedDepartment dept)
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
