using Content.Shared.Access.Components;
using Content.Shared.Examine;
using Content.Shared.NameModifier.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Shared.Access.Systems;

public sealed partial class TemporaryAccessSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly NameModifierSystem _nameModifier = default!;
    [Dependency] private readonly SharedAccessSystem _access = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemporaryAccessComponent, ExaminedEvent>(OnPriorityExamine);
        SubscribeLocalEvent<TemporaryAccessComponent, MapInitEvent>(OnPriorityMapInit);
    }

    private void OnPriorityExamine(Entity<TemporaryAccessComponent> ent, ref ExaminedEvent args)
    {
        var timeLeft = ent.Comp.ExpireTime - _timing.CurTime;

        if (ent.Comp.Expired)
            args.PushMarkup(Loc.GetString("temporary-access-expired-examine"));
        else if (_timing.CurTime < ent.Comp.ExpireTime)
            args.PushMarkup(Loc.GetString("temporary-access-active-examine", ("time", timeLeft.ToString("mm\\:ss"))));
        else
            args.PushMarkup(Loc.GetString("temporary-access-frozen"));
    }

    private void OnPriorityMapInit(Entity<TemporaryAccessComponent> ent, ref MapInitEvent args)
    {
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
