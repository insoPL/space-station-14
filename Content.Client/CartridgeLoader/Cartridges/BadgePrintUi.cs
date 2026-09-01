using Content.Client.UserInterface.Fragments;
using Content.Shared.Access.Systems;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client.CartridgeLoader.Cartridges;

public sealed partial class BadgePrintUi : UIFragment
{
    private BadgePrintUiFragment? _fragment;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IEntityManager _entityManager = default!;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        IoCManager.InjectDependencies(this);

        _fragment = new BadgePrintUiFragment();

        _fragment.OnPrintPressed += (timer, department) =>
        {
            SendPrintMessage(department, timer, userInterface);
        };

        UpdateClientAccess();
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
    }

    private void UpdateClientAccess()
    {
        if (_playerManager.LocalEntity is not { } player ||
            !player.IsValid() || _fragment == null)
            return;

        var accessReader = _entityManager.System<AccessReaderSystem>();

        var accessItems = accessReader.FindPotentialAccessItems(player);
        var accessTags = accessReader.FindAccessTags(player, accessItems);

        _fragment.UpdateAccess(accessTags);
    }

    private void SendPrintMessage(SelectedDepartment department, SelectedBadgeTimer timer, BoundUserInterface userInterface)
    {
        var printMessage = new BadgePrintUiMessageEvent(timer, department);
        var message = new CartridgeUiMessage(printMessage);

        userInterface.SendPredictedMessage(message);
    }
}
