/////////////////////////////////////////////////////////////////////////
// Server.cs - CommService server                                      //
// ver 2.4                                                             //
// Application: Demonstration for CSE687-SMA, Project#4                //
// Author:      Young Kyu Kim, Syracuse University                     //
//              (315) 870-8906, ykim127@syr.edu                        //
// Source:      Jim Fawcett, CST 4-187, Syracuse University            //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - This server now receives and then sends back received messages.
 */
/*
 * Plans:
 * - Add message decoding and NoSqlDb calls in performanceServiceAction.
 * - Provide requirements testing in requirementsServiceAction, perhaps
 *   used in a console client application separate from Performance 
 *   Testing GUI.
 */
/*
 * Maintenance History:
 * --------------------
 * ver 2.4 : 22 Nov, 2015
 * - finished implementing server related stuff for Project #4
 * ver 2.3 : 29 Oct 2015
 * - added handling of special messages: 
 *   "connection start message", "done", "closeServer"
 * ver 2.2 : 25 Oct 2015
 * - minor changes to display
 * ver 2.1 : 24 Oct 2015
 * - added Sender so Server can echo back messages it receives
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 2.0 : 20 Oct 2015
 * - Defined Receiver and used that to replace almost all of the
 *   original Server's functionality.
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4Starter
{
    using Util = Utilities;

    class Server
    {
        string address { get; set; } = "localhost";
        string port { get; set; } = "8080";

        static DBEngine<int, DBElement<int, string>> dbEngine = new DBEngine<int, DBElement<int, string>>();         //unpersisted database
        static PersistEngine<int, string> persistEngine = new PersistEngine<int, string>();                    //persist engine for creating permanent xml file
        static QueryEngine<int, string> queryEngine = new QueryEngine<int, string>();
        static DBElement<int, string> tempElem;

        static Dictionary<int, DBElement<int, string>> output = new Dictionary<int, DBElement<int, string>>();

        //----< quick way to grab ports and addresses from commandline >-----
        public void ProcessCommandLine(string[] args)
        {
            if (args.Length > 0)
            {
                port = args[0];
            }
            if (args.Length > 1)
            {
                address = args[1];
            }
        }

        //Returns new dbElement based on message content
        static public DBElement<int, string> createDBElement(Message msg)
        {
            DBElement<int, string> elem = new DBElement<int, string>();
            elem.name = msg.name;
            elem.descr = msg.descr;
            elem.timeStamp = msg.timestamp1;
            elem.children = msg.children;
            elem.payload = msg.payload;
            return elem;
        }

        //Sets up augmentation for db
        static void prepareAugment(DBEngine<int, DBElement<int,string>> dbInt)
        {
            //Insert Elements to prepare for Augmentation
            DBElement<int, string> elemAug = new DBElement<int, string>();
            elemAug.name = "element7";
            elemAug.descr = "extra element for augmentation";
            elemAug.timeStamp = DateTime.Now;
            elemAug.children.AddRange(new List<int> { 9, 13 });
            elemAug.payload = "elem7's payload";
            dbInt.insert(7, elemAug);
            DBElement<int, string> elemAug2 = new DBElement<int, string>();
            elemAug2.name = "element7";
            elemAug2.descr = "blahblahblah";
            elemAug2.timeStamp = DateTime.Now;
            elemAug2.children.AddRange(new List<int> { 77, 99 });
            elemAug2.payload = "huehuehuehuehue";
            dbInt.insert(9, elemAug2);
        }

        //Performs operation based on message's data
        static void processMessage(ref Message msg, Sender sndr)
        {
            if(msg.mode != "query")
                Console.WriteLine("\nPerforming XML " + msg.mode);
            else
                Console.WriteLine("\nPerforming " + msg.query);
                
            switch (msg.mode)
            {
                case "add":
                    dbEngine.insert(msg.key, createDBElement(msg));       //add element to db
                    break;
                case "delete":
                    if (dbEngine.remove(msg.key))
                        Console.WriteLine("Successfully deleted key" + msg.key);
                    else
                        Console.WriteLine("Failed to delete key " + msg.key);
                    break;
                case "update":
                    dbEngine.getValue(msg.key, out tempElem);
                    dbEngine.editName<string>(msg.key, msg.name);
                    dbEngine.editDescr<string>(msg.key, msg.descr);
                    dbEngine.editTimeStamp<string>(msg.key, DateTime.Now);
                    dbEngine.editChildren<string>(msg.key, msg.children);
                    dbEngine.editPayload<string>(msg.key, msg.payload);
                    break;
                case "persist":
                    persistEngine.persistDB(dbEngine);
                    break;
                case "restore":
                    persistEngine.unpersistDB(dbEngine);
                    break;
                case "augment":
                    prepareAugment(dbEngine);
                    persistEngine.augmentDB(dbEngine);
                    break;
                case "query":
                    processQuery(msg, sndr);
                    break;
                default:
                    break;
            }
        }

        //Messages sent with msg.mode == query, is processed here
        static void processQuery(Message msg, Sender sndr)
        {
            string response = "";

            if(msg.query == "valueOfKey")
            {
                response = generateQueryResponse(dbEngine, queryEngine.searchValue(dbEngine, msg.key), msg.key.ToString(), "");
            }
            if (msg.query == "childrenOfKey")
            {
                response = generateQueryResponse(dbEngine, queryEngine.searchChildren(dbEngine, msg.key), msg.key.ToString(), "");
            }
                
            if (msg.query == "patternSearch")
            {
                if(msg.content == null)
                {
                    //Console.WriteLine("Its NUUULLLL");
                }
                else
                {
                    //Console.WriteLine(msg.content);
                    response = generateQueryResponse(dbEngine, queryEngine.searchKeyPattern(dbEngine, msg.content), msg.content, "");
                }
            }
            
            if (msg.query == "stringSearch")
            {
                response = generateQueryResponse(dbEngine, queryEngine.searchMetadata(dbEngine, msg.content), msg.content, "");
            }
                
            if (msg.query == "timeSearch")
            {
                response = generateQueryResponse(dbEngine, queryEngine.searchTimeInterval(dbEngine, msg.timestamp1, msg.timestamp2), msg.timestamp1.ToString(), msg.timestamp2.ToString());
            }

            //Console.WriteLine("Done processing query");
            sendResponse(msg, sndr, response);

            //send final message to stop timer
            if (msg.isLast == true)
            {
                Message closeMsg = new Message();
                closeMsg.response = "done";
                closeMsg.query = msg.query;
                closeMsg.toUrl = msg.fromUrl;
                closeMsg.fromUrl = msg.toUrl;
                sndr.sendMessage(closeMsg);
            } 
        }

        //----< show query results >---------------------------------------
       static public string generateQueryResponse(DBEngine<int, DBElement<int, string>> db, Dictionary<int, DBElement<int, string>> keyDictionary, string queryParam, string queryParam2)
       {
            string response = "";
            if (keyDictionary.Count() != 0) // query succeeded for at least one key
            {
                //Console.WriteLine("keyDictionary size : " + keyDictionary.Count());
                if (queryParam2 == "")                             //Response for every query except time search
                {
                    foreach (KeyValuePair<int, DBElement<int, string>> pair in keyDictionary)
                    {
                        response = "found " + queryParam + " in " + pair.Value.name + " at key" + pair.Key;
                    }
                    return response + "\n";
                }
                else                                               //Response for timer interval search
                {
                    foreach (KeyValuePair<int, DBElement<int, string>> pair in keyDictionary)
                    {
                        response = "found elements between " + queryParam + " and " + queryParam2 + " at key " + pair.Key;
                    }

                    return response + "\n";
                }
           }
           else                                                    //response for failed queries
           {
                response = "query failed with queryParam " + queryParam;
                return response + "\n";
           }

            return response+"\n";
       }

        //creates and sends response back to clients
        static void sendResponse(Message msg, Sender sndr, string response)
        {
            //Console.WriteLine("response : " + response);
            //Util.swapUrls(ref msg);
            Message responseMsg = new Message();
            responseMsg.response = response;
            responseMsg.key = msg.key;
            responseMsg.query = msg.query;
            responseMsg.content = msg.content;
            responseMsg.timestamp1 = msg.timestamp1;
            responseMsg.timestamp2 = msg.timestamp2;
            responseMsg.toUrl = msg.fromUrl;
            responseMsg.fromUrl = msg.toUrl;
            sndr.sendMessage(responseMsg);
        }

        static void Main(string[] args)
        {
            DBElement<int, string> newElement = new DBElement<int, string>();
            //DBElement<int, string> tempElem;                                      //used for update

            Util.verbose = false;
            Server srvr = new Server();
            srvr.ProcessCommandLine(args);

            Console.Title = "Server";
            Console.Write(String.Format("\n  Starting CommService server listening on port {0}", srvr.port));
            Console.Write("\n ====================================================\n");

            Sender sndr = new Sender(Util.makeUrl(srvr.address, srvr.port));
            //Sender sndr = new Sender();
            Receiver rcvr = new Receiver(srvr.port, srvr.address);

                // - serviceAction defines what the server does with received messages
                // - This serviceAction just announces incoming messages and echos them
                //   back to the sender.  
                // - Note that demonstrates sender routing works if you run more than
                //   one client.

            DBElement<int, string> elem = new DBElement<int, string>();

            Action serviceAction = () =>
            {
                Message msg = null;
                while (true)
                {
                    msg = rcvr.getMessage();   // note use of non-service method to deQ messages
                    Console.Write("\n  Received message:");
                    Console.Write("\n  sender is {0}", msg.fromUrl);
                    //Console.Write("\n  content is {0}\n", msg.content);

                    if (msg.content == "connection start message")
                    {
                        continue; // don't send back start message
                    }
                    if (msg.content == "done")
                    {
                        Console.Write("\n  client has finished\n");
                        continue;
                    }
                    if (msg.content == "closeServer")
                    {
                        Console.Write("received closeServer");
                        break;
                    }

                    //Project 4 switch statements
                    processMessage(ref msg, sndr);

                    msg.content = "received " + msg.content + " from " + msg.fromUrl;

                    // swap urls for outgoing message
                    Util.swapUrls(ref msg);

                #if (TEST_WPFCLIENT)
                    /////////////////////////////////////////////////
                    // The statements below support testing the
                    // WpfClient as it receives a stream of messages
                    // - for each message received the Server
                    //   sends back 1000 messages
                    //
                    /*
                    int count = 0;
                    for (int i = 0; i < 0; ++i)
                    {
                        Message testMsg = new Message();
                        testMsg.toUrl = msg.toUrl;
                        testMsg.fromUrl = msg.fromUrl;
                        testMsg.content = String.Format("test message #{0}", ++count);
                        Console.Write("\n  sending testMsg: {0}", testMsg.content);
                        sndr.sendMessage(testMsg);
                    }
                    */
                #else
                    /////////////////////////////////////////////////
                    // Use the statement below for normal operation
                    sndr.sendMessage(msg);
                #endif
                    }
                };

            if (rcvr.StartService())
            {
                rcvr.doService(serviceAction); // This serviceAction is asynchronous,
            }                                // so the call doesn't block.
            Util.waitForUser(); 
        }
    }
}
