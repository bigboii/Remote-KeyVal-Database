///////////////////////////////////////////////////////////////
// Scheduler.cs - Performs scheduled persists               //
//                 Handles Project Requirement #6            //
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
 * This package schedules a persist operation based on a timer interval.
 * It performs the following operations :
 *    - accepts a positive timer interval (seconds)
 *    - periodically signals to perform "persist operation"
 *
 * The timer will always run in a second increment
 * It will not suppport persist after certain # of writes.
 * 
 * Maintenance :
 * --------------
 * Required Files: Scheduler.cs, and DBEngine.cs
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 02 Oct, 2015
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Project4Starter
{
    public class Scheduler
    {
        public Timer schedular { get; set; } = new Timer();     //Timer that generates event after a set interval
        public int timerInterval = 0;                           //Persist interval in seconds
        public int counter;                                     //Counts the # of intervals generated
        public int maxPersist = 1;                              //Number of persists the scheduler will perform
        public int countPersist = 0;                            //Current # of persists the scheduler signaled
        public bool isPersist = false;

        //Initial Constructor
        public Scheduler()
        {
            timerInterval = 1;
            isPersist = false;
            counter = 0;
            schedular.AutoReset = true;                         //
            schedular.Interval = 1000;                          // elapsed function called after 1 second

            schedular.Elapsed += (object source, ElapsedEventArgs e) =>
            {
                Console.Write("\n  an event occurred at {0}", e.SignalTime);
                Console.Write("\n counter : " + counter);
                counter++;                                      //increment interval counter

                //If "timerInterval" amount of seconds have passed, set persistTrue 
                if (counter == timerInterval && countPersist != maxPersist)
                {
                    Console.Write("\nPERSIST\n");
                    isPersist = true;
                }
            };
        }

        //Edit persist interval (seconds)
        public void setTimerInterval(int newInterval)
        {
            timerInterval = newInterval;
            Console.Write("\nTimerInterval : "+ timerInterval);
            schedular.Interval = 1000;
        }

        public void setMaxPersist(int num)
        {
            maxPersist = num;
        }

        //Begin scheduled persist operation
        public void enableTimer()
        {
            schedular.Enabled = true;
            wait();
        }
        
        //Waits until persist interval
        public void wait()
        {
            while (counter != timerInterval)
            {
                //Console.Write("counter : " + counter + "  timerInterval : " + timerInterval);
                //Waiting until set number of intervals have occurred
            }

            countPersist++;
            counter = 0;      //reset counter
        }
        
        //Ends Timer, therefore halting persist operation
        public void disableTimer()
        {
            Console.Write("\nDisabling Timer\n");
            schedular.Enabled = false;
        }

        //Update boolean flag to allow persist operation; referenced by TestExec (requirement #6)
        public bool getIsPersist()
        {
            return isPersist;
        }
    }
}
