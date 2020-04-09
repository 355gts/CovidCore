using Covid.Repository.Users;

namespace Covid.Repository.Facades
{
    public interface ICovidRepositoryFacade
    {
        IUserRepository Users { get; }
    }
}
