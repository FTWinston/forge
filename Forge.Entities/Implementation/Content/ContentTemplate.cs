﻿// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Forge.Collections;
using Forge.Entities.Implementation.Runtime;
using Forge.Entities.Implementation.Shared;
using Forge.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Forge.Entities.Implementation.Content {
    [JsonConverter(typeof(ContentTemplateSerializationFormatConverter))]
    internal class ContentTemplateSerializationFormat {
        public int TemplateId;
        public string PrettyName;
        public List<Data.IData> DefaultDataInstances;
    }

    /// <summary>
    /// A custom converter for ContentTemplateSerializationFormat. This supports a more
    /// sophisticated serialization format that only emits data as necessary and additionally allows
    /// for custom converters to be defined on Data.IData derived types.
    /// </summary>
    internal class ContentTemplateSerializationFormatConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new NotSupportedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var instance = existingValue as ContentTemplateSerializationFormat;
            if (instance == null) {
                instance = new ContentTemplateSerializationFormat();
            }

            JObject obj = serializer.Deserialize<JObject>(reader);

            instance.TemplateId = Read<int>(obj, "TemplateId");
            instance.PrettyName = Read<string>(obj, "PrettyName") ?? "";
            instance.DefaultDataInstances = new List<Data.IData>();

            JArray dataInstances = obj["DefaultDataInstances"].ToObject<JArray>(serializer);
            foreach (JObject dataInstance in dataInstances) {
                Type dataType = dataInstance["DataType"].ToObject<Type>(serializer);
                Data.IData data = TryReadData(dataInstance, serializer, "Data", dataType);
                instance.DefaultDataInstances.Add(data);
            }

            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var instance = (ContentTemplateSerializationFormat)value;

            JObject instanceObj = new JObject();

            instanceObj["TemplateId"] = instance.TemplateId;
            if (string.IsNullOrEmpty(instance.PrettyName) == false) instanceObj["PrettyName"] = instance.PrettyName;

            JArray array = new JArray();
            foreach (Data.IData data in instance.DefaultDataInstances) {
                JObject dataObj = new JObject();
                dataObj["DataType"] = JToken.FromObject(data.GetType(), serializer);
                dataObj["Data"] = JToken.FromObject(data, serializer);
                array.Add(dataObj);
            }
            instanceObj["DefaultDataInstances"] = array;

            serializer.Serialize(writer, instanceObj);
        }

        private static T Read<T>(JObject obj, string key) {
            JToken token = obj[key];
            if (token == null) {
                return default(T);
            }

            return token.Value<T>();
        }

        private static Data.IData TryReadData(JObject obj, JsonSerializer serializer,
            string key, Type dataType) {

            JToken token = obj[key];
            if (token == null) {
                return null;
            }

            return (Data.IData)token.ToObject(dataType, serializer);
        }
    }

    [JsonConverter(typeof(QueryableEntityConverter))]
    internal class ContentTemplate : ITemplate {
        public ContentTemplateSerializationFormat GetSerializedFormat() {
            return new ContentTemplateSerializationFormat() {
                DefaultDataInstances = _defaultDataInstances.Select(pair => pair.Value).ToList(),
                TemplateId = TemplateId,
                PrettyName = PrettyName
            };
        }

        private SparseArray<Data.IData> _defaultDataInstances;

        [JsonProperty("TemplateId")]
        public int TemplateId {
            get;
            private set;
        }

        [JsonProperty("PrettyName")]
        public string PrettyName {
            get;
            set;
        }

        public ContentTemplate(int id) {
            _defaultDataInstances = new SparseArray<Data.IData>();
            TemplateId = id;
            PrettyName = "";
        }

        public ContentTemplate(ITemplate template)
            : this(template.TemplateId) {
            PrettyName = template.PrettyName;

            foreach (DataAccessor accessor in template.SelectData()) {
                AddDefaultData(template.Current(accessor));
            }
        }

        /// <summary>
        /// Initializes the ContentTemplate with data from the given ContentTemplate.
        /// </summary>
        public void Initialize(ContentTemplateSerializationFormat template) {
            TemplateId = template.TemplateId;
            PrettyName = template.PrettyName;

            foreach (Data.IData data in template.DefaultDataInstances) {
                _defaultDataInstances[DataAccessorFactory.GetId(data)] = data;
            }
        }

        /// <summary>
        /// Adds a default data instance to the template. The template "owns" the passed data
        /// instance; a copy is not made of it.
        /// </summary>
        /// <remarks>
        /// If the ITemplate is currently being backed by an IGameEngine, this will throw an
        /// InvalidOperationException.
        /// </remarks>
        /// <param name="data">The data instance to copy from.</param>
        public void AddDefaultData(Data.IData data) {
            _defaultDataInstances[DataAccessorFactory.GetId(data)] = data;
        }

        /// <summary>
        /// Remove the given type of data from the template instance. New instances will not longer
        /// have this added to the template.
        /// </summary>
        /// <remarks>
        /// If the ITemplate is currently being backed by an IGameEngine, this will throw an
        /// InvalidOperationException.
        /// </remarks>
        /// <param name="accessor">The type of data to remove.</param>
        /// <returns>True if the data was removed.</returns>
        public bool RemoveDefaultData(DataAccessor accessor) {
            return _defaultDataInstances.Remove(accessor.Id);
        }

        public IEntity Instantiate() {
            throw new InvalidOperationException("Unable to instantiate an entity from a ContentTemplate");
        }

        public ICollection<DataAccessor> SelectData(bool includeRemoved = false,
            Predicate<DataAccessor> filter = null, ICollection<DataAccessor> storage = null) {
            if (storage == null) {
                storage = new List<DataAccessor>();
            }

            foreach (var pair in _defaultDataInstances) {
                DataAccessor accessor = new DataAccessor(pair.Key);
                if (filter == null || filter(accessor)) {
                    storage.Add(accessor);
                }
            }

            return storage;
        }

        public Data.IData Current(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _defaultDataInstances[accessor.Id];
        }

        public Data.IVersioned Previous(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            if (_defaultDataInstances[accessor.Id] is Data.IVersioned == false) {
                throw new PreviousRequiresVersionedDataException(this, accessor);
            }

            return (Data.IVersioned)_defaultDataInstances[accessor.Id];
        }

        public bool ContainsData(DataAccessor accessor) {
            return _defaultDataInstances.ContainsKey(accessor.Id);
        }

        public bool WasModified(DataAccessor accessor) {
            return false;
        }

        public override string ToString() {
            if (PrettyName.Length > 0) {
                return string.Format("Template [tid={0}, name={1}]", TemplateId, PrettyName);
            }
            else {
                return string.Format("Template [tid={0}]", TemplateId);
            }
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        public override bool Equals(Object obj) {
            return obj is ITemplate && this == (ITemplate)obj;
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode() {
            return TemplateId.GetHashCode();
        }
        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        public static bool operator ==(ContentTemplate x, ITemplate y) {
            if (x.TemplateId != y.TemplateId) return false;
            if (x.PrettyName != y.PrettyName) return false;

            // TODO: should we verify the data is also equal?
            return true;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are not equal.
        /// </summary>
        public static bool operator !=(ContentTemplate x, ITemplate y) {
            return !(x == y);
        }

    }
}