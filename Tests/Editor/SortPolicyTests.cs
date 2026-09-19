using System;
using NUnit.Framework;

namespace NewestFirst.Tests
{
    public class SortPolicyTests
    {
        private const string Url = "https://example.invalid/-/api/purchases?offset=100&limit=50&query=a%26b&orderBy=purchased_date&order=desc&tagging=hello%20world";

        [Test]
        public void RewritesDirectionWithoutChangingPaginationSearchOrFilters()
        {
            Assert.That(SortPolicy.Rewrite(Url, true), Is.EqualTo(Url.Replace("order=desc", "order=asc")));
        }

        [TestCase("name")]
        [TestCase("update_date")]
        public void OtherSortModesAreUnchanged(string field)
        {
            var url = Url.Replace("purchased_date", field);
            Assert.That(SortPolicy.Rewrite(url, true), Is.EqualTo(url));
        }

        [Test]
        public void OtherEndpointsAndDisabledModeAreUnchanged()
        {
            Assert.That(SortPolicy.Rewrite(Url, false), Is.EqualTo(Url));
            var url = Url.Replace("/purchases?", "/product?");
            Assert.That(SortPolicy.Rewrite(url, true), Is.EqualTo(url));
            Assert.That(SortPolicy.Rewrite("not a URI", true), Is.EqualTo("not a URI"));
            Assert.That(SortPolicy.Rewrite(null, true), Is.Null);
        }

        [Test]
        public void AmbiguousOrAlreadyAscendingRequestsAreUnchanged()
        {
            var ambiguous = Url + "&order=desc";
            Assert.That(SortPolicy.Rewrite(ambiguous, true), Is.EqualTo(ambiguous));
            var asc = Url.Replace("order=desc", "order=asc");
            Assert.That(SortPolicy.Rewrite(asc, true), Is.EqualTo(asc));
        }

        [Test]
        public void DetectsReversedAndCorrectServiceResponses()
        {
            var oldest = DateTimeOffset.Parse("2011-04-25T00:00:00Z");
            var newest = DateTimeOffset.Parse("2026-09-19T00:00:00Z");
            Assert.That(SortPolicy.Detect(new[] { newest, oldest }, new[] { oldest, newest }), Is.True);
            Assert.That(SortPolicy.Detect(new[] { oldest, newest }, new[] { newest, oldest }), Is.False);
        }

        [Test]
        public void EqualMissingAndUnorderedDatesAreInconclusive()
        {
            var date = DateTimeOffset.UtcNow;
            Assert.That(SortPolicy.Detect(new[] { date, date }, new[] { date, date }), Is.Null);
            Assert.That(SortPolicy.Detect(Array.Empty<DateTimeOffset>(), new[] { date }), Is.Null);
            Assert.That(SortPolicy.Direction(new[] { date, date.AddDays(1), date.AddDays(-1) }), Is.Zero);
        }
    }
}
