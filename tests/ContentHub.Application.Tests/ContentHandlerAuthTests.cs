using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Messaging;
using ContentHub.Application.Content.Commands.ArchiveContent;
using ContentHub.Application.Content.Commands.DeleteContent;
using ContentHub.Application.Content.Commands.PublishContent;
using ContentHub.Application.Content.Commands.RestoreContent;
using ContentHub.Application.Content.Commands.UpdateContent;
using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ContentHub.Application.Tests
{
    public class ContentHandlerAuthTests
    {
        [Fact]
        public async Task Update_Unauthenticated_ReturnsUnauthorized()
        {
            var content = CreateContent(Guid.NewGuid());
            var repo = new FakeContentRepository(content);
            var currentUser = new FakeCurrentUserService(null, null, false);
            var handler = new UpdateContentHandler(repo, currentUser, new FakeUnitOfWork());

            var result = await handler.HandleAsync(new UpdateContentCommand(content.Id, "Title", "Body", RowVersion()));

            Assert.False(result.IsSuccess);
            Assert.Equal("Unauthorized.", result.Error);
        }

        [Fact]
        public async Task Update_NotOwner_ReturnsForbidden()
        {
            var authorId = Guid.NewGuid();
            var content = CreateContent(authorId);
            var repo = new FakeContentRepository(content);
            var currentUser = new FakeCurrentUserService(Guid.NewGuid(), UserRole.Author, true);
            var handler = new UpdateContentHandler(repo, currentUser, new FakeUnitOfWork());

            var result = await handler.HandleAsync(new UpdateContentCommand(content.Id, "Title", "Body", RowVersion()));

            Assert.False(result.IsSuccess);
            Assert.Equal("Forbidden.", result.Error);
        }

        [Fact]
        public async Task Update_Admin_Allows()
        {
            var authorId = Guid.NewGuid();
            var content = CreateContent(authorId);
            var repo = new FakeContentRepository(content);
            var currentUser = new FakeCurrentUserService(Guid.NewGuid(), UserRole.Admin, true);
            var unitOfWork = new FakeUnitOfWork();
            var handler = new UpdateContentHandler(repo, currentUser, unitOfWork);

            var result = await handler.HandleAsync(new UpdateContentCommand(content.Id, "New", "Body", RowVersion()));

            Assert.True(result.IsSuccess);
            Assert.Equal(1, unitOfWork.CommitCount);
        }

        [Fact]
        public async Task Archive_NotOwner_ReturnsForbidden()
        {
            var authorId = Guid.NewGuid();
            var content = CreateContent(authorId);
            var repo = new FakeContentRepository(content);
            var currentUser = new FakeCurrentUserService(Guid.NewGuid(), UserRole.Author, true);
            var handler = new ArchiveContentHandler(repo, currentUser, new FakeUnitOfWork());

            var result = await handler.HandleAsync(new ArchiveContentCommand(content.Id, RowVersion()));

            Assert.False(result.IsSuccess);
            Assert.Equal("Forbidden.", result.Error);
        }

        [Fact]
        public async Task Delete_Admin_Allows()
        {
            var authorId = Guid.NewGuid();
            var content = CreateContent(authorId);
            var repo = new FakeContentRepository(content);
            var currentUser = new FakeCurrentUserService(Guid.NewGuid(), UserRole.Admin, true);
            var unitOfWork = new FakeUnitOfWork();
            var handler = new DeleteContentHandler(repo, currentUser, unitOfWork);

            var result = await handler.HandleAsync(new DeleteContentCommand(content.Id, RowVersion()));

            Assert.True(result.IsSuccess);
            Assert.Equal(1, unitOfWork.CommitCount);
        }

        [Fact]
        public async Task Publish_NotOwner_ReturnsForbidden()
        {
            var authorId = Guid.NewGuid();
            var content = CreateContent(authorId);
            var repo = new FakeContentRepository(content);
            var currentUser = new FakeCurrentUserService(Guid.NewGuid(), UserRole.Author, true);
            var handler = new PublishContentHandler(repo, currentUser, new FakeUnitOfWork(), new FakePublisher());

            var result = await handler.HandleAsync(new PublishContentCommand(content.Id, RowVersion()));

            Assert.False(result.IsSuccess);
            Assert.Equal("Forbidden.", result.Error);
        }

        [Fact]
        public async Task Restore_NotOwner_ReturnsForbidden()
        {
            var authorId = Guid.NewGuid();
            var content = CreateContent(authorId);
            content.Archive(DateTime.UtcNow);
            var repo = new FakeContentRepository(content);
            var currentUser = new FakeCurrentUserService(Guid.NewGuid(), UserRole.Author, true);
            var handler = new RestoreContentHandler(repo, currentUser, new FakeUnitOfWork());

            var result = await handler.HandleAsync(new RestoreContentCommand(content.Id, RowVersion()));

            Assert.False(result.IsSuccess);
            Assert.Equal("Forbidden.", result.Error);
        }

        private static ContentItem CreateContent(Guid authorId)
        {
            return new ContentItem("Title", "Body", authorId, DateTime.UtcNow);
        }

        private static string RowVersion()
        {
            return Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        }

        private sealed class FakeContentRepository : IContentRepository
        {
            private readonly ContentItem? _content;

            public FakeContentRepository(ContentItem? content)
            {
                _content = content;
            }

            public Task AddAsync(ContentItem contentItem)
            {
                return Task.CompletedTask;
            }

            public Task<ContentItem?> GetByIdAsync(Guid id)
            {
                return Task.FromResult(_content?.Id == id ? _content : null);
            }

            public void Remove(ContentItem contentItem)
            {
            }

            public void SetOriginalRowVersion(ContentItem contentItem, byte[] rowVersion)
            {
            }
        }

        private sealed class FakeUnitOfWork : IUnitOfWork
        {
            public int CommitCount { get; private set; }

            public Task CommitAsync()
            {
                CommitCount++;
                return Task.CompletedTask;
            }
        }

        private sealed class FakeCurrentUserService : ICurrentUserService
        {
            public FakeCurrentUserService(Guid? userId, UserRole? role, bool isAuthenticated)
            {
                UserId = userId;
                Role = role;
                IsAuthenticated = isAuthenticated;
            }

            public Guid? UserId { get; }
            public UserRole? Role { get; }
            public bool IsAuthenticated { get; }
        }

        private sealed class FakePublisher : IRabbitMqPublisher
        {
            public Task PublishAsync(string queue, string message)
            {
                return Task.CompletedTask;
            }
        }
    }
}
