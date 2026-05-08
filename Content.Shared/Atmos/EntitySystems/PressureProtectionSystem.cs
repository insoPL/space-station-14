using Content.Shared.Atmos.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Verbs;

namespace Content.Shared.Atmos.EntitySystems;

public sealed class PressureProtectionSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PressureProtectionComponent, GetVerbsEvent<ExamineVerb>>(OnDetailedExamine);
    }
    private void OnDetailedExamine(EntityUid ent, PressureProtectionComponent component, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!HasComp<ClothingComponent>(ent) || component.LowPressureMultiplier < 1000)
            return;

        var iconTexture = "/Textures/Interface/VerbIcons/zap.svg.192dpi.png";

        _examine.AddHoverExamineVerb(args,
            component,
            Loc.GetString("pressure-protection-examinable-verb-text"),
            Loc.GetString("pressure-protection-examinable-verb-text-message"),
            iconTexture
        );
    }

}
