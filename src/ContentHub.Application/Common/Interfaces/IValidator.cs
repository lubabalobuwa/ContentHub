using ContentHub.Application.Common;

namespace ContentHub.Application.Common.Interfaces
{
    public interface IValidator<in T>
    {
        Result Validate(T command);
    }
}
