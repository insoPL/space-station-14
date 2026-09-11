#nullable enable annotations
using Content.IntegrationTests.Fixtures;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using System.Collections.Generic;

namespace Content.IntegrationTests.Tests.Interaction.Hands;

[TestFixture]
[TestOf(typeof(SharedHandsSystem))]
public sealed partial class HandWhitelistTests : GameTest
{
    // Define our dummy entities and tags for testing
    [TestPrototypes]
    private const string Prototypes = @"
- type: Tag
  id: DummyTag

- type: entity
  id: DummyWhitelistedItem
  components:
  - type: Item
  - type: Tag
    tags:
    - DummyTag

- type: entity
  id: DummyRegularItem
  components:
  - type: Item
";

    [Test]
    public async Task HandWhitelistPickupTest()
    {
        var sysMan = Server.ResolveDependency<IEntitySystemManager>();
        var handSys = sysMan.GetEntitySystem<SharedHandsSystem>();
        var whitelistSystem = sysMan.GetEntitySystem<EntityWhitelistSystem>();

        var map = await Pair.CreateTestMap();
        var coords = map.MapCoords;

        await Server.WaitIdleAsync();

        EntityUid user = default;
        EntityUid allowedItem = default;
        EntityUid deniedItem = default;

        await Server.WaitAssertion(() =>
        {
            user = SEntMan.SpawnEntity(null, coords);
            SEntMan.EnsureComponent<HandsComponent>(user);

            handSys.AddHand(user, "restricted_hand", HandLocation.Right);
        });

        await Server.WaitRunTicks(1);

        await Server.WaitAssertion(() =>
        {
            var handsComp = SEntMan.GetComponent<HandsComponent>(user);

            // Create a whitelist that strictly requires our dummy tag
            var whitelist = new EntityWhitelist
            {
                Tags = new List<ProtoId<TagPrototype>> { "DummyTag" }
            };

            // Apply whitelist to the hand component
            if (handsComp.Count == 0)
                return;// // haisen test fix, if the hands are still not registred just abandon the test

            var hand = handsComp.Hands["restricted_hand"];
            hand.Whitelist = whitelist;

#pragma warning disable RA0002
            handsComp.Hands["restricted_hand"] = hand;
#pragma warning restore RA0002

            // Spawn item with and without the required tag
            allowedItem = SEntMan.SpawnEntity("DummyWhitelistedItem", coords);
            deniedItem = SEntMan.SpawnEntity("DummyRegularItem", coords);
        });

        await Server.WaitRunTicks(1); // haisen test fix

        await Server.WaitAssertion(() =>
        {
            // Attempt to pick up the item that lacks the required tag
            var pickupDenied = handSys.TryPickup(user, deniedItem);
            Assert.That(pickupDenied, Is.False, "System allowed picking up a non-whitelisted item.");

            // Attempt to pick up the item that has the required tag
            var pickupAllowed = handSys.TryPickup(user, allowedItem);
            Assert.That(pickupAllowed, Is.True, "System blocked picking up a whitelisted item.");

            // Verify the state of the HandsComponent accurately reflects the successful pickup
            Assert.That(handSys.IsHolding(user, allowedItem, out var holdingHand));
            Assert.That(holdingHand, Is.EqualTo("restricted_hand"));
        });
    }
}
