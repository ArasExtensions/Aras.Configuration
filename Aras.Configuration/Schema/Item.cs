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
    public class Item
    {
        public Manager Manager { get; private set; }

        private XmlDocument Document;

        private XmlNode Node;

        public String ID
        {
            get
            {
                XmlAttribute id = this.Node.Attributes["id"];

                if (id != null)
                {
                    return id.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        public String ItemType
        {
            get
            {
                XmlAttribute type = this.Node.Attributes["type"];

                if (type != null)
                {
                    return type.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        public String GetProperty(String Name, String Default)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode != null)
            {
                XmlAttribute is_null = propnode.Attributes["is_null"];

                if (is_null != null && is_null.Value == "1")
                {
                    return null;
                }
                else
                {
                    return propnode.InnerText;
                }
            }
            else
            {
                return Default;
            }
        }

        private String _key;
        public String Key
        {
            get
            {
                if (this._key == null)
                {
                    String kepprop = this.Manager.Filter.ItemType(this.ItemType).Key;
                    this._key = this.GetProperty(kepprop, null);
                }

                return this._key;
            }
        }

        internal Item(Manager Manager, String Item)
        {
            this.Manager = Manager;
            this.Document = new XmlDocument();
            this.Document.LoadXml(Item);
            this.Node = this.Document.SelectSingleNode("Item");
        }
    }
}
