using Content.Server.Implants.Components;
using Content.Shared.Tag;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;

namespace Content.Server.Cloning.MindBackupImplant
{
    public sealed class MindBackupSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<SubdermalImplantComponent, MindBackupImplantImplantedEvent>(ImplantCheck);
        }
        public void ImplantCheck(EntityUid uid, SubdermalImplantComponent comp, ref MindBackupImplantImplantedEvent ev)
        {
            if (ev.Implanted != null)
            {
                EnsureComp<MindBackupComponent>(ev.Implanted.Value);
            }
        }

    }
}
