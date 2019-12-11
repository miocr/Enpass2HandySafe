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

        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Id;
    }

    public class Card
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Icon;

        [XmlElement("Field")]
        public List<Field> Fields;

        [XmlElement("Note")]
        public string Note;
    }

    public class Field
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
