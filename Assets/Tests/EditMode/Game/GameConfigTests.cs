using NUnit.Framework;
using SeaBattle.Common.Config;
using UnityEngine;

namespace SeaBattle.Tests.Game
{
    public class GameConfigTests
    {
        [Test]
        public void CreateRules_UsesAssignmentDefaults()
        {
            var config = ScriptableObject.CreateInstance<GameConfig>();

            var rules = config.CreateRules();

            Assert.That(rules.Width, Is.EqualTo(6));
            Assert.That(rules.Height, Is.EqualTo(6));
            Assert.That(rules.ShipLengths, Is.EqualTo(new[] { 3, 2, 2, 1 }));
            Assert.That(rules.TurnSeconds, Is.EqualTo(20));
            Assert.That(config.DeliveryDelayMilliseconds, Is.EqualTo(400));
            Assert.That(config.MessageLossPercent, Is.EqualTo(0));
            Assert.That(config.TurnSeconds, Is.EqualTo(20));
            Assert.That(config.MessageLogEnabled, Is.False);
            Object.DestroyImmediate(config);
        }
    }
}
