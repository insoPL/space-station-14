using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed partial class BadgePrintCartridgeSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    [SubscribeLocalEvent]
    private void OnPrintMessage(EntityUid uid, BadgePrintCartridgeComponent component, CartridgeMessageEvent args)
    {
        if (args is not BadgePrintUiMessageEvent message)
            return;

        var badgePrototype = GetDepartmentPrototype(message.Dept);
        if (badgePrototype == null)
            return;

        var coords = _transform.GetMapCoordinates(uid);
        var badge = Spawn(badgePrototype, coords);

        if (TryComp<TemporaryAccessComponent>(badge, out var tempAccess))
        {
            var duration = GetTimerSpan(message.Timer);
            tempAccess.AccessExpireTime = duration;
            tempAccess.ExpireTime = _timing.CurTime + duration;

            Dirty(badge, tempAccess);
        }

        var user = GetUserWithHands(uid);
        if (user != null)
        {
            _hands.TryPickupAnyHand(user.Value, badge);
        }
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
            SelectedDepartment.Bridge => "CommandAccessBadge",
            SelectedDepartment.Service => "ServiceAccessBadge",
            _ => "AccessBadge"
        };
    }

    /// <summary>
    /// Climbs the transform tree from the Cartridge -> PDA -> Player to find their hands.
    /// </summary>
    private EntityUid? GetUserWithHands(EntityUid cartridgeUid)
    {
        var current = Transform(cartridgeUid).ParentUid;

        while (current.IsValid())
        {
            if (HasComp<HandsComponent>(current))
                return current;

            current = Transform(current).ParentUid;
        }

        return null;
    }
}
