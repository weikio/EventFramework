﻿// Copyright (c) Cloud Native Foundation. 
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

namespace CloudNative.CloudEvents
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;

    public class CloudEventJsonInputFormatter : TextInputFormatter
    {
        static JsonEventFormatter jsonFormatter = new JsonEventFormatter();
        public CloudEventJsonInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/cloudevents+json"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            var request = context.HttpContext.Request;

            try
            {
                var cloudEvent = await request.ReadCloudEventAsync(jsonFormatter);
                return await InputFormatterResult.SuccessAsync(cloudEvent);
            }
            catch (Exception)
            {
                return await InputFormatterResult.FailureAsync();
            }
        }

        protected override bool CanReadType(Type type)
        {
            if (type == typeof(CloudEvent))
            {
                return base.CanReadType(type);
            }

            return false;
        }
    }
}
