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
        static List<string> ignoredFolders = new List<string>() { "dd032f9a-2bf5-42cf-a17f-47ab87e05b1c" };
        static KeyValuePair<string, string> defaultFolder =
            new KeyValuePair<string, string>("xxxx-xxxx-xxxx-xxxx", "Default");


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string jsonData = File.ReadAllText("/Users/macwhite/Desktop/all.json");
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();

                Enpass enpass = JsonConvert.DeserializeObject<Enpass>(jsonData);

                Dictionary<string, string> enpassFolders = new Dictionary<string, string>();
                enpassFolders.Add(defaultFolder.Key, defaultFolder.Value);
                foreach (EnpassModel.Folder enpassFolder in enpass.Folders)
                {
                    if (ignoredFolders.Contains(enpassFolder.Uuid))
                        continue;
                    enpassFolders.Add(enpassFolder.Uuid, enpassFolder.Title);
                }

                // Fix pro items, ktere nejsou v zadne kategorii, zaradime do default
                /*
                foreach (var item in enpass.Items)
                {
                    if (item.Folders == null || item.Folders.Length == 0)
                    {
                        item.Folders = new string[1] { defaultFolder.Key };
                    }
                }
                */

                // Items nezarazene do zadneho folder
                var uncategorizedItems = enpass.Items.Where(i => i.Folders?.Length == 0);
                foreach (var uncategorizedItem in uncategorizedItems)
                {
                    Console.WriteLine(uncategorizedItem.Title);
                }

                foreach (KeyValuePair<string, string> enpassFolder in enpassFolders)
                {
                    var folderItems = enpass.Items
                        .Where(i => i.Folders?.Length > 0 && i.Folders.ToList()
                        .Contains(enpassFolder.Key));

                    foreach (var folderItem in folderItems)
                    {
                        Console.WriteLine(folderItem.Title);
                    }
                }

                HandySafe handySafe = new HandySafe();
                List<HandySafeModel.Folder> hsFolders = new List<HandySafeModel.Folder>();

                HandySafeModel.Folder hsFolder = new HandySafeModel.Folder()
                {
                    Name = "MainFolder",
                    Id = "1"
                };

                hsFolders.Add(hsFolder);

                List<HandySafeModel.Card> hsCards = new List<Card>();

                foreach (EnpassModel.Item enpassItem in enpass.Items)
                {
                    List<HandySafeModel.Field> hsFields = new List<HandySafeModel.Field>();
                    foreach (EnpassModel.Field enpassField in enpassItem.Fields)
                    {
                        HandySafeModel.Field hsField = new HandySafeModel.Field()
                        {
                            Name = enpassField.Label,
                            Type = enpassField.Type,
                            Value = enpassField.Value
                        };
                        hsFields.Add(hsField);
                    }

                    HandySafeModel.Card hsCard = new HandySafeModel.Card()
                    {
                        Name = enpassItem.Title,
                        Fields = hsFields
                    };

                    hsCards.Add(hsCard);
                }

                hsFolder.Cards = hsCards;

                handySafe.Folders = hsFolders;

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(HandySafeModel.HandySafe));

                TextWriter tw = new StreamWriter(@"/Users/macwhite/Desktop/all-converted.xml");
                xmlSerializer.Serialize(tw, handySafe);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
