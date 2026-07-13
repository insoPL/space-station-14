using Content.Shared.Atmos.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Verbs;

namespace Content.Shared.Atmos.EntitySystems;

/// <summary>
///     A system for creating examine verbs for pressure protection clothing.
/// </summary>
public sealed partial class PressureProtectionSystem : EntitySystem
{
    [Dependency] private ExamineSystemShared _examine = default!;

    [SubscribeLocalEvent]
    private void OnDetailedExamine(EntityUid ent, PressureProtectionComponent component, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!HasComp<ClothingComponent>(ent) || component.LowPressureMultiplier < 1000)
            return;

        //var iconTexture = "/Textures/Interface/Alerts/pressure.rsi/lowpressure1.png";
        var iconTexture = "/Textures/Interface/Alerts/pressure.rsi/test.png";

        _examine.AddHoverExamineVerb(args,
            component,
            Loc.GetString("pressure-protection-examinable-verb-text"),
            Loc.GetString("pressure-protection-examinable-verb-text-message"),
            iconTexture
        );
    }

}
