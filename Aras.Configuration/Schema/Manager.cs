/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Aras.Configuration.Schema
{
    public abstract class Manager
    {
        public Configuration.Session Configuration { get; private set; }

        public Filter.Session Filter
        {
            get
            {
                return this.Configuration.Filter;
            }
        }

        public Logging.Log Log
        {
            get
            {
                return this.Configuration.Log;
            }
        }

        protected abstract void LoadSettings(XmlNode Settings);

        private Dictionary<String, Dictionary<String, Item>> ItemIDCache;

        private Dictionary<String, Dictionary<String, Item>> ItemKeyCache;

        internal void AddItemToCache(Item Item)
        {
            if (!this.ItemIDCache.ContainsKey(Item.ItemType.Name))
            {
                this.ItemIDCache[Item.ItemType.Name] = new Dictionary<String, Item>();
                this.ItemIDCache[Item.ItemType.Name][Item.ID] = Item;
            }
            else
            {
                if (!this.ItemIDCache[Item.ItemType.Name].ContainsKey(Item.ID))
                {
                    this.ItemIDCache[Item.ItemType.Name][Item.ID] = Item;
                }
                else
                {
                    throw new ArgumentException("Duplicate Item ID");
                }
            }
        }

        public IEnumerable<String> LoadedItemTypes
        {
            get
            {
                return this.ItemIDCache.Keys;
            }
        }

        public IEnumerable<Item> LoadedItems(String ItemType)
        {
            if (this.ItemIDCache.ContainsKey(ItemType))
            {
                return this.ItemIDCache[ItemType].Values;
            }
            else
            {
                throw new ArgumentException("ItemType not Loaded");
            }
        }

        public Item LoadedItem(String ItemType, String ID)
        {
            if (this.ItemIDCache.ContainsKey(ItemType))
            {
                if (this.ItemIDCache[ItemType].ContainsKey(ID))
                {
                    return this.ItemIDCache[ItemType][ID];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        protected abstract void LoadItems();

        private void Validate()
        {
            foreach(Filter.ItemType itemtype in this.Filter.RootItemTypes)
            {
                if (this.ItemIDCache.ContainsKey(itemtype.Name))
                {
                    foreach (Item item in this.ItemIDCache[itemtype.Name].Values)
                    {
                        item.Validate();
                    }
                }
            }
        }

        public void Load()
        {
            // Load Items
            this.LoadItems();

            // Validate Schema
            this.Validate();

            // Build Key Cache
        

        }

        public abstract void Save();

        public void Merge(Manager Target)
        {
            foreach(Filter.ItemType itemtype in this.Filter.RootItemTypes)
            {
                foreach(Item sourceitem in this.LoadedItems(itemtype.Name))
                {
                    Item targetitem = Target.LoadedItem(itemtype.Name, sourceitem.ID);

                    if (targetitem == null)
                    {
                        XmlDocument targetdocument = new XmlDocument();
                        targetdocument.LoadXml(sourceitem.GetString());
                        targetitem = new Item(Target, targetdocument);
                        targetitem.Action = Item.Actions.Add;
                    }
                    else
                    {

                    }
                }
            }
        }

        internal Manager(Configuration.Session Configuration, XmlNode Settings)
        {
            this.ItemIDCache = new Dictionary<String, Dictionary<String, Item>>();
            this.ItemKeyCache = new Dictionary<String, Dictionary<String, Item>>();
            this.Configuration = Configuration;
            
            // Load Settings
            this.LoadSettings(Settings);
        }
    }
}
