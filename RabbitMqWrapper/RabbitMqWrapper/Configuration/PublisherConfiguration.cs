﻿using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace RabbitMQWrapper.Configuration
{
    [DataContract]
    [Serializable]
    public class PublisherConfiguration : IPublisherConfiguration
    {
        [DataMember(IsRequired = true)]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("exchangeName")]
        public string ExchangeName { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("routingKey")]
        public string RoutingKey { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("publishesPersistentMessages", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool PublishesPersistentMessages { get; set; }

        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return false;

                if (string.IsNullOrEmpty(ExchangeName))
                    return false;

                return true;
            }
        }
    }
}
