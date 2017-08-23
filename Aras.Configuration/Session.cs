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

namespace Aras.Configuration
{
    public class Session
    {
        public Logging.Log Log { get; private set; }

        public Filter.Session Filter { get; private set; }

        public Schema.Manager Source { get; private set; }

        public Schema.Manager Target { get; private set; }

        private Boolean ValidSettings
        {
            get
            {
                if (this.Filter != null && this.Source != null && this.Target != null)
                {
                    return true;
                }
                else 
                { 
                    return false; 
                }
            }
        }

        public void Merge()
        {
            if (this.ValidSettings)
            {
                // Load Source
                this.Source.Load();

                // Load Target
                this.Target.Load();

                //Merge Source into Target
                this.Source.Merge(this.Target);

                // Save Target
                this.Target.Save();
            }
        }

        private Schema.Manager CreateSchemaManager(XmlNode Settings)
        {
            XmlAttribute typeAttribite = Settings.Attributes["type"];

            if (typeAttribite != null)
            {
                switch(typeAttribite.InnerText)
                {
                    case "Server":
                        return new Schema.Managers.Server(this, Settings);
                    case "Directory":
                        return new Schema.Managers.Directory(this, Settings);
                    default:
                        this.Log.Add(Logging.Levels.Error, "Server Schema Manager settings contains an invalid type attribute: " + typeAttribite.InnerText);
                        return null;
                }
            }
            else
            {
                this.Log.Add(Logging.Levels.Error, "Server Schema Manager settings do not contain a type attribute");
                return null;
            }
        }

        private void LoadSettings(FileInfo Settings)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Settings.FullName);

            XmlNode configurationNode = doc.SelectSingleNode("configuration");

            if (configurationNode != null)
            {
                XmlNode filterNode = configurationNode.SelectSingleNode("filter");

                if (filterNode != null)
                {
                    this.Filter = new Filter.Session(filterNode, this.Log);
                }
                else
                {
                    this.Log.Add(Logging.Levels.Error, "Configuration does not contain a filter node");
                }

                XmlNode sourceNode = configurationNode.SelectSingleNode("source");

                if (sourceNode != null)
                {
                    this.Source = this.CreateSchemaManager(sourceNode);          
                }
                else
                {
                    this.Log.Add(Logging.Levels.Error, "Configuration does not contain a source node");
                }

                XmlNode targetNode = configurationNode.SelectSingleNode("target");

                if (targetNode != null)
                {
                    this.Target = this.CreateSchemaManager(targetNode);
                }
                else
                {
                    this.Log.Add(Logging.Levels.Error, "Configuration does not contain a target node");
                }
            }
            else
            {
                this.Log.Add(Logging.Levels.Error, "Configuration does not contain a configuration node");
            }
        }

        public Session(FileInfo Settings, Logging.Log Log)
        {
            this.Log = Log;
            this.LoadSettings(Settings);
        }
    }
}
