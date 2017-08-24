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
using System.IO;
using System.Xml;

namespace Aras.Configuration.Filter
{
    public class Session
    {
        public FileInfo Filename { get; private set; }

        public Logging.Log Log { get; private set; }

        private List<String> SystemPropertiesCache;

        public IEnumerable<String> SystemProperties
        {
            get
            {
                return this.SystemPropertiesCache;
            }
        }

        private Dictionary<String, ItemType> ItemTypesCache;

        public IEnumerable<ItemType> ItemTypes
        {
            get
            {
                return this.ItemTypesCache.Values;
            }
        }

        private void Load()
        {
            // Create Caches
            this.SystemPropertiesCache = new List<String>();
            this.ItemTypesCache = new Dictionary<String,ItemType>();

            // Open XML File
            XmlDocument doc = new XmlDocument();
            doc.Load(this.Filename.FullName);
            XmlNode filterNode = doc.SelectSingleNode("filter");

            // Load System Properties
            XmlNode systemPropertiesNode = filterNode.SelectSingleNode("systemproperties");

            if (systemPropertiesNode != null)
            {
                foreach (XmlNode propertyNode in systemPropertiesNode.SelectNodes("property"))
                {
                    String property = propertyNode.InnerText;

                    if (!String.IsNullOrEmpty(property))
                    {
                        if (!this.SystemPropertiesCache.Contains(property))
                        {
                            this.SystemPropertiesCache.Add(property);
                        }
                    }
                }
            }
            else
            {
                this.Log.Add(Logging.Levels.Error, "Filter does not contain a systemproperties node");
            }

            // Load ItemTypes
            XmlNode itemtypesNode = filterNode.SelectSingleNode("itemtypes");

            if (itemtypesNode != null)
            {
                foreach(XmlNode itemtypeNode in itemtypesNode.SelectNodes("itemtype"))
                {
                    ItemType itemtype = new ItemType(itemtypeNode);

                    if (!this.ItemTypesCache.ContainsKey(itemtype.Name))
                    {
                        this.ItemTypesCache[itemtype.Name] = itemtype;
                    }
                }
            }
            else
            {
                this.Log.Add(Logging.Levels.Error, "Filter does not contain an itemtypes node");
            }
        }

        internal Session(XmlNode Configuration, Logging.Log Log)
        {
            this.Filename = new FileInfo(Configuration.InnerText);
            this.Load();
        }
    }
}
