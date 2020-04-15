using Covid.Common.Mapper;
using Dom = Covid.Web.Model.Users;
using Repo = Covid.Repository.Model.Users;

namespace Covid.Api.Mappers.Users
{
    sealed class UserMapper : ITypeMapper<Dom.User, Repo.User>, ITypeMapper<Repo.User, Dom.User>
    {
        private CreateUserMapper _createUserMapper = new CreateUserMapper();

        public Repo.User Map(Dom.User fromObject, Repo.User toObject = null)
        {
            var user = toObject ?? new Repo.User();

            _createUserMapper.Map(fromObject, user);

            user.Id = fromObject.Id;

            return user;
        }

        public Dom.User Map(Repo.User fromObject, Dom.User toObject = null)
        {
            var user = toObject ?? new Dom.User();

            user.DateOfBirth = fromObject.DateOfBirth;
            user.Firstname = fromObject.Firstname;
            user.Id = fromObject.Id;
            user.Surname = fromObject.Surname;

            return user;
        }
    }
}
