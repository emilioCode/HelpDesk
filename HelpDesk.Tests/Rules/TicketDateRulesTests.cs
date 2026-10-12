using HelpDesk.Core.Rules;
using NUnit.Framework;
using System;

namespace HelpDesk.Tests.Rules
{
    public class TicketDateRulesTests
    {
        private static readonly DateTime Created = new DateTime(2026, 10, 11);

        [Test]
        public void Validate_WithoutDates_IsValid()
        {
            Assert.IsNull(TicketDateRules.Validate(Created, null, null, null, null));
        }

        [Test]
        public void Validate_StartOnCreationDate_IsValid()
        {
            Assert.IsNull(TicketDateRules.Validate(Created, Created, null, null, null));
        }

        [Test]
        public void Validate_StartAfterCreation_IsValid()
        {
            Assert.IsNull(TicketDateRules.Validate(Created, Created.AddDays(3), new TimeSpan(9, 0, 0), null, null));
        }

        [Test]
        public void Validate_StartBeforeCreation_IsRejected()
        {
            var error = TicketDateRules.Validate(Created, Created.AddDays(-1), null, null, null);
            Assert.AreEqual(TicketDateRules.StartBeforeCreation, error);
        }

        [Test]
        public void Validate_StartWithTimeOnCreationDay_IgnoresTimeOfDay()
        {
            Assert.IsNull(TicketDateRules.Validate(Created, Created.AddHours(8), null, null, null));
        }

        [Test]
        public void Validate_EndBeforeStartDate_IsRejected()
        {
            var error = TicketDateRules.Validate(Created, Created.AddDays(2), null, Created.AddDays(1), null);
            Assert.AreEqual(TicketDateRules.EndBeforeStart, error);
        }

        [Test]
        public void Validate_SameDayEndTimeBeforeStartTime_IsRejected()
        {
            var day = Created.AddDays(1);
            var error = TicketDateRules.Validate(Created, day, new TimeSpan(15, 0, 0), day, new TimeSpan(9, 0, 0));
            Assert.AreEqual(TicketDateRules.EndBeforeStart, error);
        }

        [Test]
        public void Validate_SameDayEndTimeAfterStartTime_IsValid()
        {
            var day = Created.AddDays(1);
            Assert.IsNull(TicketDateRules.Validate(Created, day, new TimeSpan(9, 0, 0), day, new TimeSpan(15, 0, 0)));
        }

        [Test]
        public void Validate_EndDateWithoutStartDate_IsValid()
        {
            Assert.IsNull(TicketDateRules.Validate(Created, null, null, Created.AddDays(1), null));
        }
    }
}
