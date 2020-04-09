using System;
using System.Runtime.Serialization;

namespace Covid.Message.Model.Users
{
    [DataContract]
    [Serializable]
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
