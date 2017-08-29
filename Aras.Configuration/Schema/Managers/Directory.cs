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
using System.IO;

namespace Aras.Configuration.Schema.Managers
{
    public class Directory : Manager
    {
        public DirectoryInfo BaseDirectory { get; private set; }

        protected override void LoadSettings(XmlNode Settings)
        {
            XmlNode nameNode = Settings.SelectSingleNode("name");

            if (nameNode != null)
            {
                this.BaseDirectory = new DirectoryInfo(nameNode.InnerText);
            }
            else
            {
                this.Log.Add(Logging.Levels.Error, "Directory Schema settings do not contain a name node");
            }
        }

        protected override void LoadItems()
        {
     
        }

        private FileInfo ItemFilename (Item Item)
        {
            return new FileInfo(this.BaseDirectory.FullName + "\\" + Item.ItemType + "\\" + Item.Key + ".xml");
        }

        public override void Save()
        {
           foreach(String itemtype in this.LoadedItemTypes)
           {
               foreach(Item item in this.LoadedItems(itemtype))
               {
                   FileInfo file = this.ItemFilename(item);

                   switch(item.Action)
                   {
                       case Item.Actions.Add:
                       case Item.Actions.Update:

                           if (!file.Directory.Exists)
                           {
                               file.Directory.Create();
                           }

                           using (XmlTextWriter xmlwriter = new XmlTextWriter(file.FullName, Encoding.UTF8))
                           {
                               xmlwriter.Formatting = Formatting.Indented;
                               item.Document.WriteContentTo(xmlwriter);
                           }

                           break;

                       case Item.Actions.Delete:

                           if (file.Exists)
                           {
                               file.Delete();
                           }

                           break;
                       default:

                           break;
                   }
               }
           }
        }

        internal Directory(Configuration.Session Configuration, XmlNode Settings)
            :base(Configuration, Settings)
        {

        }
    }
}
