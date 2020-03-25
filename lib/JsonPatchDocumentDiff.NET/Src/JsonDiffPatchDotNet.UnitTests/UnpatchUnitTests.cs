﻿using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestFixture]
	public class UnpatchUnitTests
	{
		[Test]
		public void Unpatch_ObjectApplyDelete_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"" : true }");
			var right = JObject.Parse(@"{ }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(unpatched, "Unpatched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property Undeleted");
			Assert.AreEqual(JTokenType.Boolean, unpatched.Property("p").Value.Type);
			Assert.IsTrue(unpatched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[Test]
		public void Unpatch_ObjectApplyAdd_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(0, unpatched.Properties().Count(), "Property Deleted");
		}

		[Test]
		public void Unpatch_ObjectApplyEdit_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"" : false }");
			var right = JObject.Parse(@"{ ""p"" : true }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Boolean, unpatched.Property("p").Value.Type);
			Assert.IsFalse(unpatched.Property("p").Value.ToObject<bool>(), "Patched Property");
		}

		[Test]
		public void Unpatch_ObjectApplyEditText_Success()
		{
			var jdp = new JsonDiffPatch();
			const string value =
				@"bla1h111111111111112312weldjidjoijfoiewjfoiefjefijfoejoijfiwoejfiewjfiwejfowjwifewjfejdewdwdewqwertyqwertifwiejifoiwfei";
			var left = JObject.Parse(@"{ ""p"" : """ + value + @""" }");
			var right = JObject.Parse(@"{ ""p"" : ""blah1"" }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.String, unpatched.Property("p").Value.Type, "String Type");
			Assert.AreEqual(value, unpatched.Property("p").Value.ToObject<string>(), "String value");
		}

		[Test]
		public void Unpatch_ObjectApplyEditTextEfficient_Success()
		{
			var options = new Options {MinEfficientTextDiffLength = 1, TextDiff = TextDiffMode.Efficient};
			var jdp = new JsonDiffPatch(options);
			var left = JObject.Parse(@"{ ""p"" : ""The quick brown fox jumps over the lazy dog."" }");
			var right = JObject.Parse(@"{ ""p"" : ""That quick brown fox jumped over a lazy dog."" }");
			var patch = jdp.Diff(left, right);

			var unpatched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(unpatched, "Patched object");
			Assert.AreEqual(1, unpatched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.String, unpatched.Property("p").Value.Type, "String Type");
			Assert.AreEqual("The quick brown fox jumps over the lazy dog.", unpatched.Property("p").Value.ToString(),
				"String value");
		}

		[Test]
		public void Unpatch_NestedObjectApplyEdit_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""i"": { ""p"" : false } }");
			var right = JObject.Parse(@"{ ""i"": { ""p"" : true } }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch) as JObject;

			Assert.IsNotNull(patched, "Patched object");
			Assert.AreEqual(1, patched.Properties().Count(), "Property");
			Assert.AreEqual(JTokenType.Object, patched.Property("i").Value.Type);
			Assert.AreEqual(1, ((JObject) patched.Property("i").Value).Properties().Count());
			Assert.AreEqual(JTokenType.Boolean, ((JObject) patched.Property("i").Value).Property("p").Value.Type);
			Assert.IsFalse(((JObject) patched.Property("i").Value).Property("p").Value.ToObject<bool>());
		}

		[Test]
		public void Unpatch_ArrayUnpatchAdd_Success()
		{
			var jdp = new JsonDiffPatch(new Options {ArrayDiff = ArrayDiffMode.Efficient});
			var left = JToken.Parse(@"[1,2,3]");
			var right = JToken.Parse(@"[1,2,3,4]");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayUnpatchRemove_Success()
		{
			var jdp = new JsonDiffPatch(new Options {ArrayDiff = ArrayDiffMode.Efficient});
			var left = JToken.Parse(@"[1,2,3]");
			var right = JToken.Parse(@"[1,2]");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayUnpatchModify_Success()
		{
			var jdp = new JsonDiffPatch(new Options {ArrayDiff = ArrayDiffMode.Efficient});
			var left = JToken.Parse(@"[1,3,{""p"":false}]");
			var right = JToken.Parse(@"[1,4,{""p"": [1] }]");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayUnpatchComplex_Success()
		{
			var jdp = new JsonDiffPatch(new Options {ArrayDiff = ArrayDiffMode.Efficient});
			var left = JToken.Parse(@"{""p"": [1,2,[1],false,""11111"",3,{""p"":false},10,10] }");
			var right = JToken.Parse(@"{""p"": [1,2,[1,3],false,""11112"",3,{""p"":true},10,10] }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayUnpatchMoving_Success()
		{
			var jdp = new JsonDiffPatch(new Options {ArrayDiff = ArrayDiffMode.Efficient});
			var left = JToken.Parse(@"[0,1,2,3,4,5,6,7,8,9,10]");
			var right = JToken.Parse(@"[10,0,1,7,2,4,5,6,88,9,3]");
			var patch =
				JToken.Parse(
					@"{ ""8"": [88], ""_t"": ""a"", ""_3"": ["""", 10, 3], ""_7"": ["""", 3, 3], ""_8"": [8, 0, 0], ""_10"": ["""", 0, 3] }");

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayPatchMovingNonConsecutive_Success()
		{
			var jdp = new JsonDiffPatch(new Options {ArrayDiff = ArrayDiffMode.Efficient});
			var left = JToken.Parse(@"[0,1,3,4,5]");
			var right = JToken.Parse(@"[0,4,3,1,5]");
			var patch = JToken.Parse(@"{""_t"": ""a"", ""_2"": ["""", 2, 3],""_3"": ["""", 1, 3]}");

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_ArrayPatchMoveDeletingNonConsecutive_Success()
		{
			var jdp = new JsonDiffPatch(new Options {ArrayDiff = ArrayDiffMode.Efficient});
			var left = JToken.Parse(@"[0,1,3,4,5]");
			var right = JToken.Parse(@"[0,5,3]");
			var patch = JToken.Parse(@"{""_t"": ""a"", ""_1"": [ 1, 0, 0], ""_3"": [4,0, 0],""_4"": [ """", 1, 3 ]}");

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_Bug16Exception_Success()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse("{\r\n  \"rootRegion\": {\r\n    \"rows\": [\r\n      \"auto\"\r\n    ],\r\n    \"members\": [\r\n      {\r\n        \"row\": 2\r\n      }\r\n    ]\r\n  }\r\n}");
			var right = JToken.Parse("{\r\n  \"rootRegion\": {\r\n    \"rows\": [\r\n      \"auto\",\r\n      \"auto\"\r\n    ],\r\n    \"members\": [\r\n      {\r\n        \"row\": 3\r\n      },\r\n      {\r\n        \"name\": \"label-header\"\r\n      }\r\n    ]\r\n  }\r\n}");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.AreEqual(left.ToString(), patched.ToString());
		}

		[Test]
		public void Unpatch_Bug16SilentFail_Success()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse("{\r\n    \"members\": [\r\n      {\r\n        \"name\": \"text-box\",\r\n        \"version\": \"1.0.0\",\r\n        \"required\": false,\r\n        \"isArray\": false,\r\n        \"row\": 2,\r\n        \"rowSpan\": 1,\r\n        \"column\": 0,\r\n        \"columnSpan\": 1,\r\n        \"readOnly\": false,\r\n        \"properties\": [\r\n          {\r\n            \"destPath\": \"ng-model\",\r\n            \"srcPath\": \"cmt\"\r\n          }\r\n        ],\r\n        \"parent\": \"Acknowledge Unit (111)\"\r\n      },\r\n      {\r\n        \"name\": \"component-label\",\r\n        \"version\": \"1.0.0\",\r\n        \"label\": \"COMMAND_DIALOG_COMMENT\",\r\n        \"required\": false,\r\n        \"isArray\": false,\r\n        \"row\": 1,\r\n        \"rowSpan\": 1,\r\n        \"column\": 0,\r\n        \"columnSpan\": 1,\r\n        \"readOnly\": false,\r\n        \"properties\": [],\r\n        \"parent\": \"Acknowledge Unit (111)\"\r\n      }\r\n    ]\r\n  \r\n}");
			var right = JToken.Parse("{\r\n    \"members\": [\r\n      {\r\n        \"name\": \"text-box\",\r\n        \"version\": \"1.0.0\",\r\n        \"required\": false,\r\n        \"isArray\": false,\r\n        \"row\": 3,\r\n        \"rowSpan\": 1,\r\n        \"column\": 0,\r\n        \"columnSpan\": 1,\r\n        \"readOnly\": false,\r\n        \"properties\": [\r\n          {\r\n            \"destPath\": \"ng-model\",\r\n            \"srcPath\": \"cmt\"\r\n          }\r\n        ],\r\n        \"parent\": \"Acknowledge Unit (111)\"\r\n      },\r\n      {\r\n        \"name\": \"component-label\",\r\n        \"version\": \"1.0.0\",\r\n        \"label\": \"COMMAND_DIALOG_COMMENT\",\r\n        \"required\": false,\r\n        \"isArray\": false,\r\n        \"row\": 2,\r\n        \"rowSpan\": 1,\r\n        \"column\": 0,\r\n        \"columnSpan\": 1,\r\n        \"readOnly\": false,\r\n        \"properties\": [],\r\n        \"parent\": \"Acknowledge Unit (111)\"\r\n      },\r\n      {\r\n        \"name\": \"label-header\",\r\n        \"version\": \"1.0.0\",\r\n        \"column\": 0,\r\n        \"row\": 0,\r\n        \"columnSpan\": 1,\r\n        \"rowSpan\": 1,\r\n        \"properties\": [],\r\n        \"addedArgs\": {},\r\n        \"parent\": \"Acknowledge Unit (111)\",\r\n        \"label\": \"test\"\r\n      }\r\n    ]\r\n  }");
			var patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.IsTrue(JToken.DeepEquals(left.ToString(), patched.ToString()));
		}

		[Test]
		public void Unpatch_Bug17EfficienText_Success()
		{
			var jdp = new JsonDiffPatch();
			var left = JToken.Parse("{ \"key\": \"aaaa aaaaaa aaaa aaaaaaa: aaaaaaaaa aaaa aaaaaaaa aaaa: 31-aaa-2017 aaaaa aaaaa aaaaaaa aaaa aaaaaaaa aaaa: 31-aaa-2017aaaaaa aaaaaa: aaaaaaaaa aaaaaa: aaaaaa aaaaa aaaaa aaaaaaa aaaaaa: aaaaaaaaaa aaaaaaaa aaaaaaa: aaaa(aaaa aaaaaa/aaaaaaaaaaaa)-aaaaaaa(aaaaaaaaaa/aaaaaaaaaa aaaa aaaaaa)aaaaa aaaaa aaaaaaa:aaaaaa aaaaaaa: aaaaaaaa aaaaaaaaaa aaaaaaa: aaaaaaaaaaaa aaaaaaaaaa: aaaaaaaa-aaaaaaaaaaaaaaaa aaaaaaaaaa aaaaaa: aaaaaaaaaaaaaaaa aaaaaaaaaa aaaaa: aaaaaaaa aaaaa-aaaaa aaaaaaaaaa, aaaaa aaaaaaa aa aaaaaaa aaaaaaaaaaaa aaaaa aaaaaaaaaaa (aaaaaa), aaaaa a 100 aaaaa aa aaa aaaaaaa.aaa aaaa: aaaaaaaaaaaaaaaa: aaaaaaaaaaaa aaaaaaaa: aaa aaaaa aaaaa:aaaaaaa aaaaaaa: 31-aaa-2014aaaaaa aaaaa: 16-aaa-2016aaaaaa aaaaa: 30-aaa-2017aaaaaa aaaaa: 27-aaa-2017aaaaaa aaaaa: 31-aaa-2017aa aaaaaaaaaa aaaaaaaaaa, (aaaaa aa aaaa aa a 52.67 aaaaa aa aaaa aaa aaaa aaaaaa aaaaaa), aaaaa 100 aa aaaa aaaaaaa.aaaaaaa aaaaaaa: 16-aaa-2016aaaa aaaaaaa aa 100 aaaaa aa aaaa aaa aaaa aaaaaa aaa aa aaaaaaaaaa aaa aaaaaaaaaa, a 88.02 aaaaaaaaaa aa aaaa aaa aaaa aaaaaa aaaaaa.aaaaaaa aaaaaaa: 30-aaa-2017aaaa aaaaaaa aa 100 aaaaa aa aaa-aaaa aaaaaa, aaaaa aa 100 aaaaa aa aaaa aaa aaaa aaaaaa aaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaa aaa, aaaaa aa aaaa aa 65.656 aaaaa aa aaaa aaa aaaa aaaaaa aaa aa aaaaaaaaaa aaa aaaaaaaaaa aaa 34.343 aa aaaa aaa aaaa aaaaaa aaaaaa. aaaa aaa aaaa aaaaaa aaa aa aaaaaaaaaa aaa aaaaaaaaaa aa 88.02 aaaaa aa aaaa aaa aaaa aaaaaa aaaaaa.aaaaaaa aaaaaaa: 27-aaa-2017aaaa aaaaaaa aa 100 aaaaa aa aaa-aaaa aaaaaa, aaaaa\" }");
			var right = JToken.Parse("{ \"key\": \"aaaa aaaaaa aaaa aaaaaaa: aaaaaaaaa aaaa aaaaaaaa aaaa: 17-aaa-2017 aaaaa aaaaa aaaaaaa aaaa aaaaaaaa aaaa: 17-aaa-2017aaaaaa aaaaaa: aaaaaaaaa aaaaaa: aaaaaa aaaaa aaaaa aaaaaaa aaaaaa: aaaaaaaaaa aaaaaaaa aaaaaaa: aaaa(aaaa aaaaaa/aaaaaaaaaaaa)-aaaaaaa(aaaaaaaaaa/aaaaaaaaaa aaaa aaaaaa)aaaaa aaaaa aaaaaaa:aaaaaa aaaaaaa: aaaaaaaa aaaaaaaaaa aaaaaaa: aaaaaaaaaaaa aaaaaaaaaa: aaaaaaaa-aaaaaaaaaaaaaaaa aaaaaaaaaa aaaaaa: aaaaaaaaaaaaaaaa aaaaaaaaaa aaaaa aaaa: -2016aaaaaaaaa aaaaaaaaaa aaaaa: aaaa aaaaaaa aa 100 aaaaa aa aaa-aaaa aaaaaa aaa, aaaaaaaa aaaaaaaaaa aa aaaaaa.aaaaaaaaa aaaaaaaaaa: aaaaaaaa-aaaaaaaaaaaaaaaa aaaaaaaaaa aaaaaa: aaaaaaaaaaaaa aaaaaaaaaa aa aaaa: -2016aaaaaaaaa aaaaaaaaaa aaaaa: aaaaaaaa aaaaa-aaaaa aaaaaaaaaa, aaaaa aaaaaaa aa aaaaaaa aaaaaaaaaaaa aaaaa aaaaaaaaaaa (aaaaaa), aaaaa a 100 aaaaa aa aaa aaaaaaa.aaa aaaa: aaaaaaaaaaaaaaaa: aaaaaaaaaaaa aaaaaaaa: aaa aaaaa aaaaa:aaaaaaa aaaaaaa: 31-aaa-2014aaaaaa aaaaa: 16-aaa-2016aaaaaa aaaaa: 30-aaa-2017aaaaaa aaaaa: 27-aaa-2017aaaaaa aaaaa: 31-aaa-2017aaaaaa aaaaa: 16-aaa-2017aa aaaaaaaaaa aaaaaaaaaa, (aaaaa aa aaaa aa a 52.67 aaaaa aa aaaa aaa aaaa aaaaaa aaaaaa), aaaaa 100 aa aaaa aaaaaaa.aaaaaaa aaaaaaa: 16-aaa-2016aaaa\" }");
			JToken patch = jdp.Diff(left, right);

			var patched = jdp.Unpatch(right, patch);

			Assert.IsTrue(JToken.DeepEquals(left.ToString(), patched.ToString()));
		}
	}
}
