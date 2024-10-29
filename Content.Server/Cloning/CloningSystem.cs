using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Cloning.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Server.EUI;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Humanoid;
using Content.Server.Implants.Components;
using Content.Server.Materials;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.CCVar;
using Content.Shared.Chemistry.Components;
using Content.Shared.Cloning;
using Content.Shared.Damage;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Tag;
using Content.Shared.Mobs;

namespace Content.Server.Cloning
{
    public sealed class CloningSystem : EntitySystem
    {
        [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
        [Dependency] private readonly IPlayerManager _playerManager = null!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly EuiManager _euiManager = null!;
        [Dependency] private readonly CloningConsoleSystem _cloningConsoleSystem = default!;
        [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
        [Dependency] private readonly ContainerSystem _containerSystem = default!;
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly PuddleSystem _puddleSystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IConfigurationManager _configManager = default!;
        [Dependency] private readonly MaterialStorageSystem _material = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;
        [Dependency] private readonly MetaDataSystem _metaSystem = default!;
        [Dependency] private readonly TagSystem _tag = default!;

        [ValidatePrototypeId<TagPrototype>]
        public const string MindBackupTag = "MindBackup";

        public readonly Dictionary<MindBackupComponent, EntityUid> ClonePodWaitingForMind = new();
        // MindBackupComponent - linked implant
        // EntityUid - clone

        public const float EasyModeCloningCost = 0.7f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CloningPodComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<RoundRestartCleanupEvent>(Reset);
            SubscribeLocalEvent<BeingClonedComponent, MindAddedMessage>(HandleMindAdded);
            SubscribeLocalEvent<CloningPodComponent, PortDisconnectedEvent>(OnPortDisconnected);
            SubscribeLocalEvent<CloningPodComponent, AnchorStateChangedEvent>(OnAnchor);
            SubscribeLocalEvent<CloningPodComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<CloningPodComponent, GotEmaggedEvent>(OnEmagged);
            SubscribeLocalEvent<MindBackupComponent, MobStateChangedEvent>(OnDeathOfScanned);
        }

        private void OnDeathOfScanned(Entity<MindBackupComponent> ent, ref MobStateChangedEvent args)
        {
            if (args.NewMobState != MobState.Dead)
                return;

            var uidOfDeceased = args.Target;
            var mind = _mindSystem.GetMind(uidOfDeceased);

            if (!_playerManager.TryGetSessionByEntity(uidOfDeceased, out var netUserId) ||
                mind == null)
                return;

            _euiManager.OpenEui(new AcceptCloningEui(ent.Owner, this), netUserId);
        }

        private void OnComponentInit(EntityUid uid, CloningPodComponent clonePod, ComponentInit args)
        {
            clonePod.BodyContainer = _containerSystem.EnsureContainer<ContainerSlot>(uid, "clonepod-bodyContainer");
            _signalSystem.EnsureSinkPorts(uid, CloningPodComponent.PodPort);
        }

        internal void TransferMindToClone(EntityUid targetEntity)
        {
            if (!TryComp<MindBackupComponent>(targetEntity, out var mindBackupComponent) ||
                mindBackupComponent == null ||
                !ClonePodWaitingForMind.TryGetValue(mindBackupComponent, out var clonePodEntity) ||
                !EntityManager.EntityExists(clonePodEntity))
                return;

            if (!TryComp<CloningPodComponent>(clonePodEntity, out var cloningPodComponent) ||
                cloningPodComponent == null)
                return;

            var cloneEntity = cloningPodComponent.BodyContainer.ContainedEntity;
            var mind = _mindSystem.GetMind(targetEntity);

            if (mind == null || cloneEntity==null)
                return;

            _mindSystem.UnVisit(mind.Value);
            _mindSystem.TransferTo(mind.Value, cloneEntity, ghostCheckOverride: true);
            ClonePodWaitingForMind.Remove(mindBackupComponent);
            
            if (cloningPodComponent == null) return;
            EjectClone(clonePodEntity, cloningPodComponent);
        }

        private void HandleMindAdded(EntityUid uid, BeingClonedComponent clonedComponent, MindAddedMessage message)///when its fired?
        {
            if (clonedComponent.Parent == EntityUid.Invalid ||
                !EntityManager.EntityExists(clonedComponent.Parent) ||
                !TryComp<CloningPodComponent>(clonedComponent.Parent, out var cloningPodComponent) ||
                uid != cloningPodComponent.BodyContainer.ContainedEntity)
            {
                EntityManager.RemoveComponent<BeingClonedComponent>(uid);
                return;
            }
            UpdateStatus(clonedComponent.Parent, CloningPodStatus.Cloning, cloningPodComponent);
        }

        private void OnPortDisconnected(EntityUid uid, CloningPodComponent pod, PortDisconnectedEvent args)
        {
            pod.ConnectedConsole = null;
        }

        private void OnAnchor(EntityUid uid, CloningPodComponent component, ref AnchorStateChangedEvent args)
        {
            if (component.ConnectedConsole == null || !TryComp<CloningConsoleComponent>(component.ConnectedConsole, out var console))
                return;

            if (args.Anchored)
            {
                _cloningConsoleSystem.RecheckConnections(component.ConnectedConsole.Value, uid, console.GeneticScanner, console);
                return;
            }
            _cloningConsoleSystem.UpdateUserInterface(component.ConnectedConsole.Value, console);
        }

        private void OnExamined(EntityUid uid, CloningPodComponent component, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange || !_powerReceiverSystem.IsPowered(uid))
                return;

            args.PushMarkup(Loc.GetString("cloning-pod-biomass", ("number", _material.GetMaterialAmount(uid, component.RequiredMaterial))));
        }

