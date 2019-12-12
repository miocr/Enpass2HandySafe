using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Enpass2HandySafe.HandySafeModel
{
    [XmlRoot]
    public class HandySafe
    {
        [XmlElement("Folder")]
        public List<Folder> Folders;
    }

    public class Folder
    {
        [XmlElement("Card")]
        public List<Card> Cards;

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("id")]
        public string Id;
    }

    public class Card
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("icon")]
        public string Icon;

        [XmlElement("Field")]
        public List<Field> Fields;

        [XmlElement("Note")]
        public string Note;
    }

    public class Field
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
