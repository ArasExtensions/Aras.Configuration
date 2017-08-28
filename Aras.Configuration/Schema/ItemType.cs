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

namespace Aras.Configuration.Schema
{
    public class ItemType : IEquatable<ItemType>
    {
        public String Name { get; private set; }

        public Boolean IsRelationship { get; private set; }

        private Dictionary<String, Property> PropertyCache;

        internal void AddProperty(String Name, String DataType)
        {
            if (!this.PropertyCache.ContainsKey(Name))
            {
                this.PropertyCache[Name] = new Property(Name, DataType);
            }
        }

        public IEnumerable<Property> Properties
        {
            get
            {
                return this.PropertyCache.Values;
            }
        }

        public Property Property(String Name)
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
        public String Select
        {
            get
            {
                if (this._select == null)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach(Property prop in this.Properties)
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
}
