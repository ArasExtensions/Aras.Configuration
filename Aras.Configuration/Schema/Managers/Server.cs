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

namespace Aras.Configuration.Schema.Managers
{
    public class Server : Manager
    {
        public String URL { get; private set; }

        public String Database { get; private set; }

        public String Username { get; private set; }

        public String Password { get; private set; }

        protected override void LoadSettings(XmlNode Settings)
        {
            XmlNode urlNode = Settings.SelectSingleNode("url");

            if (urlNode != null)
            {
                this.URL = urlNode.InnerText;
            }
            else
            {
                this.Log.Add(Logging.Levels.Error, "Server Schema Manager settings do not contain a url node");
            }

            XmlNode databaseNode = Settings.SelectSingleNode("database");

            if (databaseNode != null)
            {
                this.Database = databaseNode.InnerText;
            }
            else
            {
                this.Log.Add(Logging.Levels.Error, "Server Schema Manager settings do not contain a database node");
            }

            XmlNode usernameNode = Settings.SelectSingleNode("username");

            if (usernameNode != null)
            {
                this.Username = usernameNode.InnerText;
            }
            else
            {
                this.Log.Add(Logging.Levels.Error, "Server Schema Manager settings do not contain a username node");
            }

            XmlNode passwordNode = Settings.SelectSingleNode("password");

            if (passwordNode != null)
            {
                this.Password = IO.Server.PasswordHash(passwordNode.InnerText);
            }
            else
            {
                this.Log.Add(Logging.Levels.Error, "Server Schema Manager settings do not contain a password node");
            }
        }

        private void AddRelationshipTypes(IO.Request Request, IO.Item Source, Filter.ItemType FilterItemType)
        {
            foreach (Filter.RelationshipType filterrelationshiptype in FilterItemType.RelationshipTypes)
            {
                IO.Item item = Request.NewItem(filterrelationshiptype.Name, "get");
                item.Select = this.ItemType(filterrelationshiptype.Name).Select;
                this.AddRelationshipTypes(Request, item, filterrelationshiptype);
                Source.AddRelationship(item);
            }
        }

        protected override void LoadItems()
        {
            foreach(Filter.ItemType filteritemtype in this.Filter.RootItemTypes)
            {
                // Connect to Server
                IO.Server server = new IO.Server(this.URL);
                IO.Database database = server.Database(this.Database);
                IO.Session session = database.Login(this.Username, this.Password);

                // Create Base Item
                IO.Request request = session.Request(IO.Request.Operations.ApplyItem);
                IO.Item query = request.NewItem(filteritemtype.Name, "get");
                query.Select = this.ItemType(filteritemtype.Name).Select;

                // Load Relationships
                this.AddRelationshipTypes(request, query, filteritemtype);

                // Reqest Items
                IO.Response response = request.Execute();

                // Add Items to Cache
                foreach (IO.Item ioitem in response.Items)
                {
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(ioitem.GetString());
                    Item item = new Item(this, document);
                }
            }
        }

        public override void Save()
        {

        }

        private Dictionary<String, ItemType> ItemTypesCache;

        private IEnumerable<ItemType> ItemTypes
        {
            get
            {
                return this.ItemTypesCache.Values;
            }
        }

        private ItemType ItemType(String Name)
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

        private void LoadItemTypes()
        {
            this.ItemTypesCache = new Dictionary<String, Managers.ItemType>();

            // Connect to Server
            IO.Server server = new IO.Server(this.URL);
            IO.Database database = server.Database(this.Database);
            IO.Session session = database.Login(this.Username, this.Password);

            IO.Request request = session.Request(IO.Request.Operations.ApplyItem);
            IO.Item item = request.NewItem("ItemType", "get");
            item.Select = "name,is_relationship";
            IO.Item relitem = new IO.Item("Property", "get");
            relitem.Select = "name,data_type";
            item.AddRelationship(relitem);

            IO.Response response = request.Execute();

            foreach (IO.Item ioitem in response.Items)
            {
                ItemType itemtype = new ItemType(ioitem.GetProperty("name"), "1".Equals(ioitem.GetProperty("is_relationship")));
                this.ItemTypesCache[itemtype.Name] = itemtype;

                foreach (IO.Item ioprop in ioitem.Relationships)
                {
                    String name = ioprop.GetProperty("name");

                    if (!this.Filter.SystemProperties.Contains(name))
                    {
                        itemtype.AddProperty(name, ioprop.GetProperty("data_type"));
                    }
                }
            }
        }

        internal Server(Configuration.Session Configuration, XmlNode Settings)
            :base(Configuration, Settings)
        {
            this.LoadItemTypes();
        }
    }

    internal class ItemType : IEquatable<ItemType>
    {
        internal String Name { get; private set; }

        internal Boolean IsRelationship { get; private set; }

        private Dictionary<String, Property> PropertyCache;

        internal void AddProperty(String Name, String DataType)
        {
            if (!this.PropertyCache.ContainsKey(Name))
            {
                this.PropertyCache[Name] = new Property(Name, DataType);
            }
        }

        internal IEnumerable<Property> Properties
        {
            get
            {
                return this.PropertyCache.Values;
            }
        }

        internal Property Property(String Name)
        {
            if (this.PropertyCache.ContainsKey(Name))
            {
                return this.PropertyCache[Name];
            }
            else
            {
                throw new ArgumentException("Invalid Property Name");
            }
        }

        private String _select;
        internal String Select
        {
            get
            {
                if (this._select == null)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (Property prop in this.Properties)
                    {
                        sb.Append(prop.Name);

                        if (prop.DataType == "item")
                        {
                            sb.Append("(id)");
                        }

                        sb.Append(",");
                    }

                    if (this.IsRelationship)
                    {
                        sb.Append("source_id(id),related_id(id),id");
                    }
                    else
                    {
                        sb.Append("id");
                    }

                    this._select = sb.ToString();
                }

                return this._select;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public Boolean Equals(ItemType other)
        {
            if (other == null)
            {
                return false;
            }
            else
            {
                return this.Name.Equals(other.Name);
            }
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) && !(obj is ItemType))
            {
                return false;
            }
            else
            {
                return this.Equals((ItemType)obj);
            }
        }

        internal ItemType(String Name, Boolean IsRelationship)
        {
            this.PropertyCache = new Dictionary<String, Property>();
            this.Name = Name;
            this.IsRelationship = IsRelationship;
        }
    }

    internal class Property
    {
        internal String Name { get; private set; }

        internal String DataType { get; private set; }

        public override string ToString()
        {
            return this.Name;
        }

        internal Property(String Name, String DataType)
        {
            this.Name = Name;
            this.DataType = DataType;
        }
    }
}
