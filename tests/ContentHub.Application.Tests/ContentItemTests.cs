using ContentHub.Domain.Content;
using System;
using Xunit;

namespace ContentHub.Application.Tests
{
    public class ContentItemTests
    {
        [Fact]
        public void Publish_FromDraft_Succeeds()
        {
            var item = new ContentItem("Title", "Body", Guid.NewGuid(), DateTime.UtcNow);

            item.Publish(DateTime.UtcNow);

            Assert.Equal(ContentStatus.Published, item.Status);
        }

        [Fact]
        public void Publish_FromPublished_Throws()
        {
            var item = new ContentItem("Title", "Body", Guid.NewGuid(), DateTime.UtcNow);
            item.Publish(DateTime.UtcNow);

            Assert.Throws<InvalidOperationException>(() => item.Publish(DateTime.UtcNow));
        }

        [Fact]
        public void Archive_SetsArchivedStatus()
        {
            var item = new ContentItem("Title", "Body", Guid.NewGuid(), DateTime.UtcNow);

            item.Archive(DateTime.UtcNow);

            Assert.Equal(ContentStatus.Archived, item.Status);
            Assert.NotNull(item.ArchivedAtUtc);
        }

        [Fact]
        public void Restore_FromArchived_SetsDraft()
        {
            var item = new ContentItem("Title", "Body", Guid.NewGuid(), DateTime.UtcNow);
            item.Archive(DateTime.UtcNow);

            item.Restore(DateTime.UtcNow);

            Assert.Equal(ContentStatus.Draft, item.Status);
            Assert.Null(item.ArchivedAtUtc);
        }

        [Fact]
        public void Update_OnArchived_Throws()
        {
            var item = new ContentItem("Title", "Body", Guid.NewGuid(), DateTime.UtcNow);
            item.Archive(DateTime.UtcNow);

            Assert.Throws<InvalidOperationException>(() => item.Update("New", "Body", DateTime.UtcNow));
        }
    }
}
