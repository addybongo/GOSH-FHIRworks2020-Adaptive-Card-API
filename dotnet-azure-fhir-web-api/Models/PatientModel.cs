﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using HDR_UK_Web_Application.Models;
//
//    var patient = Patient.FromJson(jsonString);

namespace HDR_UK_Web_Application.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class PatientModel
    {
        [JsonProperty("id")]
        public string id { get; set; }

        //Jargon
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        //Jargon
        [JsonProperty("text")]
        public Text Text { get; set; }

        //Jargon
        [JsonProperty("extension")]
        public PatientExtension[] Extension { get; set; }

        [JsonProperty("identifier")]
        public Identifier[] Identifier { get; set; }

        [JsonProperty("name")]
        public Name[] Name { get; set; }

        [JsonProperty("telecom")]
        public Telecom[] Telecom { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("birthDate")]
        public DateTimeOffset BirthDate { get; set; }

        [JsonProperty("deceasedDateTime")]
        public DateTimeOffset DeceasedDateTime { get; set; }

        [JsonProperty("address")]
        public Address[] Address { get; set; }

        [JsonProperty("maritalStatus")]
        public MaritalStatus MaritalStatus { get; set; }

        [JsonProperty("multipleBirthBoolean")]
        public bool MultipleBirthBoolean { get; set; }

        [JsonProperty("communication")]
        public Communication[] Communication { get; set; }
        
        [JsonProperty("generalPractitioner")]
        public string assignedGP { get; set; }
        
        [JsonProperty("managingOrganization")]
        public string gpOrganization { get; set; }
    }

    public partial class Address
    {
        //Stores latitude and longitudinal coordinates
        [JsonProperty("extension")]
        public AddressExtension[] Extension { get; set; }

        //Road Name and House/Apt Number
        [JsonProperty("line")]
        public string[] Line { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
 
    //Stores data about latitude and longitude (advanced stuff)
    //Address/...
    public partial class AddressExtension
    {
        [JsonProperty("extension")]
        public AddressExtensionExtension[] Extension { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    //Address/extension/...
    public partial class AddressExtensionExtension
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("valueDecimal")]
        public double ValueDecimal { get; set; }
    }

    public partial class Communication
    {
        [JsonProperty("language")]
        public Language Language { get; set; }
    }

    //Communication/<list>/...
    public partial class Language
    {
        [JsonProperty("coding")]
        public Coding[] Coding { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
    
    public partial class MaritalStatus
    {
        [JsonProperty("coding")]
        public Coding[] Coding { get; set; }

        //Marital Status Value (can be any one of: A, D, I, L, M, P, S, T, U, W, or UNK)
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    //Used throughout JSON file
    //Used to store links to the FHIR schemas for each file
    //Don't touch these
    public partial class Coding
    {
        [JsonProperty("system")]
        public string System { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; }
    }

    public partial class PatientExtension
    {
        [JsonProperty("extension", NullValueHandling = NullValueHandling.Ignore)]
        public Extension[] Extension { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("valueString", NullValueHandling = NullValueHandling.Ignore)]
        public string ValueString { get; set; }

        [JsonProperty("valueCode", NullValueHandling = NullValueHandling.Ignore)]
        public string ValueCode { get; set; }

        [JsonProperty("valueAddress", NullValueHandling = NullValueHandling.Ignore)]
        public ValueAddress ValueAddress { get; set; }

        [JsonProperty("valueDecimal", NullValueHandling = NullValueHandling.Ignore)]
        public double? ValueDecimal { get; set; }
    }

    //extension/list/...
    public partial class Extension
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("valueCoding", NullValueHandling = NullValueHandling.Ignore)]
        public Coding ValueCoding { get; set; }

        [JsonProperty("valueString", NullValueHandling = NullValueHandling.Ignore)]
        public string ValueString { get; set; }
    }

    //Stores Address Values
    //extension/list/...
    public partial class ValueAddress
    {
        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public partial class Identifier
    {
        [JsonProperty("system")]
        public string System { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public MaritalStatus Type { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("versionId")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long VersionId { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }
    }

    public partial class Name
    {
        [JsonProperty("use")]
        public string Use { get; set; }

        [JsonProperty("family")]
        public string Family { get; set; }

        [JsonProperty("given")]
        public string[] Given { get; set; }

        [JsonProperty("prefix")]
        public string[] Prefix { get; set; }
    }

    public partial class Telecom
    {
        [JsonProperty("system")]
        public string System { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("use")]
        public string Use { get; set; }
    }

    public partial class Text
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("div")]
        public string Div { get; set; }
    }

    public partial class Patient
    {
        public static Patient FromJson(string json) => JsonConvert.DeserializeObject<Patient>(json, HDR_UK_Web_Application.Models.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Patient self) => JsonConvert.SerializeObject(self, HDR_UK_Web_Application.Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
