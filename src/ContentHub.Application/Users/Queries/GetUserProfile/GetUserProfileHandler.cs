using ContentHub.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Queries.GetUserProfile
{
    public class GetUserProfileHandler
    {
        private readonly IUserRepository _userRepository;

        public GetUserProfileHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfileResult?> HandleAsync(GetUserProfileQuery query)
        {
            var user = await _userRepository.GetByIdAsync(query.UserId);
            if (user is null)
                return null;

            return new UserProfileResult(user.Id, user.Email, user.DisplayName, user.Role.ToString());
        }
    }
}
