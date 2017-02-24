using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Xml;

namespace WSNTBS
{
    public class TBXMLConfiguration : TBConfiguration
    {
        private readonly string CONFIG_FILE = "WSNTBS.xml";

        public override void LoadConfigure()
        {
            if ( File.Exists(CONFIG_FILE) )
            {
                XmlReader xr = XmlReader.Create(CONFIG_FILE);

                while (xr.Read())
                {
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        switch (xr.Name.ToString())
                        {
                            case "Host":
                                SetDatabaseHost( xr.ReadString() ) ;
                                break;
                            case "Database":
                                SetDatabase( xr.ReadString() );
                                break;
                            case "Username":
                                SetDatabaseUsername( xr.ReadString() );
                                break;
                            case "Password":
                                SetDatabasePassword( xr.ReadString() );
                                break;
                            case "Location":
                                SetRegion(xr.ReadString());
                                break;
                            case "StartOfNode":
                                SetStartNodeID( int.Parse(xr.ReadString())) ;
                                break;
                            case "NumberOfNodes":
                                SetNumOfNodes( int.Parse(xr.ReadString())) ;
                                break;
                            case "MaxProgrammerThreads" :
                                SetMaxProgrammerThreads(int.Parse(xr.ReadString()));
                                break;
                        }

                        if (xr.Name.ToString().IndexOf("Node_") >= 0)
                        {
                            string tmp = xr.ReadString();

                            if (tmp != "Disable")
                                SetCOMPort(int.Parse(xr.Name.ToString().Substring(5)), int.Parse(tmp.Substring(3)));
                            else
                                SetCOMPort(int.Parse(xr.Name.ToString().Substring(5)), 0);
                        }
                    }
                }
                xr.Close();
            }
            else
            {
                LoadDefaultConfigure();
                SaveConfigure();
            }
        }

        public override void SaveConfigure()
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.NewLineHandling = NewLineHandling.Replace;
            xws.NewLineChars = "\r\n";
            xws.Indent = true;
            xws.IndentChars = "\t";
            xws.Encoding = Encoding.UTF8;

            XmlWriter xw = XmlWriter.Create(CONFIG_FILE, xws);

            xw.WriteStartDocument();
            xw.WriteStartElement("WSNTBS");

            xw.WriteStartElement("MySQLServer");
            xw.WriteElementString("Host", GetDatabaseHost() );
            xw.WriteElementString("Database", GetDatabase() );
            xw.WriteElementString("Username", GetDatabaseUsername() );
            xw.WriteElementString("Password", GetDatabasePassword() );
            xw.WriteElementString("Location", GetRegionText(false) );
            xw.WriteElementString("StartOfNode", GetStartNodeID().ToString() );
            xw.WriteElementString("NumberOfNodes", GetNumOfNodes().ToString() );
            xw.WriteElementString("MaxProgrammerThreads", GetMaxProgrammerThreads().ToString());
            xw.WriteEndElement();

            xw.WriteStartElement("PortMapping");
            for (int i = 0; i < GetNumOfNodes() ; ++i)
            {
                int node_id = GetStartNodeID() + i;
                if (GetCOMPort(node_id) != "")
                    xw.WriteElementString("Node_" + (node_id).ToString(), GetCOMPort(node_id));
                else
                    xw.WriteElementString("Node_" + (node_id).ToString(), "Disable");
            }

            xw.WriteEndElement();

            xw.WriteEndDocument();
            xw.Close();
        }
    }
}
