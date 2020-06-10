using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Covid.UserService.AppSettings
{
    [DataContract]
    [Serializable]
    public class UserServiceSettings : IUserServiceSettings
    {
        [DataMember(IsRequired = false)]
        [JsonProperty("myUserServiceProperty1")]
        public string MyUserServiceProperty1 { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("myUserServiceProperty2")]
        public string MyUserServiceProperty2 { get; set; }
    }
}
