using Content.Client.Atmos.Overlays;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.Client.Atmos.EntitySystems;

/// <summary>
///     System responsible for rendering visible atmos gasses (like plasma for example) using <see cref="GasTileVisibleGasOverlay"/>.
/// </summary>
[UsedImplicitly]
public sealed class GasTileHeatBlurOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private GasTileHeatBlurOverlay _gasTileHeatBlurOverlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _gasTileHeatBlurOverlay = new GasTileHeatBlurOverlay();
        _overlayMan.AddOverlay(_gasTileHeatBlurOverlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMan.RemoveOverlay<GasTileHeatBlurOverlay>();
    }

}
