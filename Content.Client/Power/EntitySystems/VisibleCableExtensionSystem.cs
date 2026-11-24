using Content.Client.SubFloor;
using Content.Shared.Power.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Client.Power.VisibleCableExtension;

/// <summary>
/// Spawns a client-side visual line between an <see cref="ExtensionCableReceiverComponent"/>
/// and its connected <see cref="ExtensionCableReceiverComponent.Provider"/>.
/// </summary>
public sealed partial class VisibleCableExtensionSystem : EntitySystem
{
    private static readonly EntProtoId ExtensionLVProtoId = "VisibleCableExtensionLV";

    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SubFloorHideSystem _subFloor = default!;

    [Dependency] private EntityQuery<TransformComponent> _xformQuery = default!;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery = default!;

    [SubscribeLocalEvent]
    private void OnAfterAutoHandleState(Entity<ExtensionCableReceiverComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateCable(ent);
    }
    private static readonly float LvCableOffset = 0.22f;

    /// <summary>
    /// Spawns and stretches <see cref="ExtensionLVProtoId"/> between the power receiver and its provider.
    /// </summary>
    private void UpdateCable(Entity<ExtensionCableReceiverComponent> ent)
    {
        QueueDel(ent.Comp.ExtensionCordEntity);
        ent.Comp.ExtensionCordEntity = null;

        if (ent.Comp.Provider is not { } providerUid || TerminatingOrDeleted(providerUid))
            return;

        if (!_xformQuery.TryComp(ent, out var receiverXform) ||
            !_xformQuery.TryComp(providerUid, out var providerXform))
            return;

        if (receiverXform.GridUid is not { } gridUid || gridUid != providerXform.GridUid)
            return;

        var diff = receiverXform.LocalPosition - providerXform.LocalPosition;
        var angle = diff.ToAngle();
        var distance = diff.Length();
        if (distance <= 0.001f)
            return;

        var spawnPos = new EntityCoordinates(gridUid, providerXform.LocalPosition);
        var effect = SpawnAtPosition(ExtensionLVProtoId, spawnPos);

        var extraOffset = OffsetForDirection(angle);
        var adjustedDistance = MathF.Max(0.001f, distance + extraOffset);
        var centerDist = (distance - extraOffset) * 0.5f;

        if (_spriteQuery.TryComp(effect, out var sprite))
        {
            _sprite.SetScale((effect, sprite), new Vector2(adjustedDistance, 1f));
            _sprite.SetRotation((effect, sprite), angle);
            _sprite.SetOffset((effect, sprite), diff / distance * centerDist);
        }

        ent.Comp.ExtensionCordEntity = effect;
    }

    /// <summary>
    /// LV cables sit slightly off-center on a tile; offsets the start so the extension stretches from the LV cable.
    /// </summary>
    private static float OffsetForDirection(Angle angle)
    {
        return angle.GetCardinalDir() switch
        {
            Direction.South or Direction.West => LvCableOffset,
            Direction.North or Direction.East => -LvCableOffset,
            _ => 0f,
        };
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<ExtensionCableReceiverComponent> ent, ref ComponentShutdown args)
    {
        QueueDel(ent.Comp.ExtensionCordEntity);
    }
}
