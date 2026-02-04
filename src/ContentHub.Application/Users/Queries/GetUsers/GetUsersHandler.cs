using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Queries.GetUsers
{
    public class GetUsersHandler
    {
        private readonly IUserRepository _userRepository;

        public GetUsersHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<PagedResult<UserSummaryDto>> HandleAsync(GetUsersQuery query)
        {
            return _userRepository.GetUsersAsync(query.Page, query.PageSize, query.Search);
        }
    }
}
