using Content.Server.EUI;
using Content.Shared.Cloning;
using Content.Shared.Eui;
using Content.Shared.Mind;

namespace Content.Server.Cloning
{
    public sealed class AcceptCloningEui : BaseEui
    {
        private readonly EntityUid _entityId;
        private readonly CloningSystem _cloningSystem;

        public AcceptCloningEui(EntityUid targetUid, CloningSystem cloningSys)
        {
            _entityId = targetUid;
            _cloningSystem = cloningSys;
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            if (msg is not AcceptCloningChoiceMessage choice ||
                choice.Button == AcceptCloningUiButton.Deny)
            {
                Close();
                return;
            }

            _cloningSystem.TransferMindToClone(_entityId);
            Close();
        }
    }
}
