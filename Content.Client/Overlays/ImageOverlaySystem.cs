using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;
using Robust.Client.Graphics;
using Robust.Client.Timing;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Client.Overlays;


/// <summary>
/// Adds image overlay when wearing item with ImageOverlayComponent
/// </summary>
public sealed partial class ImageOverlaySystem : EquipmentHudSystem<ImageOverlayComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IClientGameTiming _gameTiming = default!;

    public static readonly ProtoId<ShaderPrototype> ImageShader = "ImageMask";
    private ImageOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ImageOverlayComponent, ItemWieldedEvent>(OnEyeOffsetWielded);
        SubscribeLocalEvent<ImageOverlayComponent, ItemUnwieldedEvent>(OnEyeOffsetUnwielded);
        SubscribeLocalEvent<ImageOverlayComponent, GotUnequippedHandEvent>(OnUnequippedHand);

        _overlay = new();
    }

    private void OnUnequippedHand(Entity<ImageOverlayComponent> entity, ref GotUnequippedHandEvent args)
    {
        DeactivateInternal();
    }

    private void OnEyeOffsetUnwielded(Entity<ImageOverlayComponent> entity, ref ItemUnwieldedEvent args)
    {
        if (_gameTiming.IsFirstTimePredicted)
            DeactivateInternal();
    }

    private void OnEyeOffsetWielded(Entity<ImageOverlayComponent> entity, ref ItemWieldedEvent args)
    {
        _overlay.ImageShaders.Clear();
        var values = new ImageShaderValues
        {
            PathToOverlayImage = entity.Comp.PathToOverlayImage,
            AdditionalColorOverlay = entity.Comp.AdditionalColorOverlay
        };
        _overlay.ImageShaders.Add((_prototypeManager.Index(ImageShader).InstanceUnique(), values));

        _overlayMan.AddOverlay(_overlay);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<ImageOverlayComponent> component)
    {
        base.UpdateInternal(component);

        _overlay.ImageShaders.Clear();

        foreach (var comp in component.Components)
        {
            var values = new ImageShaderValues
            {
                PathToOverlayImage = comp.PathToOverlayImage,
                AdditionalColorOverlay = comp.AdditionalColorOverlay
            };
            _overlay.ImageShaders.Add((_prototypeManager.Index(ImageShader).InstanceUnique(), values));
        }

        _overlayMan.AddOverlay(_overlay);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        _overlay.ImageShaders.Clear();

        _overlayMan.RemoveOverlay(_overlay);
    }
}
