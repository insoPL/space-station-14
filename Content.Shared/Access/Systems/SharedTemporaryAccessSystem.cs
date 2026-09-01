using Content.Shared.Access.Components;
using Content.Shared.Examine;
using Robust.Shared.Timing;

namespace Content.Shared.Access.Systems;

public sealed partial class TemporaryAccessSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAccessSystem _access = default!;

    [SubscribeLocalEvent]
    private void OnExamine(Entity<TemporaryAccessComponent> ent, ref ExaminedEvent args)
    {

        if (ent.Comp.Expired)
            args.PushMarkup(Loc.GetString("temporary-access-expired-examine"));
        else if (_timing.CurTime < ent.Comp.ExpireTime)
        {
            var timeLeft = ent.Comp.ExpireTime - _timing.CurTime;
            args.PushMarkup(Loc.GetString("temporary-access-active-examine", ("time", timeLeft.ToString("mm\\:ss"))));
        }
        else
            args.PushMarkup(Loc.GetString("temporary-access-frozen"));
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<TemporaryAccessComponent> ent, ref MapInitEvent args)
    {
        // If ExpireTime is not Zero, it means it was loaded from a save file. Don't overwrite it!
        if (ent.Comp.ExpireTime != TimeSpan.Zero)
            return;

        ent.Comp.ExpireTime = _timing.CurTime + ent.Comp.AccessExpireTime;

        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TemporaryAccessComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Expired || _timing.CurTime < comp.ExpireTime)
                continue;

            ExpireAccess((uid, comp));
        }
    }

    /// <summary>
    /// Marks an <see cref="TemporaryAccessComponent"/> as expired, disabling the aceesses.
    /// </summary>
    private void ExpireAccess(Entity<TemporaryAccessComponent> ent)
    {
        if (ent.Comp.Expired)
            return;

        _access.SetAccessEnabled(ent, false);
        ent.Comp.Expired = true;
        Dirty(ent);
    }
}