        public bool TryLinkingCloneAndMindBackupImplant(EntityUid cloningPodUid, EntityUid bodyToClone, Entity<MindComponent> targetMindComponent, CloningPodComponent? clonePod, float failChanceModifier = 1)
        {
            if (!Resolve(cloningPodUid, ref clonePod))
                return false;

            if (HasComp<CloneIsGrowingComponent>(cloningPodUid) || clonePod.ConnectedConsole == null)
                return false;

            var mind = targetMindComponent.Comp;
            if (mind.OwnedEntity == null ||
                _mobStateSystem.IsDead(mind.OwnedEntity.Value) ||
                !TryComp<HumanoidAppearanceComponent>(bodyToClone, out var humanoid) ||
                !_prototype.TryIndex(humanoid.Species, out var speciesPrototype) ||
                !TryComp<PhysicsComponent>(bodyToClone, out var physics)
                )
            {
                _chatSystem.TrySendInGameICMessage(clonePod.ConnectedConsole.Value, Loc.GetString("cloned-is-not-incompatible"), InGameICChatType.Speak, false);
                return false; // Cloned is dead
            }

            if (!TryComp<MindBackupComponent>(bodyToClone, out var mindBackupComponent))
            {
                _chatSystem.TrySendInGameICMessage(clonePod.ConnectedConsole.Value, Loc.GetString("cloning-pod-biomass"), InGameICChatType.Speak, false);
                return false;
            }

            // if somone already have linked clonepod TODO refactor this shit
            if (ClonePodWaitingForMind.TryGetValue(mindBackupComponent, out var clonePodEntity))
            {
                if (!EntityManager.EntityExists(clonePodEntity))
                    ClonePodWaitingForMind.Remove(mindBackupComponent);
                else
                {
                    if (!TryComp<CloningPodComponent>(clonePodEntity, out var cloningPodComponent) ||
                        cloningPodComponent == null)
                        return false;

                    var cloneEntity = cloningPodComponent.BodyContainer.ContainedEntity;

                    if (cloneEntity != null &&
                        EntityManager.EntityExists(cloneEntity) &&
                        !_mobStateSystem.IsDead(cloneEntity.Value) &&
                        TryComp<MindContainerComponent>(cloneEntity.Value, out var cloneMindComp) &&
                        (cloneMindComp.Mind == null || cloneMindComp.Mind == targetMindComponent))
                    {
                        _chatSystem.TrySendInGameICMessage(clonePod.ConnectedConsole.Value, "Mind already has clone", InGameICChatType.Speak, false);//Loc.GetString("cloning-console-chat-error")
                        return false;
                    }
                    ClonePodWaitingForMind.Remove(mindBackupComponent);
                }
            }

            var cloningCost = (int) Math.Round(physics.FixturesMass);

            if (_configManager.GetCVar(CCVars.BiomassEasyMode))
                cloningCost = (int) Math.Round(cloningCost * EasyModeCloningCost);

            // biomass checks
            var biomassAmount = _material.GetMaterialAmount(cloningPodUid, clonePod.RequiredMaterial);

            if (biomassAmount < cloningCost)
            {
                if (clonePod.ConnectedConsole != null)
                    _chatSystem.TrySendInGameICMessage(clonePod.ConnectedConsole.Value, Loc.GetString("cloning-console-chat-error", ("units", cloningCost)), InGameICChatType.Speak, false);
                return false;
            }

            _material.TryChangeMaterialAmount(cloningPodUid, clonePod.RequiredMaterial, -cloningCost);
            clonePod.UsedBiomass = cloningCost;
            // end of biomass checks

            // genetic damage checks TODO rewrite
            if (TryComp<DamageableComponent>(bodyToClone, out var damageable) &&
                damageable.Damage.DamageDict.TryGetValue("Cellular", out var cellularDmg))
            {
                var chance = Math.Clamp((float)(cellularDmg / 100), 0, 1);
                chance *= failChanceModifier;

                if (cellularDmg > 0 && clonePod.ConnectedConsole != null)
                    _chatSystem.TrySendInGameICMessage(clonePod.ConnectedConsole.Value, Loc.GetString("cloning-console-cellular-warning", ("percent", Math.Round(100 - chance * 100))), InGameICChatType.Speak, false);

                if (_robustRandom.Prob(chance))
                {
                    UpdateStatus(cloningPodUid, CloningPodStatus.Gore, clonePod);
                    clonePod.FailedClone = true;
                    AddComp<CloneIsGrowingComponent>(cloningPodUid);
                    return true;
                }
            }
            // end of genetic damage checks

            var freshClone = Spawn(speciesPrototype.Prototype, _transformSystem.GetMapCoordinates(cloningPodUid));
            _humanoidSystem.CloneAppearance(bodyToClone, freshClone);

            _containerSystem.Insert(freshClone, clonePod.BodyContainer);
            ClonePodWaitingForMind.Add(mindBackupComponent, cloningPodUid);
            UpdateStatus(cloningPodUid, CloningPodStatus.NoMind, clonePod);

            AddComp<CloneIsGrowingComponent>(cloningPodUid);

            return true;
        }

