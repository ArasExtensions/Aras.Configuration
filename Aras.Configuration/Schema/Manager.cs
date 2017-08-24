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

        private Dictionary<String, ItemType> ItemTypesCache;

        public IEnumerable<ItemType> ItemTypes
        {
            get
            {
                return this.ItemTypesCache.Values;
            }
        }

        public ItemType ItemType(String Name)
        {
            if (this.ItemTypesCache.ContainsKey(Name))
            {
                return this.ItemTypesCache[Name];
            }
            else
            {
                throw new ArgumentException("Invalid ItemType Name");
            }
        }

        protected abstract IEnumerable<ItemType> LoadItemTypes();

        private Dictionary<String, Dictionary<String, Item>> ItemKeyCache;

        protected void AddItem(Item Item)
        {
            if (!this.ItemKeyCache.ContainsKey(Item.ItemType))
            {
                this.ItemKeyCache[Item.ItemType] = new Dictionary<String, Item>();
                this.ItemKeyCache[Item.ItemType][Item.Key] = Item;
            }
            else
            {
                if (!this.ItemKeyCache[Item.ItemType].ContainsKey(Item.Key))
                {
                    this.ItemKeyCache[Item.ItemType][Item.Key] = Item;
                }
                else
                {
                    throw new ArgumentException("Duplicate Item Key");
                }
            }
        }

        public abstract void Load();

        public abstract void Save();

        public void Merge(Manager Target)
        {

        }

        internal Manager(Configuration.Session Configuration, XmlNode Settings)
        {
            this.ItemKeyCache = new Dictionary<String, Dictionary<String, Item>>();
            this.Configuration = Configuration;
            this.LoadSettings(Settings);
            this.ItemTypesCache = new Dictionary<String, ItemType>();
            
            foreach(ItemType itemtype in this.LoadItemTypes())
            {
                if (!this.ItemTypesCache.ContainsKey(itemtype.Name))
                {
                    this.ItemTypesCache[itemtype.Name] = itemtype;
                }
            }
        }
    }
}
