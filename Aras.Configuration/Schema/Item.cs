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
                    throw new ArgumentNullException("Missing id node");
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
                        throw new ArgumentNullException("Missing ItemType node");
                    }
                }

                return this._itemType;
            }
        }

        public IEnumerable<String> PropertyNames
        {
            get
            {
                List<String> ret = new List<String>();
                
                foreach(XmlNode node in this.Node.ChildNodes)
                {
                    switch(node.Name)
                    {
                        case "id":
                        case "Relationships":
                            break;
                        default:
                            ret.Add(node.Name);
                            break;
                    }
                }

                return ret;
            }
        }

        public Boolean IsPropertyItem(String Name)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode != null)
            {
                XmlAttribute typeattr = propnode.Attributes["type"];

                if (typeattr != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new ArgumentException("Invalid Property Name");
            }
        }

        public String GetPropertyItemType(String Name)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode != null)
            {
                XmlAttribute typeattr = propnode.Attributes["type"];

                if (typeattr != null)
                {
                    return typeattr.InnerText;
                }
                else
                {
                    throw new ArgumentNullException("Missing type attribute");
                }
            }
            else
            {
                throw new ArgumentException("Invalid Property Name");
            }
        }

        public String GetPropertyItemID(String Name)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode != null)
            {
                XmlAttribute typeattr = propnode.Attributes["type"];

                if (typeattr != null)
                {
                    XmlNode itemnode = propnode.SelectSingleNode("Item");

                    if (itemnode != null)
                    {
                        XmlNode idnode = itemnode.SelectSingleNode("id");
                        return idnode.InnerText;
                    }
                    else
                    {
                        throw new ArgumentNullException("Missing Item node");
                    }
                }
                else
                {
                    throw new ArgumentNullException("Missing type attribute");
                }
            }
            else
            {
                throw new ArgumentException("Invalid Property Name");
            }
        }

        public String GetProperty(String Name)
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
                throw new ArgumentException("Invalid Property Name");
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
                    this._key = this.GetProperty(kepprop);
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

        public IEnumerable<Item> Relationships
        {
            get
            {
                return this.RelationshipsCache;
            }
        }

        internal void Validate()
        {
            foreach(String propname in this.PropertyNames)
            {
                if (this.IsPropertyItem(propname))
                {
                    String propitemtype = this.GetPropertyItemType(propname);
                    String propitemid = this.GetPropertyItemID(propname);
                    Item propitem = this.Manager.LoadedItem(propitemtype, propitemid);

                    if (propitem == null)
                    {
                        this.Manager.Log.Add(Logging.Levels.Error, "Item not loaded: " + propitemtype + ": " + propitemid);
                    }
                }
            }

            foreach(Item relationship in this.Relationships)
            {
                relationship.Validate();
            }
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
                this.RelationshipsCache.Add(relitem);
            }
        }
    }
}
