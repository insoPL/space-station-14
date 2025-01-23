using Content.Server.Kitchen.Components;
using Content.Server.Medical.Components;
using Content.Shared.Chemistry;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Kitchen;
using Content.Shared.Medical;
using JetBrains.Annotations;
using Robust.Shared.Containers;

namespace Content.Server.Medical
{
    [UsedImplicitly]
    internal sealed class IvDripSystem : EntitySystem
    {
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
        [Dependency] private readonly IEntityManager _entManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<IvDripComponent, EntInsertedIntoContainerMessage>(OnContainerModified);
            SubscribeLocalEvent<IvDripComponent, EntRemovedFromContainerMessage>(OnContainerModified);
        }

        private void OnContainerModified(EntityUid uid, IvDripComponent ivDripComponent, ContainerModifiedMessage args)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(uid, SharedIvDrip.BagSlotId);
            _appearanceSystem.SetData(uid, IvDripVisualState.BagAttached, outputContainer.HasValue);
            if (outputContainer.HasValue)
            {
                var appearanceComponent = _entManager.GetComponent<AppearanceComponent>(outputContainer.Value);

                _appearanceSystem.TryGetData(outputContainer.Value, SolutionContainerVisuals.Color, out var color);
                _appearanceSystem.TryGetData(outputContainer.Value, SolutionContainerVisuals.FillFraction, out var fraction);

                if (color != null)
                    _appearanceSystem.SetData(uid, IvDripVisualState.Color, color);
                if (fraction != null)
                    _appearanceSystem.SetData(uid, IvDripVisualState.FillFraction, fraction);
            }
        }
    }
}