        public void UpdateStatus(EntityUid podUid, CloningPodStatus status, CloningPodComponent cloningPod)
        {
            cloningPod.Status = status;
            _appearance.SetData(podUid, CloningPodVisuals.Status, cloningPod.Status);
        }

        public override void Update(float frameTime)
        {
            var query = EntityQueryEnumerator<CloneIsGrowingComponent, CloningPodComponent>();

            while (query.MoveNext(out var uid, out var _, out var cloning))
            {

                if (!_powerReceiverSystem.IsPowered(uid))
                    continue;

                if (cloning.BodyContainer.ContainedEntity == null && !cloning.FailedClone)
                    continue;

                cloning.BodyScanProgress += frameTime;
                if (cloning.BodyScanProgress < cloning.BodyScanTime)//TODO make it fun
                    continue;

                if (cloning.FailedClone)
                    EndFailedCloning(uid, cloning);
            }
        }

        /// <summary>
        /// On emag, spawns a failed clone when cloning process fails which attacks nearby crew.
        /// </summary>
        private void OnEmagged(EntityUid uid, CloningPodComponent clonePod, ref GotEmaggedEvent args)
        {
            if (!this.IsPowered(uid, EntityManager))
                return;

            _audio.PlayPvs(clonePod.SparkSound, uid);
            _popupSystem.PopupEntity(Loc.GetString("cloning-pod-component-upgrade-emag-requirement"), uid);
            args.Handled = true;
        }

        public void EjectClone(EntityUid uid, CloningPodComponent? clonePod)
        {
            if (!Resolve(uid, ref clonePod))
                return;

            if (clonePod.BodyContainer.ContainedEntity is not { Valid: true } entity || clonePod.BodyScanProgress < clonePod.BodyScanTime)
                return;

            _containerSystem.Remove(entity, clonePod.BodyContainer);
            clonePod.BodyScanProgress = 0f;
            clonePod.UsedBiomass = 0;
            UpdateStatus(uid, CloningPodStatus.Idle, clonePod);
            RemCompDeferred<CloneIsGrowingComponent>(uid);
        }

        private void EndFailedCloning(EntityUid uid, CloningPodComponent clonePod)
        {
            clonePod.FailedClone = false;
            clonePod.BodyScanProgress = 0f;
            UpdateStatus(uid, CloningPodStatus.Idle, clonePod);
            var transform = Transform(uid);
            var indices = _transformSystem.GetGridTilePositionOrDefault((uid, transform));
            var tileMix = _atmosphereSystem.GetTileMixture(transform.GridUid, null, indices, true);

            if (HasComp<EmaggedComponent>(uid))
            {
                _audio.PlayPvs(clonePod.ScreamSound, uid);
                Spawn(clonePod.MobSpawnId, transform.Coordinates);
            }

            Solution bloodSolution = new();

            var i = 0;
            while (i < 1)
            {
                tileMix?.AdjustMoles(Gas.Ammonia, 6f);
                bloodSolution.AddReagent("Blood", 50);
                if (_robustRandom.Prob(0.2f))
                    i++;
            }
            _puddleSystem.TrySpillAt(uid, bloodSolution, out _);

            if (!HasComp<EmaggedComponent>(uid))
            {
                _material.SpawnMultipleFromMaterial(_robustRandom.Next(1, (int) (clonePod.UsedBiomass / 2.5)), clonePod.RequiredMaterial, Transform(uid).Coordinates);
            }

            clonePod.UsedBiomass = 0;
            RemCompDeferred<CloneIsGrowingComponent>(uid);
        }

        public void Reset(RoundRestartCleanupEvent ev)
        {
            ClonePodWaitingForMind.Clear();
        }
    }
}
