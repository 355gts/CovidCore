using System;
using System.Runtime.Serialization;

namespace Covid.Web.Model.Users
{
    [DataContract]
    public class CreateUser
    {
        [DataMember(IsRequired = true)]
        public string Firstname { get; set; }

        [DataMember(IsRequired = true)]
        public string Surname { get; set; }

        [DataMember(IsRequired = true)]
        public DateTime DateOfBirth { get; set; }
    }
}
