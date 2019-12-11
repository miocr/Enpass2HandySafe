using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

using Enpass2HandySafe.EnpassModel;
using Enpass2HandySafe.HandySafeModel;


namespace Enpass2HandySafe
{
    class Program
    {
        static List<string> ignoredFolders = new List<string>()
        { "dd032f9a-2bf5-42cf-a17f-47ab87e05b1c" };

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string jsonData = File.ReadAllText("/Users/macwhite/Desktop/all.json");
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();

                Enpass enpass = JsonConvert.DeserializeObject<Enpass>(jsonData);

                Dictionary<string, string> enpassFolders = new Dictionary<string, string>();
                foreach (EnpassModel.Folder enpassFolder in enpass.Folders)
                {
                    if (ignoredFolders.Contains(enpassFolder.Uuid))
                        continue;
                    enpassFolders.Add(enpassFolder.Uuid, enpassFolder.Title);
                }

                HandySafe handySafe = new HandySafe()
                {
                    Folders = new List<HandySafeModel.Folder>()
                };

                // Items nezarazene do zadneho folder
                var enpassUncategorizedItems = enpass.Items.Where(i => i.Folders == null || i.Folders.Length == 0);
                if (enpassUncategorizedItems.Count() > 0)
                {
                    HandySafeModel.Folder hsFolder = new HandySafeModel.Folder()
                    {
                        Name = "Nezarazeno",
                        Cards = new List<Card>()
                    };
                    handySafe.Folders.Add(hsFolder); 

                    foreach (var enpassUncategorizedItem in enpassUncategorizedItems)
                    {
                        Console.WriteLine("Nezarazeno: " + enpassUncategorizedItem.Title);
                        HandySafeModel.Card hsCard = ConvertItemToCard(enpassUncategorizedItem);
                        hsFolder.Cards.Add(hsCard);
                    }

                }

                foreach (KeyValuePair<string, string> enpassFolder in enpassFolders)
                {
                    HandySafeModel.Folder hsFolder = new HandySafeModel.Folder()
                    {
                        Name = enpassFolder.Value,
                        Cards = new List<Card>()
                    };

                    var enpassFolderItems = enpass.Items
                        .Where(i => i.Folders != null &&
                        i.Folders.ToList().Contains(enpassFolder.Key));

                    foreach (var enpassItem in enpassFolderItems)
                    {
                        Console.WriteLine(hsFolder.Name + ": " + enpassItem.Title);
                        HandySafeModel.Card hsCard = ConvertItemToCard(enpassItem);
                        hsFolder.Cards.Add(hsCard);
                    }

                    handySafe.Folders.Add(hsFolder);

                }

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(HandySafeModel.HandySafe));

                TextWriter tw = new StreamWriter(@"/Users/macwhite/Desktop/all-converted.xml");
                xmlSerializer.Serialize(tw, handySafe);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static HandySafeModel.Card ConvertItemToCard(EnpassModel.Item enpassItem)
        {
            List<HandySafeModel.Field> hsFields = new List<HandySafeModel.Field>();
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

            HandySafeModel.Card hsCard = new HandySafeModel.Card()
            {
                Name = enpassItem.Title,
                Fields = hsFields
            };

            // Pokud existuje poznamka, pridame ji a znak | na nove radky
            if (!String.IsNullOrEmpty(enpassItem.Note))
            {
                hsCard.Note = enpassItem.Note;//.Replace("|", System.Environment.NewLine);
                hsCard.Note = enpassItem.Note.Replace(System.Environment.NewLine, " | ");
            }


            return hsCard;
        }

        private static int? MapFieldType(EnpassModel.Field enpassField)
        {
            int? hsFieldType = null;
            /*
             * nic text
             * 1   number
             * 2   telefon
             * 3   datum
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
