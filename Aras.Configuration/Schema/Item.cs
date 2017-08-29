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
        public enum Actions { Get, Update, Add, Delete };

        public Manager Manager { get; private set; }

        internal XmlDocument Document { get; private set; }

        internal XmlNode Node { get; private set; }

        internal String GetString()
        {
            return this.Node.OuterXml;
        }

        public Actions Action { get; internal set; }

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

        private Filter.ItemType _itemType;
        public Filter.ItemType ItemType
        {
            get
            {
                if (this._itemType == null)
                {
                    XmlAttribute type = this.Node.Attributes["type"];

                    if (type != null)
                    {
                        this._itemType = this.Manager.Filter.ItemType(type.Value);
                    }
                    else
                    {
                        throw new ArgumentNullException("Missing ItemType");
                    }
                }

                return this._itemType;
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
                    String kepprop = this.ItemType.Key;
                    this._key = this.GetProperty(kepprop, null);
                }

                return this._key;
            }
        }

        private XmlNode _relationshipsNode;
        private XmlNode RelationshipsNode
        {
            get
            {
                if (this._relationshipsNode == null)
                {
                    this._relationshipsNode = this.Node.SelectSingleNode("Relationships");

                    if (this._relationshipsNode == null)
                    {
                        this._relationshipsNode = this.Document.CreateNode(XmlNodeType.Element, "Relationships", null);
                        this.Node.AppendChild(this._relationshipsNode);
                    }
                }

                return _relationshipsNode;
            }
        }

        private List<Item> RelationshipsCache;

        internal void Validate()
        {

        }

        internal Item(Manager Manager, XmlDocument Document)
            : this(Manager, Document, Document.SelectSingleNode("Item"))
        {
    
        }

        internal Item(Manager Manager, XmlDocument Document, XmlNode Node)
        {
            this.Manager = Manager;
            this.Document = Document;
            this.Node = Node;
            this.Action = Actions.Get;

            // Add this Item to Cache
            Manager.AddItemToCache(this);

            // Add Relationships to Cache
            this.RelationshipsCache = new List<Item>();

            foreach (XmlNode relnode in this.RelationshipsNode.ChildNodes)
            {
                Item relitem = new Item(this.Manager, this.Document, relnode);
            }
        }
    }
}
