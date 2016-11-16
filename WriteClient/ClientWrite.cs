/////////////////////////////////////////////////////////////////////////
// ClientWrite.cs - CommService client sends and receives messages     //
// ver 2.2                                                             //
// Application: Demonstration for CSE687-SMA, Project#4                //
// Author:      Young Kyu Kim, Syracuse University                     //
//              (315) 870-8906, ykim127@syr.edu                        //
// Source:      Jim Fawcett, CST 4-187, Syracuse University            //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added using System.Threading
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - in this incantation the client has Sender and now has Receiver to
 *   retrieve Server echo-back messages.
 * - If you provide command line arguments they should be ordered as:
 *   remotePort, remoteAddress, localPort, localAddress
 */
/*
 * Maintenance History:
 * --------------------
 * ver 2.2 : 22 Nov 2015
 * - finished implementing add/update/delete/persist/and restore operations
 * ver 2.1 : 29 Oct 2015
 * - fixed bug in processCommandLine(...)
 * - added rcvr.shutdown() and sndr.shutDown() 
 * ver 2.0 : 20 Oct 2015
 * - replaced almost all functionality with a Sender instance
 * - added Receiver to retrieve Server echo messages.
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Project4Starter
{
    using Util = Utilities;

    ///////////////////////////////////////////////////////////////////////
    // Client class sends and receives messages in this version
    // - commandline format: /L http://localhost:8085/CommService 
    //                       /R http://localhost:8080/CommService
    //   Either one or both may be ommitted

    public class ClientWrite
    {
        string localUrl { get; set; } = "http://localhost:8081/CommService";
        string remoteUrl { get; set; } = "http://localhost:8080/CommService";

        static HiResTimer timer_write = new HiResTimer();      //High Resolution Timer
        static bool isVerbose = false;

        //----< retrieve urls from the CommandLine if there are any >--------

        public void processCommandLine(string[] args)
        {
            if (args.Length == 0)
                return;
            localUrl = Util.processCommandLineForLocal(args, localUrl);
            remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
        }

        /* Project 4 Implementation */
        //Sets up message's datamembers with database element information
        static public void addElement(ref Message msg, int key)
        {
            msg.mode = "add";
            msg.key = key;
            msg.name = "default name";
            msg.descr = "default descr";
            msg.timestamp1 = DateTime.Now;
            if (msg.children ==  null)
                msg.children = new List<int>();
            msg.children.Clear();
            msg.children.Add(11);
            msg.children.Add(22); 
            msg.payload = "default payload";
        }

        static public void deleteElement(ref Message msg, int key)
        {
            msg.mode = "delete";
            msg.key = key;
        }

        static public void updateElement(ref Message msg, int key)
        {
            msg.mode = "update";
            msg.key = key;
            msg.name = "updated name";
            msg.descr = "updated descr";
            if (msg.children == null)
                msg.children = new List<int>();
            msg.children.Clear();            //reset
            msg.children.Add(123);
            msg.children.Add(456);
            msg.children.Add(789);
            msg.payload = "updated payload";
        }

        //read xml containing stresstest
        static void readXmlInput(ref Message msg, Sender sndr)
        {
            timer_write.Start();                 //start timer
            string temp_fromUrl = msg.fromUrl;//Keep copy of fromUrl and toUrl
            string temp_toUrl = msg.toUrl;

            //1. Read stresstest.xml
            XDocument reader;
            String filename = Path.GetFullPath("../../../WriteClient/bin/Debug/stresstest_write.xml");
            reader = XDocument.Load(filename);
            var tests = reader.Root.Elements("testnum");     //get list of testcases

            XElement op = null;
            XElement reps = null;
            foreach(var test in tests)
            {
                //Reset Message
                msg = new Message();
                msg.fromUrl = temp_fromUrl;
                msg.toUrl = temp_toUrl;

                //Read Element
                XElement nextNode = (XElement)test.NextNode;
                op = nextNode.Element("operation");           //get operation type
                reps = nextNode.Element("reps");               //get # of reps

                //2. Generate Messages based on stress test
                //msg.mode = op.Value;
                //msg.reps = Int32.Parse(reps.Value);
                //generateTest(op.Value, ref msg);       //should support multiple op types

                for (int i = 0; i < Int32.Parse(reps.Value); i++)
                {
                    generateTest(op.Value, ref msg, i);       //should support multiple op types

                    //message to be logged to the console
                    if (isVerbose)
                    {
                        Console.WriteLine("Sending " + msg.mode + " key " + msg.key + " request");
                    }
                    sndr.sendMessage(msg);      //send message
                    Thread.Sleep(100);
                }
            }
        }

        //Called by readXMLInput to create message with multiple operations
        static void generateTest(string op, ref Message msg, int key)
        {
            if (op == "add")
                addElement(ref msg, key);
            if (op == "delete")
                deleteElement(ref msg, key);
            if (op == "update")
                updateElement(ref msg, key);
        }

        static void Main(string[] args)
        {
            Console.Write("\n  starting CommService client");
            Console.Write("\n =============================\n");

            Console.Title = "Client " + args[1];

            ClientWrite clnt = new ClientWrite();

            //Requirement #6 : Process Command Line for verbose mode /F
            for (int i = 0; i < args.Length; ++i)
            {
                if ((args.Length > i + 1) && (args[i] == "/v" || args[i] == "/V"))
                {
                    if (args[i+1] == "y" || args[i + 1] == "Y")
                    {
                        isVerbose = true;
                    }
                    if (args[i + 1] == "n" || args[i + 1] == "N")
                    {
                        isVerbose = false;
                    }
                }
            }
            if(isVerbose)
            {
                Console.WriteLine("messsage logging : ON");
            }
            else
            {
                Console.WriteLine("message logging : OFF");
            }

            clnt.processCommandLine(args);

            //setup port and address
            string localPort = Util.urlPort(clnt.localUrl);
            string localAddr = Util.urlAddress(clnt.localUrl);

            //DEBUGGING
            Console.WriteLine("localPort : " + localPort);     //debug
            Console.WriteLine("localAddr : " + localAddr);     //debug

            Receiver rcvr = new Receiver(localPort, localAddr);
            if (rcvr.StartService())
            {
                rcvr.doService(rcvr.defaultServiceAction());       //receiver
            }

            Sender sndr = new Sender(clnt.localUrl);  // Sender needs localUrl for start message

            Message msg = new Message();
            msg.fromUrl = clnt.localUrl;
            msg.toUrl = clnt.remoteUrl;

            Console.Write("\n  sender's url is {0}", msg.fromUrl);
            Console.Write("\n  attempting to connect to {0}\n", msg.toUrl);

            if (!sndr.Connect(msg.toUrl))
            {
            Console.Write("\n  could not connect in {0} attempts", sndr.MaxConnectAttempts);
            sndr.shutdown();
            rcvr.shutDown();
            return;
            }
            
            //Start stress test
            readXmlInput(ref msg, sndr);

            msg.mode = "persist";           //Persist DB
            //msg.reps = 1;
            sndr.sendMessage(msg);
            Thread.Sleep(100);              //main thread sleeps 0.1s

            //Close Client
            msg.content = "done";
            sndr.sendMessage(msg);
            
            //End and display timer
            timer_write.Stop();
            Console.WriteLine("\n--- Write test complete --- \nElapsed Time : " + timer_write.ElapsedMicroseconds + "usec");

            // Wait for user to press a key to quit.
            // Ensures that client has gotten all server replies.
            Util.waitForUser();

            // shut down this client's Receiver and Sender by sending close messages
            rcvr.shutDown();
            sndr.shutdown();

            Console.Write("\n\n");
        }
    }
}
