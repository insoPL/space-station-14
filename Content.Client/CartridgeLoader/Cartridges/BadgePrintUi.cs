using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

namespace Content.Client.CartridgeLoader.Cartridges;

public sealed partial class BadgePrintUi : UIFragment
{
    private BadgePrintUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new BadgePrintUiFragment();

        _fragment.OnPrintPressed += (timer, department) =>
        {
            SendPrintMessage(department, timer, userInterface);
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
    }

    private void SendPrintMessage(SelectedDepartment department, SelectedBadgeTimer timer, BoundUserInterface userInterface)
    {

        var printMessage = new BadgePrintUiMessageEvent(timer, department);
        var message = new CartridgeUiMessage(printMessage);

        userInterface.SendMessage(message);
    }
}
