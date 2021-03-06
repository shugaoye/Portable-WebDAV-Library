﻿using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'propertyupdate' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [DebuggerStepThrough]
    [XmlType(TypeName = WebDavConstants.PropertyUpdate, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class PropertyUpdate
    {
        /// <summary>
        /// Gets or sets the Items.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Remove, Type = typeof(Remove))]
        [XmlElement(ElementName = WebDavConstants.Set, Type = typeof(Set))]
        public object[] Items
        {
            get;
            set;
        }
    }
}
