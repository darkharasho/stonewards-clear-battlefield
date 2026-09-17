using System.Collections.Generic;
using ClearBattlefield;
using Xunit;

namespace ClearBattlefield.Tests
{
    public class DropTrackerTests
    {
        private static DropState OnGround(uint _) => DropState.OnGround;

        [Fact]
        public void TracksAndUntracks()
        {
            var tracker = new DropTracker();
            tracker.Track(5, 0);
            Assert.True(tracker.IsTracked(5));
            Assert.True(tracker.Untrack(5));
            Assert.False(tracker.IsTracked(5));
            Assert.False(tracker.Untrack(5));
        }

        [Fact]
        public void IgnoresUnspawnedNetId()
        {
            var tracker = new DropTracker();
            tracker.Track(0, 0);
            Assert.Equal(0, tracker.Count);
        }

        [Fact]
        public void OnlyTrackedIdsAreSelected()
        {
            var tracker = new DropTracker();
            tracker.Track(1, 0);
            tracker.Track(3, 0);
            Assert.Equal(new List<uint> { 1, 3 }, tracker.SelectClearable(10, 0, OnGround));
        }

        [Fact]
        public void GoneDropsAreForgottenAndNotSelected()
        {
            var tracker = new DropTracker();
            tracker.Track(1, 0);
            tracker.Track(2, 0);
            var result = tracker.SelectClearable(10, 0, id => id == 1 ? DropState.Gone : DropState.OnGround);
            Assert.Equal(new List<uint> { 2 }, result);
            Assert.False(tracker.IsTracked(1));
        }

        [Fact]
        public void BusyDropsStayTrackedButAreNotSelected()
        {
            var tracker = new DropTracker();
            tracker.Track(1, 0);
            Assert.Empty(tracker.SelectClearable(10, 0, _ => DropState.Busy));
            Assert.True(tracker.IsTracked(1));
        }

        [Fact]
        public void MinimumAgeHoldsBackYoungDrops()
        {
            var tracker = new DropTracker();
            tracker.Track(1, 0);
            tracker.Track(2, 8);
            Assert.Equal(new List<uint> { 1 }, tracker.SelectClearable(10, 5, OnGround));
            Assert.True(tracker.IsTracked(2));
            Assert.Equal(new List<uint> { 1, 2 }, tracker.SelectClearable(13, 5, OnGround));
        }

        [Fact]
        public void ZeroMinimumAgeClearsImmediately()
        {
            var tracker = new DropTracker();
            tracker.Track(1, 10);
            Assert.Equal(new List<uint> { 1 }, tracker.SelectClearable(10, 0, OnGround));
        }

        [Fact]
        public void MergeWithUntrackedStackUntracksBoth()
        {
            var tracker = new DropTracker();
            tracker.Track(1, 0);
            Assert.False(tracker.OnMerge(survivor: 2, absorbed: 1));
            Assert.False(tracker.IsTracked(1));
            Assert.False(tracker.IsTracked(2));
        }

        [Fact]
        public void TrackedStackAbsorbingUntrackedStackIsKept()
        {
            var tracker = new DropTracker();
            tracker.Track(1, 0);
            Assert.False(tracker.OnMerge(survivor: 1, absorbed: 2));
            Assert.Equal(0, tracker.Count);
        }

        [Fact]
        public void MergeOfTwoEnemyDropsStaysTracked()
        {
            var tracker = new DropTracker();
            tracker.Track(1, 0);
            tracker.Track(2, 0);
            Assert.True(tracker.OnMerge(survivor: 1, absorbed: 2));
            Assert.True(tracker.IsTracked(1));
            Assert.True(tracker.IsTracked(2));
        }

        [Fact]
        public void ResetForgetsEverything()
        {
            var tracker = new DropTracker();
            tracker.Track(1, 0);
            tracker.Track(2, 0);
            tracker.Reset();
            Assert.Equal(0, tracker.Count);
            Assert.Empty(tracker.SelectClearable(100, 0, OnGround));
        }

        [Theory]
        [InlineData(0, 0, "Nothing to clear")]
        [InlineData(1, 0, "Cleared 1 enemy drop")]
        [InlineData(37, 0, "Cleared 37 enemy drops")]
        [InlineData(0, 1, "Cleared 1 piece of scrap")]
        [InlineData(5, 3, "Cleared 5 enemy drops and 3 pieces of scrap")]
        public void ResultText(int drops, int scrap, string expected) =>
            Assert.Equal(expected, ClearText.Result(drops, scrap));

        [Fact]
        public void ConfirmationTextNamesCounts()
        {
            Assert.Contains("12 enemy drops and 2 pieces of scrap", ClearText.Confirmation(12, 2));
            Assert.Contains("nothing on the ground", ClearText.Confirmation(0, 0));
        }
    }
}
