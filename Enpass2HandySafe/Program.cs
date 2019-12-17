using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

using Enpass2HandySafe.EnpassModel;
using Enpass2HandySafe.HandySafeModel;

// Convert Enpass6 items exported to json format to HandySafe 1.2 xml format 
namespace Enpass2HandySafe
{
    class Program
    {
        
        // Optional - list of ignored enpass categories (Tags), items will be skiped
        static List<string> enpassIgnoredFolders = new List<string>(){
            "dd032f9a-2bf5-42cf-a17f-47ab87e05b1c",
        };

        static string inputFile = @"/Users/macwhite/Desktop/all.json";

        static string outputFile = @"/Users/macwhite/Desktop/all-converted.xml";

        static void Main(string[] args)
        {
            try
            {
                if (!File.Exists(inputFile))
                {
                    throw new Exception($"Input file '{inputFile}' not found.");
                }

                string jsonData = File.ReadAllText(inputFile);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                Enpass enpass = JsonConvert.DeserializeObject<Enpass>(jsonData);

                Dictionary<string, string> enpassFolders = new Dictionary<string, string>();
                foreach (EnpassModel.Folder enpassFolder in enpass.Folders)
                {
                    if (!enpassIgnoredFolders.Contains(enpassFolder.Uuid))
                    {
                        enpassFolders.Add(enpassFolder.Uuid, enpassFolder.Title);
                    }
                }

                HandySafe handySafe = new HandySafe()
                {
                    Folders = new List<HandySafeModel.Folder>()
                };

                // Items without Tag (uncategorized)
                IEnumerable<EnpassModel.Item> enpassUncategorizedItems = enpass.Items
                    .Where(i => i.Folders == null || i.Folders.Length == 0);

                if (enpassUncategorizedItems.Count() > 0)
                {
                    HandySafeModel.Folder hsFolder = new HandySafeModel.Folder()
                    {
                        Name = "Nezařazeno",
                        Cards = new List<Card>()
                    };
                    handySafe.Folders.Add(hsFolder);

                    foreach (EnpassModel.Item enpassUncategorizedItem in enpassUncategorizedItems)
                    {
                        Console.WriteLine(hsFolder.Name + ": " + enpassUncategorizedItem.Title);
                        HandySafeModel.Card hsCard = ConvertItemToCard(enpassUncategorizedItem);
                        hsFolder.Cards.Add(hsCard);
                    }
                }

                // Items with Tag (categorized)
                foreach (KeyValuePair<string, string> enpassFolder in enpassFolders)
                {
                    HandySafeModel.Folder hsFolder = new HandySafeModel.Folder()
                    {
                        Name = enpassFolder.Value,
                        Cards = new List<Card>()
                    };

                    IEnumerable<EnpassModel.Item> enpassFolderItems = enpass.Items
                        .Where(i => i.Folders != null &&
                        i.Folders.ToList().Contains(enpassFolder.Key));

                    foreach (EnpassModel.Item enpassFolderItem in enpassFolderItems)
                    {
                        Console.WriteLine(hsFolder.Name + ": " + enpassFolderItem.Title);
                        HandySafeModel.Card hsCard = ConvertItemToCard(enpassFolderItem);
                        hsFolder.Cards.Add(hsCard);
                    }

                    handySafe.Folders.Add(hsFolder);
                }

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(HandySafeModel.HandySafe));

                XmlSerializerNamespaces emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.OmitXmlDeclaration = true;

                using (XmlWriter xw = XmlWriter.Create(outputFile, xmlWriterSettings))
                {
                    xmlSerializer.Serialize(xw, handySafe, emptyNamespaces);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Convert Enpass Item to HandySafe Card
        /// </summary>
        private static HandySafeModel.Card ConvertItemToCard(EnpassModel.Item enpassItem)
        {
            List<HandySafeModel.Field> hsFields = new List<HandySafeModel.Field>();
            if (enpassItem.Fields != null)
            {
                foreach (EnpassModel.Field enpassField in enpassItem.Fields)
                {
                    if (String.IsNullOrEmpty(enpassField.Value))
                    {
                        continue;
                    }

                    HandySafeModel.Field hsField = new HandySafeModel.Field()
                    {
                        Name = enpassField.Label,
                        Value = enpassField.Value
                    };

                    int? fieldType = MapFieldType(enpassField);
                    if (fieldType.HasValue)
                    {
                        hsField.Type = fieldType.ToString();
                    }

                    hsFields.Add(hsField);
                }
            }

            int hsIcon = MapIcon(enpassItem);
            HandySafeModel.Card hsCard = new HandySafeModel.Card()
            {
                Name = enpassItem.Title,
                Icon = hsIcon.ToString(),
                Fields = hsFields
            };

            // Add Note in enpassItem do hsCard and replace newline 
            if (!String.IsNullOrEmpty(enpassItem.Note))
            {
                hsCard.Note = enpassItem.Note;//.Replace("|", System.Environment.NewLine);
                hsCard.Note = enpassItem.Note.Replace(System.Environment.NewLine, " | ");
            }

            return hsCard;
        }

        /// <summary>
        /// Map Enpass icon (from item) to HandySafe icon (in card)
        /// </summary>
        private static int MapIcon(EnpassModel.Item enpassItem)
        {
            int hsIcon = 5; // default
            if (enpassItem?.Icon?.Image?.File != null)
            {
                switch (enpassItem.Icon.Image.File)
                {
                    case "misc/bank":
                    case "misc/finance":
                        hsIcon = 1; // money
                        break;
                    case "misc/user":
                        //hsIcon = 6; // person
                        hsIcon = 8; // identity card 
                        break;
                    case "misc/secure_note":
                        hsIcon = 17; // note
                        break;
                    case "misc/software_license":
                    case "misc/driving_license_1":
                    case "misc/driving_license_2":
                        hsIcon = 21; // licence card
                        break;
                    case "misc/login":
                        hsIcon = 19; // Key
                        break;
                    case "misc/passport":
                        hsIcon = 25; // globe
                        //hsIcon = 31; // Globe with Pin
                        break;
                    case "cc/others":
                        //hsIcon = 32; // EC card
                        hsIcon = 33; // VISA
                        break;
                    case "misc/briefcase":
                        //hsIcon = 10;  //folder
                        hsIcon = 5; // lock
                        break;
                }
            }
            return hsIcon;
        }

        /// <summary>
        /// Map Enpass field Type to HandySafe field Type 
        /// </summary>
        private static int? MapFieldType(EnpassModel.Field enpassField)
        {
            int? hsFieldType = null;
            /*
             * -   plain text (field without Type attribute)
             * 1   number
             * 2   phone
             * 3   date
             * 6   sesitive text
             */
            if (enpassField.Sensitive == 1)
            {
                hsFieldType = 6;
            }
            else
            {
                switch (enpassField.Type)
                {
                    case "numeric":
                        hsFieldType = 1;
                        break;
                    case "phone":
                        hsFieldType = 2;
                        break;
                    case "date":
                        hsFieldType = 3;
                        break;
                }
            }
            return hsFieldType;
        }
    }
}
