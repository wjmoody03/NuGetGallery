// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xunit;
using NuGetGallery.Auditing.Obfuscation;
using System.Collections.Generic;

namespace NuGetGallery.Auditing
{
    public class ObfuscatorJsonConverterTests
    {
        [Fact]
        public void ConstructorThrowsOnNull()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>( () => new ObfuscatorJsonConverter(null));
        }

        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int?), true)]
        [InlineData(typeof(string[]), true)]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(double), false)]

        public void CanConvertTest(Type type, bool expectedResult)
        {
            // Arrange
            var converter = new ObfuscatorJsonConverter(new Data());

            // Act and Assert
            Assert.Equal(expectedResult, converter.CanConvert(type));
        }

        [Fact]
        public void WriteHappyJson()
        {
            // Arrange
            var dataChild = new Data("name", "1.1.1.1", "authors", 1, new[] { "howdy" }, "abc", 2.5, null, null);
            var dataChild1 = new Data("name2", "2.2.2.2", "authorz", 3, new[] { "uno", "dos" }, "def", 3.3, null, null);
            var dataChild2 = new Data("name3", "2.2.2.2", "authorz", 3, new[] { "tres", "quatro" }, "def", 3.3, null, null);
            var data = new Data("name", "1.1.1.1", "authors", 1, new[] { "lol" }, "abc", 2.5, dataChild, new[] { dataChild1, dataChild2 });
            var obfuscatorConverter = new ObfuscatorJsonConverter(data);
            var stringBuilder = new StringBuilder();
            var jsonWriter = new JsonTextWriter(new StringWriter(stringBuilder));

            // Act
            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                MaxDepth = 10,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.None,

            };
            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(obfuscatorConverter);
            var resultString = JsonConvert.SerializeObject(data, settings);
            var result = JObject.Parse(resultString);

            // Assert
            Assert.Equal("ObfuscatedUserName", result["UserName"].ToString());
            Assert.Equal("1.1.1.0", result["IP"].ToString());
            Assert.Equal(string.Empty, result["Authors"].ToString());
            Assert.Equal("-1", result["UserKey"].ToString());
            Assert.Equal("abc", result["SupportedTypeRandom"].ToString());
            Assert.Equal(new string[] { "ObfuscatedUserName" }, result["UserNameList"].ToObject<string[]>());
            Assert.Equal("2.5", Convert.ToString(result["NotSupportedTypeRandom"], CultureInfo.InvariantCulture));

            Assert.Equal("ObfuscatedUserName", result["OtherData"]["UserName"].ToString());
            Assert.Equal("1.1.1.0", result["OtherData"]["IP"].ToString());
            Assert.Equal(string.Empty, result["OtherData"]["Authors"].ToString());
            Assert.Equal("-1", result["OtherData"]["UserKey"].ToString());
            Assert.Equal("abc", result["OtherData"]["SupportedTypeRandom"].ToString());
            Assert.Equal(new string[] { "ObfuscatedUserName" }, result["OtherData"]["UserNameList"].ToObject<string[]>());
            Assert.Equal("2.5", Convert.ToString(result["OtherData"]["NotSupportedTypeRandom"], CultureInfo.InvariantCulture));

            Assert.Equal("ObfuscatedUserName", result["OtherDatas"][0]["UserName"].ToString());
            Assert.Equal("2.2.2.0", result["OtherDatas"][0]["IP"].ToString());
            Assert.Equal(string.Empty, result["OtherDatas"][0]["Authors"].ToString());
            Assert.Equal("-1", result["OtherDatas"][0]["UserKey"].ToString());
            Assert.Equal("def", result["OtherDatas"][0]["SupportedTypeRandom"].ToString());
            Assert.Equal(new string[] { "ObfuscatedUserName" }, result["OtherDatas"][0]["UserNameList"].ToObject<string[]>());
            Assert.Equal("3.3", Convert.ToString(result["OtherDatas"][0]["NotSupportedTypeRandom"], CultureInfo.InvariantCulture));

            Assert.Equal("ObfuscatedUserName", result["OtherDatas"][1]["UserName"].ToString());
            Assert.Equal("2.2.2.0", result["OtherDatas"][1]["IP"].ToString());
            Assert.Equal(string.Empty, result["OtherDatas"][1]["Authors"].ToString());
            Assert.Equal("-1", result["OtherDatas"][1]["UserKey"].ToString());
            Assert.Equal("def", result["OtherDatas"][1]["SupportedTypeRandom"].ToString());
            Assert.Equal(new string[] { "ObfuscatedUserName" }, result["OtherDatas"][1]["UserNameList"].ToObject<string[]>());
            Assert.Equal("3.3", Convert.ToString(result["OtherDatas"][1]["NotSupportedTypeRandom"], CultureInfo.InvariantCulture));
        }

        public class Data
        {
            [Obfuscate(ObfuscationType.UserName)]
            public string UserName { get; }

            [Obfuscate(ObfuscationType.IP)]
            public string IP { get; }

            [Obfuscate(ObfuscationType.Authors)]
            public string Authors { get; }

            [Obfuscate(ObfuscationType.UserKey)]
            public int? UserKey { get; }

            [Obfuscate(ObfuscationType.UserNameList)]
            public string[] UserNameList { get; }

            public string SupportedTypeRandom { get; }

            public double NotSupportedTypeRandom { get; }

            public Data OtherData { get; }

            public Data[] OtherDatas { get; }

            public Data()
            {
            }

            public Data(
                string userName, 
                string ip, 
                string authors, 
                int? userKey, 
                string[] usernameList,
                string supportedTypeRandom, 
                double notSupportedTypeRandom,
                Data otherData,
                Data[] otherDatas)
            {
                UserName = userName;
                Authors = authors;
                SupportedTypeRandom = supportedTypeRandom;
                NotSupportedTypeRandom = notSupportedTypeRandom;
                IP = ip;
                UserKey = userKey;
                UserNameList = usernameList;
                OtherData = otherData;
                OtherDatas = otherDatas;
            }
        }
    }
}
