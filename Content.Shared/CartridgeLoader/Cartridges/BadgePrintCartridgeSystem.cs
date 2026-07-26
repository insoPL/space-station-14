using Content.Shared.GPS.Components;

namespace Content.Shared.CartridgeLoader.Cartridges;

public sealed partial class BadgePrintCartridgeSystem : EntitySystem
{
    [Dependency] private CartridgeLoaderSystem _cartridgeLoaderSystem = default!;

    [SubscribeLocalEvent]
    private void OnCartridgeAdded(Entity<BadgePrintCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        EnsureComp<HandheldGPSComponent>(args.Loader);
    }

    [SubscribeLocalEvent]
    private void OnCartridgeRemoved(Entity<BadgePrintCartridgeComponent> ent, ref CartridgeRemovedEvent args)
    {
        // only remove when the program itself is removed
        if (!_cartridgeLoaderSystem.HasProgram<BadgePrintCartridgeComponent>(args.Loader.AsNullable()))
        {
            RemComp<HandheldGPSComponent>(args.Loader);
        }
    }
}
