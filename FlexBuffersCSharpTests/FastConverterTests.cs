using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FlexBuffers;

namespace FlexBuffersCSharpTests
{
    [TestFixture]
    public class FastConverterTests
    {
        [Test]
        public void ComplexMap()
        {

            const string json = @"
{
    ""age"": 35,
    ""weight"": 72.5,
    ""name"": ""Maxim"",
    ""flags"": [true, false, true, true],
    ""something"": null,
    ""address"": {
        ""city"": ""Bla"",
        ""zip"": ""12345"",
        ""countryCode"": ""XX""
    }
}
";
            var bytes = JsonToFlexBufferFastConverter.Convert(json);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(6, flx.AsMap.Length);

            Assert.AreEqual(35, flx["age"].AsLong);
            Assert.AreEqual(72.5, flx["weight"].AsDouble);
            Assert.AreEqual("Maxim", flx["name"].AsString);
            Assert.AreEqual(true, flx["something"].IsNull);

            Assert.AreEqual(4, flx["flags"].AsVector.Length);
            Assert.AreEqual(true, flx["flags"][0].AsBool);
            Assert.AreEqual(false, flx["flags"][1].AsBool);
            Assert.AreEqual(true, flx["flags"][2].AsBool);
            Assert.AreEqual(true, flx["flags"][3].AsBool);

            Assert.AreEqual(3, flx["address"].AsMap.Length);
            Assert.AreEqual("Bla", flx["address"]["city"].AsString);
            Assert.AreEqual("12345", flx["address"]["zip"].AsString);
            Assert.AreEqual("XX", flx["address"]["countryCode"].AsString);
        }
    }
}
