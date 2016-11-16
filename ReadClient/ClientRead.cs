/////////////////////////////////////////////////////////////////////////
// ClientRead.cs - CommService client sends and receives messages      //
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
 * - finished implementing query operations
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace Project4Starter
{
  using Util = Utilities;

  ///////////////////////////////////////////////////////////////////////
  // Client class sends and receives messages in this version
  // - commandline format: /L http://localhost:8085/CommService 
  //                       /R http://localhost:8080/CommService
  //   Either one or both may be ommitted

  public class ClientRead
  {
    string localUrl { get; set; } = "http://localhost:8082/CommService";
    string remoteUrl { get; set; } = "http://localhost:8080/CommService";

        static HiResTimer timer_read = new HiResTimer();

    //----< retrieve urls from the CommandLine if there are any >--------

    public void processCommandLine(string[] args)
    {
      if (args.Length == 0)
        return;
      localUrl = Util.processCommandLineForLocal(args, localUrl);
      remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
    }

        //Project 4 Read Client Implementation
        //read xml containing stresstest
        static void readXmlInput(ref Message msg, Sender sndr)
        {
            timer_read.Start();
            string temp_fromUrl = msg.fromUrl;//Keep copy of fromUrl and toUrl
            string temp_toUrl = msg.toUrl;

            int count = 0;
            countTotalRequests(ref count);

            //1. Read stresstest.xml
            XDocument reader;

            String filename = Path.GetFullPath("../../../ReadClient/bin/Debug/stresstest_read.xml");
            reader = XDocument.Load(filename);
            var tests = reader.Root.Elements("testnum");     //get list of testcases

            XElement op = null;
            XElement reps = null;
            int tempCount = 0;
            foreach (var test in tests)
            {
                //Reset Message
                msg = new Message();
                msg.fromUrl = temp_fromUrl;
                msg.toUrl = temp_toUrl;

                //read xml
                XElement nextNode = (XElement)test.NextNode;
                op = nextNode.Element("operation");           //get operation type
                reps = nextNode.Element("reps");              //get # of reps

                //2. Generate Messages based on stress test
                //msg.mode = op.Value;
                //msg.reps = Int32.Parse(reps.Value);

                for (int i = 0; i < Int32.Parse(reps.Value); i++)
                {
                    generateQuery(op.Value, ref msg, i);       //should support multiple op types

                    Console.WriteLine("Sending " + op.Value + "requests");
                    if (tempCount + 1 == count)
                        msg.isLast = true;
                    sndr.sendMessage(msg);
                    Thread.Sleep(100);

                    tempCount++;
                }
            }
        }

        static public void countTotalRequests(ref int count)
        {
            //1. Read stresstest.xml
            XDocument reader;

            String filename = Path.GetFullPath("../../../ReadClient/bin/Debug/stresstest_read.xml");
            reader = XDocument.Load(filename);
            var tests = reader.Root.Elements("testnum");     //get list of testcases

            XElement op = null;
            XElement reps = null;
            foreach (var test in tests)
            {
                //read xml
                XElement nextNode = (XElement)test.NextNode;
                op = nextNode.Element("operation");           //get operation type
                reps = nextNode.Element("reps");              //get # of reps

                count += Int32.Parse(reps.Value);
            }
        }

        //Called by readXMLInput to create message with multiple operations
        static void generateQuery(string op, ref Message msg, int key)
        {
            msg.mode = "query";
            msg.query = op;
            if (op == "valueOfKey")
                msg.key = key; 
            if (op == "childrenOfKey")
                msg.key = key;
            if (op == "patternSearch")
                msg.content = "1";            //defaultly choosed 1 for testing
            if (op == "stringSearch")
                msg.content = "updated";
            if (op == "timeSearch")
            {
                DateTime prev = DateTime.Now;
                DateTime next = DateTime.Now;
                TimeSpan ts = new TimeSpan(24, 00, 0);           //10 hours, 30 min, 0 seconds
                prev = prev.Date - ts;
                next = next.Date + ts;
                msg.timestamp1 = prev;
                msg.timestamp2 = next;
            }
        }

        static void Main(string[] args)
        {
            Console.Write("\n  starting CommService client");
            Console.Write("\n =============================\n");

            Console.Title = "Client " + args[1];                   //display title

            ClientRead clnt = new ClientRead();
            clnt.processCommandLine(args);

            string localPort = Util.urlPort(clnt.localUrl);
            string localAddr = Util.urlAddress(clnt.localUrl);
            Receiver rcvr = new Receiver(localPort, localAddr);
            if (rcvr.StartService())
            {
                rcvr.doService(rcvr.defaultServiceAction());            //Start listening for responses
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

            int numMsgs = 5;
            int counter = 0;

            readXmlInput(ref msg, sndr);

            //Close Client
            msg.content = "done";
            sndr.sendMessage(msg);

            //Wait for "done" message from server
            while (rcvr.isDone == false)
            {

            }

            //disable timer and display
            timer_read.Stop();
            Console.WriteLine("\n--- Read test complete --- \nElapsed Time : " + timer_read.ElapsedMicroseconds + "usec");

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

