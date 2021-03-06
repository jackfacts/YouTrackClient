﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using YouTrack.Models.Extensions;

namespace YouTrack.Models.Serialization
{
    public class XmlAttributeElementDeserializer<TType>
    {
        private readonly XmlDeserializationEvents _deserializationEvents;

        private readonly Dictionary<XmlAttributeElement, PropertyInfo> _elementAttributeProperties;

        public XmlAttributeElementDeserializer()
        {
            IList<PropertyInfo> properties = typeof(TType).GetProperties();

            _elementAttributeProperties = new Dictionary<XmlAttributeElement, PropertyInfo>();

            foreach (var propertyInfo in properties)
            {
                var attribute = propertyInfo.GetCustomAttribute<XmlAttributeElementAttribute>();

                if (attribute == null) continue;

                _elementAttributeProperties.Add(attribute.AttributeElement, propertyInfo);
            }

            _deserializationEvents = new XmlDeserializationEvents();

            _deserializationEvents.OnUnknownElement += OnUnknownElement;
        }

        public XmlDeserializationEvents DeserializationEvents => _deserializationEvents;

        private void OnUnknownElement(object sender, XmlElementEventArgs xmlElementEventArgs)
        {
            var element = xmlElementEventArgs.Element;

            var nameAttribute = element.GetAttribute("name");

            if (string.IsNullOrEmpty(nameAttribute)) return;

            var tuple = new XmlAttributeElement
            {
                ElementName = element.Name,
                AttributeName = nameAttribute
            };

            var success = _elementAttributeProperties.ContainsKey(tuple);

            if (!success) return;

            var valueNode = xmlElementEventArgs.Element.SelectSingleNode("value");

            if (valueNode == null) return;

            var property = _elementAttributeProperties[tuple];
            SetProperty(property, xmlElementEventArgs.ObjectBeingDeserialized, valueNode.InnerText);
        }

        private void SetProperty(PropertyInfo property, object target, string value)
        {
            if (property.PropertyType == typeof(string))
            {
                property.SetValue(target, value);
                return;
            }
            if (property.PropertyType == typeof(int))
            {
                int i;

                var success = int.TryParse(value, out i);

                if (!success) return;

                property.SetValue(target, i);
                return;
            }
            if (property.PropertyType == typeof(long))
            {
                long i;

                var success = long.TryParse(value, out i);

                if (!success) return;

                property.SetValue(target, i);
                return;
            }
            if (property.PropertyType == typeof(short))
            {
                short i;

                var success = short.TryParse(value, out i);

                if (!success) return;

                property.SetValue(target, i);
                return;
            }
            if (property.PropertyType == typeof(DateTime))
            {
                DateTime dateTime;

                var success = DateTime.TryParse(value, out dateTime);

                if (success)
                {

                    property.SetValue(target, dateTime);
                    return;
                }
                long i;

                var tryLong = long.TryParse(value, out i);

                if (!tryLong) return;

                dateTime = i.ConvertToDateTime();
                
                property.SetValue(target, dateTime);

                return;
            }

        }
    }
}