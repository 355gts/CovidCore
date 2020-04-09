using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Covid.Web.Model.Users
{
    [DataContract]
    public class User : CreateUser
    {
        [DataMember(IsRequired = true)]
        [Range(0, Int64.MaxValue)]
        public long Id { get; set; }
    }
}
