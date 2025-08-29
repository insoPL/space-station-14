using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Client.BodyOfWaterEffect;

public sealed partial class BodyOfWaterEffectSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private EntityQuery<SpriteComponent> _spriteQuery;


    public override void Initialize()
    {
        base.Initialize();
        _spriteQuery = GetEntityQuery<SpriteComponent>();

        SubscribeLocalEvent<BodyOfWaterEffectComponent, ComponentStartup>(OnStartup);
    }
    private void OnStartup(Entity<BodyOfWaterEffectComponent> ent, ref ComponentStartup args)
    {
        SetShader(ent.Owner, true);
    }

    private void SetShader(Entity<SpriteComponent?> sprite, bool enabled)
    {
        if (!_spriteQuery.Resolve(sprite.Owner, ref sprite.Comp, false))
            return;

        var shader = _proto.Index<ShaderPrototype>("LakeEffect").InstanceUnique();
        shader.SetParameter("LAKE_COLOR", new Vector4(95, 0, 127, 1));

        if (sprite.Comp.PostShader is not null && sprite.Comp.PostShader != shader)
            return;

        if (enabled)
        {
            sprite.Comp.PostShader = shader;
        }
        else
        {
            sprite.Comp.PostShader = null;
        }
    }
}
