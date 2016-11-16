/////////////////////////////////////////////////////////////////////////
// Client1.cs - Test Requirements for Project #4                       //
// ver 2.1                                                             //
// Application: Demonstration for CSE687-SMA, Project#4                //
// Author:      Young Kyu Kim, Syracuse University                     //
//              (315) 870-8906, ykim127@syr.edu                        //
// Source:      Jim Fawcett, CST 4-187, Syracuse University            //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package begins the demonstration of meeting requirements.
 * It will demonstrate project requirements 2, 3, 4, 5, 6, 7.
 */
/*
* Maintenance:
* ------------
* Required Files: 
*   TestExec.cs,  Client1.cs, Client2.cs, Server.cs, Project2Starter cs files
*
* Maintenance History:
* --------------------
* ver 1.0 : 22 Nov 15
* - first release
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Project4Starter
{
    public class TestExec
    {
        static int num_write = 1;         //# of write clients to start
        static string input = "";         //boolean flag determining verbose mode for writeclient
        static int num_read = 1;          //# of read clients to start
        static string isVerbose;


        //testing project 2 implementation + WCF
        void TestR234()
        {
            "Explaining Requirement #2, #3, and #4".title();
            "Requirement #2".title();
            Console.WriteLine("\n   The project implements my own noSQL database from project 2, not Dr. Fawcett's solution. The package that contains my db is \"Project2Starter\"");
            Console.WriteLine("   Requirement 2 can be checked by checking the Project2Starter package in the solutions");

            "Requirement #3".title();
            Console.WriteLine("\n   WCF is used to communicate messages between clients. You can see this in action in Write and Read Clients");

            "Requirement #4".title();
            Console.WriteLine("\n   The write client will demonstrate add, delete, and update of key value pairs. It will also demonstrate persist and restoring of database.");
            Console.WriteLine("   The read client will demonstrate 5 query operations : \n value of key \n value of children \n pattern Search \n string search \n time search");
            Console.WriteLine("\n   The database will be persisted to \"../../../Server/bin/Debug/PersistDBInt.xml\"");
            Console.WriteLine("   Please check the read and write clients to verify the functionality of Requirement #4");
        }

        //Write Client
        void TestR567()
        {
            "Demonstrating Requirement #4, #5, #6, and #7".title();
            "Requirement #5".title();
            Console.WriteLine("\n   The write clients read xml input from \"../../../WriteClient/bin/Debug/stresstest_write.xml\" which contains various add, update, and delete operations.");

            "Requirement #6".title();
            Console.WriteLine("\n   The read client reads an xml input from \"../../../ReadClient/bin/Debug/stresstest_read.xml\" which contains various query requests");

            "Requirement #7".title();
            Console.WriteLine("\nTo set a specified number of requests, please edit \"../../../Client/bin/Debug/stresstest_read.xml\" file");

            //Collect input from user
            "Project 4 Demonstration Start".title('+');
            Console.WriteLine("\n   For the sake of this demo, DBEngine<int, DBElement<int, string>> will be used\n");
            Console.WriteLine("\n   Please insert the # of write clients to launch (Max 4) : ");
            num_write = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("   Log messages for write client? [y/n] : ");
            input = Console.ReadLine().ToString();
            while (true)
            {
                if (input == "y" || input == "Y" || input == "n" || input == "N")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\n   Please input \"y\" or \"n\" : ");
                    input = Console.ReadLine().ToString();
                }
            }

            Console.WriteLine("   Please insert the # of read clients to launch (Max 4) : ");
            num_read = Convert.ToInt32(Console.ReadLine());
        }

        void startProcesses()
        {
            //Get path of write and read client, and start process
            String path;
            int portnum;
            for (int i = 0; i < num_write; i++)
            {
                portnum = 8085 + i;
                string l = "/L http://localhost:" + portnum + "/CommService";
                string str = l + " /R http://localhost:8080/CommService " + " /V " + input + " ";

                path = Path.GetFullPath("../../../WriteClient/bin/Debug/ClientWrite.exe");      //Write Client
                Process.Start(path, str.ToString());
            }

            for (int i = 0; i < num_read; i++)
            {
                portnum = 8095 + i;
                string l = "/L http://localhost:" + portnum + "/CommService";
                string str = l + " /R http://localhost:8080/CommService";
                path = Path.GetFullPath("../../../ReadClient/bin/Debug/ClientRead.exe"/* + num_read*/);             //Read Client
                Process.Start(path, str.ToString());
            }
        }

        static void Main(string[] args)
        {
            TestExec exec = new TestExec();

            "Demonstrating Project#4 Requirements".title('=');

            //Start Client
            String path = Path.GetFullPath("../../../Server/bin/Debug/Server.exe");
            Process.Start(path);

            exec.TestR234();
            exec.TestR567();
            exec.startProcesses();
        }
    }
}
