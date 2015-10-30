﻿// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.ModelBinding;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DataBooster.DbWebApi.WebForm
{
	public class WebFormUrlEncodedMediaTypeFormatter : JQueryMvcFormUrlEncodedFormatter
	{
		/// <param name="type">The type of object to read.</param>
		/// <param name="readStream">The <see cref="T:System.IO.Stream"/> from which to read.</param>
		/// <param name="content">The content being read.</param>
		/// <param name="formatterLogger">The <see cref="T:System.Net.Http.Formatting.IFormatterLogger"/> to log events to.</param>
		/// <returns>A <see cref="T:System.Threading.Tasks.Task"/> whose result will be the object instance that has been read.</returns>
#if WEB_API2
		public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			if (type.IsAssignableFrom(typeof(InputParameters)))
			{
				JObject jObj = await base.ReadFromStreamAsync(typeof(JObject), readStream, content, formatterLogger) as JObject;
				return new InputParameters(jObj);
			}
			else
				return base.ReadFromStreamAsync(type, readStream, content, formatterLogger);
		}
#else	// ASP.NET Web API 1
		public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			if (type.IsAssignableFrom(typeof(InputParameters)))
			{
				return base.ReadFromStreamAsync(typeof(JObject), readStream, content, formatterLogger).ContinueWith<object>(jTask =>
					{
						if (jTask.IsCanceled)
							return null;
						if (jTask.IsFaulted)
							throw jTask.Exception;

						JObject jObj = jTask.Result as JObject;
						return new InputParameters(jObj);
					});
			}
			else
				return base.ReadFromStreamAsync(type, readStream, content, formatterLogger);
		}
#endif

		public static void ReplaceJQueryMvcFormUrlEncodedFormatter(MediaTypeFormatterCollection formatters)
		{
			for (int i = 0; i < formatters.Count; i++)
			{
				if (formatters[i] is WebFormUrlEncodedMediaTypeFormatter)
					return;

				if (formatters[i].GetType() == typeof(JQueryMvcFormUrlEncodedFormatter))
				{
					formatters[i] = new WebFormUrlEncodedMediaTypeFormatter();
					return;
				}
			}

			formatters.Add(new WebFormUrlEncodedMediaTypeFormatter());
		}
	}
}