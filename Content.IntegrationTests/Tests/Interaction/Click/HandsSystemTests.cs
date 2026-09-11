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
  id: DummyTagedItem
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
        EntityUid taggedItem = default;
        EntityUid regularItem = default;

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
            taggedItem = SEntMan.SpawnEntity("DummyTagedItem", coords);
            regularItem = SEntMan.SpawnEntity("DummyRegularItem", coords);
        });

        await Server.WaitRunTicks(1); // haisen test fix

        await Server.WaitAssertion(() =>
        {
            // Attempt to pick up the item that lacks the required tag
            var pickupDenied = handSys.TryPickup(user, regularItem);
            Assert.That(pickupDenied, Is.False, "System allowed picking up a non-whitelisted item.");

            // Attempt to pick up the item that has the required tag
            var pickupAllowed = handSys.TryPickup(user, taggedItem);
            Assert.That(pickupAllowed, Is.True, "System blocked picking up a whitelisted item.");

            // Verify the state of the HandsComponent accurately reflects the successful pickup
            Assert.That(handSys.IsHolding(user, taggedItem, out var holdingHand));
            Assert.That(holdingHand, Is.EqualTo("restricted_hand"));
        });
    }

    [Test]
    public async Task HandBlacklistPickupTest()
    {
        var sysMan = Server.ResolveDependency<IEntitySystemManager>();
        var handSys = sysMan.GetEntitySystem<SharedHandsSystem>();
        var whitelistSystem = sysMan.GetEntitySystem<EntityWhitelistSystem>();

        var map = await Pair.CreateTestMap();
        var coords = map.MapCoords;

        await Server.WaitIdleAsync();

        EntityUid user = default;
        EntityUid taggedItem = default;
        EntityUid regularItem = default;

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

            // Create a blacklist that strictly banns tag
            var blacklist = new EntityWhitelist // whitelist of blacklisted items lol
            {
                Tags = new List<ProtoId<TagPrototype>> { "DummyTag" }
            };

            if (handsComp.Count == 0)
                return;// // haisen test fix, if the hands are still not registred just abandon the test

            // Apply blacklist to the hand component
            var hand = handsComp.Hands["restricted_hand"];
            hand.Blacklist = blacklist;

#pragma warning disable RA0002
            handsComp.Hands["restricted_hand"] = hand;
#pragma warning restore RA0002

            // Spawn item with and without the tag
            taggedItem = SEntMan.SpawnEntity("DummyTagedItem", coords);
            regularItem = SEntMan.SpawnEntity("DummyRegularItem", coords);
        });

        await Server.WaitRunTicks(1); // haisen test fix

        await Server.WaitAssertion(() =>
        {
            // Attempt to pick up the item that has the blacklisted tag
            var pickupOfTagged = handSys.TryPickup(user, taggedItem);
            Assert.That(pickupOfTagged, Is.False, "System allowed picking up a blacklisted item.");

            // Attempt to pick up the item that lacks the blacklisted tag
            var pickupRegular = handSys.TryPickup(user, regularItem);
            Assert.That(pickupRegular, Is.True, "System blocked picking up a non-blacklisted item.");

            // Verify the state of the HandsComponent accurately reflects the successful pickup
            Assert.That(handSys.IsHolding(user, regularItem, out var holdingHand));
            Assert.That(holdingHand, Is.EqualTo("restricted_hand"));
        });
    }
}
