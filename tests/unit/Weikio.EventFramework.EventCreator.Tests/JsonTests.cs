using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Weikio.EventFramework.EventCreator.Tests
{
    public class JsonTests
    {
        [Fact]
        public void CanCreateJson()
        {
            var obj = new CustomerCreated(Guid.NewGuid(), "Test", "User");

            var result = CloudEventCreator.CreateJson(obj);

            JToken.Parse(result);
        }
        
        [Fact]
        public void CanCreateJsonFromObjects()
        {
            var objs = new List<CustomerCreated>();

            for (var i = 0; i < 10; i++)
            {
                var obj = new CustomerCreated(Guid.NewGuid(), "Test", "User " + i);
                objs.Add(obj);
            }

            var result = CloudEventCreator.CreateJson(objs);

            JArray.Parse(result);
        }
    }
}
