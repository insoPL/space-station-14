using Content.Shared.Examine;
using Content.Shared.Materials;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Materials;

/// <inheritdoc/>
public sealed class MaterialReclaimerSystem : SharedMaterialReclaimerSystem
{
    private static readonly EntProtoId ExamineArrow = "TurnstileArrow";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RecyclerVisualsComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<RecyclerVisualsComponent> ent, ref ExaminedEvent args)
    {
        Spawn(ExamineArrow, new EntityCoordinates(ent, 0, 0));
        Spawn(ExamineArrow, new EntityCoordinates(ent, 0, -1.35f));
    }
}
