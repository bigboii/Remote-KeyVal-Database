///////////////////////////////////////////////////////////////
// PersistEngine.cs - Persists database contents to          //
//                    an XML file                            //
//                    Handles Project Requirement #5         //
// Ver 1.0                                                   //
// Application: Demonstration for CSE681-SMA, Project#2      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Alienware 17 R2, Core-i7, Windows 10         //
// Author:      Young Kyu Kim, Syracuse University           //
//              (315) 870-8906, ykim127@syr.edu              //
///////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package handles the persisting of the contents of DBEngine (im-memory) to a permanent XML file.
 * It supports 3 core functionality : 
 *   - Creates an XML version of DBEngine's database, then clears DBEngine
 *   - Process is reversible (DB can be restored from XML to DBEngine)
 *   - Database can be augmented from existing XML file as well as write its contents out to an XML file
 *
 * It is called by Scheduler, PersistEngine, and TestExec packages work together to fullfill requirement #5 and #6.

 * Maintenance :
 *  -------------
 * Required Files: PersistEngine.cs, DBEngine.cs, and DBElement.cs
 *                 UtilityExtensions.cs only if you enable the test stub
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 02 Oct, 2015
 * - first release

 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Project4Starter
{
    public class PersistEngine<Key, Data>
    {
        //Persists entire in-memory Database stored in DBEngine (Int Keys)
        public void persistDB(DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            XDocument xml = new XDocument();            //XML Document
            IEnumerable<Key> keys = dbEngine.Keys();            //Get list of Keys
            XElement db = new XElement("db");
            xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            XComment comment = new XComment("Demonstrating Persist Operation");
            xml.Add(comment);
            xml.Add(db);
            XElement keyTypeElem, payloadTypeElem;                               //Check KeyType
            checkKeyTypes<Key>(out keyTypeElem, out payloadTypeElem);
            db.Add(keyTypeElem);
            db.Add(payloadTypeElem);

            //Check Number of key-value pairs
            if (dbEngine.getKeyCount() > 0)
            {
                foreach (var i in keys)
                {
                    DBElement<Key, Data> element;
                    dbEngine.getValue(i, out element);
                    XElement keyElem = new XElement("key", i);                  //Store MetaData
                    db.Add(keyElem);
                    db.Add(createXmlChild<Key>(element, i));                    //Store remaining metadata
                }

                XElement numPairs = new XElement("keycount", dbEngine.getKeyCount());
                db.Add(numPairs);
                if (typeof(Key) == typeof(int))
                {
                    String filename = Path.GetFullPath("../../../Server/bin/Debug/PersistDBInt.xml");
                    xml.Save(filename);
                }
                else
                    xml.Save("PersistDBString.xml");            // ?? ACCESS DENIED ERROR HERE
            }
            else
            {
                Console.Write("\n Error; void persistDB() : DB Empty\n");       //Debugging
            }
        }
        
        //Check if key type is <int> or <string>, then create xml tags accordingly
        public void checkKeyTypes<Key>(out XElement keyType, out XElement payloadType)
        {
            keyType = new XElement("keytype");
            payloadType = new XElement("payloadType");

            if (typeof(Key) == typeof(int))                    //Check KeyType
                keyType.SetValue("int");
            if (typeof(Key) == typeof(string))
                keyType.SetValue("string");
            if (typeof(Key) == typeof(List<string>))            //Check PayloadType
                payloadType.SetValue("ListOfStrings");
            if (typeof(Key) == typeof(string) || typeof(Key) == typeof(int))
                payloadType.SetValue("string");
        }

        //Create an XElement with metadata; called by persistDB()
        public XElement createXmlChild<Key>(DBElement<Key, Data> element, Key key)
        {
            XElement newElement = new XElement("element");
            XElement metadata1 = new XElement("name", element.name);
            XElement metadata2 = new XElement("descr", element.descr);
            XElement metadata3 = new XElement("timestamp", element.timeStamp);
            XElement metadata4 = new XElement("children");
                               
            foreach (Key val in element.children)                     //Create Children xml list
            {
                XElement elemChildren = new XElement("key", val);
                metadata4.Add(elemChildren);
            }

            XElement metaPayload = new XElement("payload");

            if(typeof(Key) == typeof(string))                         //Create Payload xml list
            {
                foreach (String payload in element.payload as List<String>)
                {
                    XElement elemPayload = new XElement("item", payload);
                    metaPayload.Add(elemPayload);
                }
            }
            if(typeof(Key) == typeof(int))                           //<if> int key, payload is string type
                metaPayload.Value = element.payload as string;

            newElement.Add(metadata1);                               //Add all the metadata into a single <element> node
            newElement.Add(metadata2);
            newElement.Add(metadata3);
            newElement.Add(metadata4);
            newElement.Add(metaPayload);

            return newElement;
        }
        
         //Display Int XML in Console
        public void displayIntXML()
        {
            XDocument reader = XDocument.Load("PersistDBInt.xml");
            Console.Write(reader.ToString());
            Console.Write("\n\n");
        }

        //Display Int XML in Console
        public void displayStrXML()
        {
            XDocument reader = XDocument.Load("PersistDBString.xml");
            Console.Write(reader.ToString());
            Console.Write("\n\n");
        }

        // unpersistDB (Generic Version)
        public void unpersistDB(DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            XDocument reader;                                                 //XML reader
            String fileName = "unnamed";
            dbEngine.clear();                                                 //delete dB
            if (typeof(Key) == typeof(int))                                   //Check which xml to open, INT or STRING
                fileName = "PersistDBInt.xml";
            if (typeof(Key) == typeof(string))
            {   fileName = "PersistDBString.xml";
                if(typeof(Data) == typeof(string))
                    fileName = "packageStructure.xml";
            }
            reader = XDocument.Load(fileName);
            XElement count = reader.Element("db").Element("keycount");                                  //Update Keycount
            dbEngine.setKeyCount(Convert.ToInt32(count.Value));
            var keys = reader.Root.Elements("key");                                                     //Get list of keys
            foreach (var i in keys)                                                                     //Access Element
            {
                XElement nextNode = (XElement)i.NextNode;                                               //get access to element (key -> element)
                DBElement<Key, Data> newElem = new DBElement<Key, Data>();
                newElem.name = nextNode.Element("name").Value;                                          //name
                newElem.descr = nextNode.Element("descr").Value;                                        //descr
                newElem.timeStamp = Convert.ToDateTime(nextNode.Element("timestamp").Value);            //timeStamp
                var childkeys = nextNode.Element("children").Elements("key");                           //children
                foreach (var j in childkeys)
                    newElem.children.Add((Key)Convert.ChangeType(j.Value, typeof(Key)));
                if (typeof(Key) == typeof(int))                                                                           //<if> keyType is int
                {
                    newElem.payload = (Data)Convert.ChangeType(nextNode.Element("payload").Value, typeof(Data));                      //payload
                    dbEngine.insert((Key)Convert.ChangeType(i.Value, typeof(Key)), newElem as DBElement<Key, Data>);                  //Insert
                }
                if (typeof(Key) == typeof(string))
                {
                    if (typeof(Data) == typeof(string))
                    {
                        newElem.payload = (Data)Convert.ChangeType(nextNode.Element("payload").Value, typeof(Data));                          //payload
                        dbEngine.insert((Key)Convert.ChangeType(i.Value, typeof(Key)), newElem as DBElement<Key, Data>);                      //Insert
                    }
                    if (typeof(Data) == typeof(List<string>))
                    {
                        var payloadKeys = nextNode.Element("payload").Elements("item");
                        List<string> list = new List<string>();
                        foreach (string payload in payloadKeys)
                            list.Add(payload);
                        newElem.payload = ((Data)Convert.ChangeType(list, typeof(Data)));
                        dbEngine.insert((Key)Convert.ChangeType(i.Value, typeof(Key)), newElem as DBElement<Key, Data>);                   //Insert
                    }
                }
            }
        }
        
        /*
         *    Augument XML
         */
        public void augmentDB(DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            XDocument reader;                                                //XML reader
            String fileName = "unnamed";
            determineFileName<Key>(out fileName);
            reader = XDocument.Load(fileName);
            XElement count = reader.Element("db").Element("keycount");       //Update Keycount
            dbEngine.setKeyCount(Convert.ToInt32(count.Value));
            var keys = reader.Root.Elements("key");                          //Get list of keys
            foreach (var i in keys)                                                                         //Access Elements
            {
                XElement nextNode = (XElement)i.NextNode;                                                   //get access to element (key -> element)
                DBElement<Key, Data> newElem = new DBElement<Key, Data>();
                newElem.name = nextNode.Element("name").Value;                                              //name
                newElem.descr = nextNode.Element("descr").Value;                                            //descr
                newElem.timeStamp = Convert.ToDateTime(nextNode.Element("timestamp").Value);                //timeStamp
                var childkeys = nextNode.Element("children").Elements("key");                               //children
                foreach (var j in childkeys)
                    newElem.children.Add((Key)Convert.ChangeType(j.Value, typeof(Key)));
                if (typeof(Key) == typeof(int))                                                             //<if> keyType is int
                {
                    newElem.payload = (Data)Convert.ChangeType(nextNode.Element("payload").Value, typeof(Data));                      //payload
                    dbEngine.insert((Key)Convert.ChangeType(i.Value, typeof(Key)), newElem as DBElement<Key, Data>);                  //Insert
                }
                if (typeof(Key) == typeof(string))                                                          //<if> keyType is string
                {
                    if (typeof(Data) == typeof(string))                                                     //<if> keyType is string and dataType is string
                    {
                        newElem.payload = (Data)Convert.ChangeType(nextNode.Element("payload").Value, typeof(Data));             //payload
                        dbEngine.insert((Key)Convert.ChangeType(i.Value, typeof(Key)), newElem as DBElement<Key, Data>);          //Insert
                    }
                    if (typeof(Data) == typeof(List<string>))                                               //<if> keyType is string and dataType is List<string>
                    {
                        var payloadKeys = nextNode.Element("payload").Elements("item");
                        List<string> list = new List<string>();
                        foreach (string payload in payloadKeys)
                            list.Add(payload);
                        newElem.payload = ((Data)Convert.ChangeType(list, typeof(Data)));
                        dbEngine.insert((Key)Convert.ChangeType(i.Value, typeof(Key)), newElem as DBElement<Key, Data>);         //Insert
                    }
                }

                    clearFile<Key>();                          //?? ACCESS DENIED ERROR HERE
                    persistDB(dbEngine);                                     //?? ACCESS DENIED ERROR HERE
                
            }
        }

        public void determineFileName<Key>(out string fileName)
        {
            fileName = "";
            if (typeof(Key) == typeof(int))                                  //Check which xml to open, INT or STRING
                fileName = "PersistDBInt_Aug.xml";
            if (typeof(Key) == typeof(string))
                fileName = "PersistDBString_Aug.xml";
        }

        //Emptys the content of a file based on KeyType
        public void clearFile<Key>()
        {
            string fileName ="";
            if (typeof(Key) == typeof(int))                              //Delete int type XML 
            {
                fileName = "PersistDBInt.xml";                          //?? ACCESS DENIED ERROR HERE
            }
            if (typeof(Key) == typeof(string))                            //Delete string type
            {
                fileName = "PersistDBString.xml";
            }

            FileInfo file = new FileInfo(fileName);
            System.IO.File.WriteAllText(file.ToString(), string.Empty);
        }

    }

    class PersistEngine
    {
        static void Main(string[] args)
        {

        }
    }
}