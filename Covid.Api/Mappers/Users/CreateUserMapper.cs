using Covid.Common.Mapper;
using Dom = Covid.Web.Model.Users;
using Repo = Covid.Repository.Model.Users;

namespace Covid.Api.Mappers.Users
{
    sealed class CreateUserMapper : ITypeMapper<Dom.CreateUser, Repo.User>
    {
        public Repo.User Map(Dom.CreateUser fromObject, Repo.User toObject = null)
        {
            var user = toObject ?? new Repo.User();

            user.DateOfBirth = fromObject.DateOfBirth;
            user.Firstname = fromObject.Firstname;
            user.Surname = fromObject.Surname;

            return user;
        }
    }
}
