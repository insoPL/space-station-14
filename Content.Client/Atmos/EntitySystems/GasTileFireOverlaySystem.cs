using Content.Client.Atmos.Overlays;
using Content.Shared.Atmos.EntitySystems;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.Client.Atmos.EntitySystems;

[UsedImplicitly]

public sealed class GasTileFireOverlaySystem : EntitySystem
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly SpriteSystem _spriteSys = default!;
    [Dependency] private readonly SharedTransformSystem _xformSys = default!;
    [Dependency] private readonly GasTileOverlaySystem _networkedGasTileOverlay = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private GasTileFireOverlay _fireOverlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _fireOverlay = new GasTileFireOverlay(_networkedGasTileOverlay, EntityManager, _resourceCache, _protoMan, _spriteSys, _xformSys);
        _overlayMan.AddOverlay(_fireOverlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMan.RemoveOverlay<GasTileFireOverlay>();
    }

}
