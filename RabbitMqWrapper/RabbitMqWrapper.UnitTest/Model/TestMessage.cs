using System;
using System.Runtime.Serialization;

namespace RabbitMQWrapper.UnitTest.Model
{
    [DataContract]
    [Serializable]
    public class TestMessage
    {
        public string Property { get; set; }
    }
}
