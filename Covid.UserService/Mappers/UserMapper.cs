using Covid.Common.Mapper;
using Dom = Covid.Web.Model.Users;
using Msg = Covid.Message.Model.Users;

namespace Covid.UserService.Mappers
{
    sealed class UserMapper : ITypeMapper<Msg.CreateUser, Dom.CreateUser>, ITypeMapper<Dom.User, Msg.User>
    {
        public Dom.CreateUser Map(Msg.CreateUser fromObject, Dom.CreateUser toObject = null)
        {
            var user = toObject ?? new Dom.CreateUser();

            user.DateOfBirth = fromObject.DateOfBirth;
            user.Firstname = fromObject.Firstname;
            user.Surname = fromObject.Surname;

            return user;
        }

        public Msg.User Map(Dom.User fromObject, Msg.User toObject = null)
        {
            var user = toObject ?? new Msg.User();

            user.DateOfBirth = fromObject.DateOfBirth;
            user.Firstname = fromObject.Firstname;
            user.Id = fromObject.Id;
            user.Surname = fromObject.Surname;

            return user;
        }
    }
}
