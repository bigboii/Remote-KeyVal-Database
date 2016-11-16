///////////////////////////////////////////////////////////////
// QueryEngine.cs - define noSQL database                    //
//                  Package used to fulfill requirement #7   //
// Ver 1.0                                                   //
// Application: Demonstration for CSE687-OOD, Project#2      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Alienware 17R2, Core-i7, Windows 10          //
// Author:      Young Kyu Kim, Syracuse University           //
//              (315) 870-8906, ykim127@syr.edu              //
///////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package implements Query handling and execution.
 * It supports queries for the following :
 *      - The value of a specified key
 *      - The children of a specified key
 *      - The set of all keys matching a specifed pattern which defaults to all keys
 *      - All keys that contain a specified string in their metadata section
 *      - All keys that contain values written within a specified time-date interval.
 *           + If only one end of the interval is provided, it shall take the present as 
 *             the other end of the interval 
 * 
 * Each Query functions will return a dictionary containing key-value pairs,
 * which will be stored in DBFactory to support Requirement #8,
 *
 * Lambda Queries are based on Dr.Fawcett's PredicateLambda Demo.
 *
 * Maintenance:
 * ------------
 * Required Files: DBEngine.cs, DBElement.cs, 
                   DBExtension.cs, and Display.cs : used for displaying query output
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 06 Oct, 2015
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//MUST INCLUDE REFERENCE TO DBExtensions and Display for displaying

namespace Project4Starter
{
    public class QueryEngine<Key, Data>
    {
        //Initial Constructor
        public QueryEngine()
        {

        }

        //----< build queryPredicate >-------------------------------------
        /*
         * Query returns true if query(key) condition is satisfied.
         * In this example the query is checking to see if the query
         * parameter, the captured string, test, is a substring of
         * the database element referenced by key.
         */

        //Generates a predicate query for metadata search queries
        public Func<Key, bool> defineSearchQuery(DBEngine<Key, DBElement<Key, Data>> db,  string test)
       {
           // Func<int, bool> is delegate that binds to 
           // functions of the form bool query(int key).

           Func<Key, bool> searchQueryPredicate = (Key key) =>
           {
               DBElement<Key, Data> elem;
               db.getValue((Key)Convert.ChangeType(key, typeof(Key)), out elem);

               //check name
               if(elem.name == test)  //check if key exists
                   return true; 

               //check descr
               if(elem.descr == test)
                   return true;

               //check childrens
               foreach(Key someKey in elem.children)
               {
                   if(someKey.ToString() == test)
                   {
                       return true;
                   }
               }
               //check payload
               if(typeof(Data) == typeof(string))
               {
                   if (elem.payload as string == test)
                   {
                       return true;
                   }
               }
               else
               {
                   foreach (var somePayload in elem.payload as List<string>)
                   {
                       if (somePayload == test)
                           return true;
                   }
               }

               return false;
           };
           return searchQueryPredicate;
       }

        //Generates a predicate query for time interval queries
        public Func<Key, bool> defineTimeQuery(DBEngine<Key, DBElement<Key, Data>> db, DateTime before, DateTime after)
        {
            Func<Key, bool> timeQueryPredicate = (Key key) =>
            {
                DBElement<Key, Data> elem;
                db.getValue((Key)Convert.ChangeType(key, typeof(Key)), out elem);

                //check name
                if (elem.timeStamp <= after && elem.timeStamp >= before) //if between before and after
                {
                    //Console.Write("\nTIME HERE\n");
                    return true;
                }
                return false;
            };
            return timeQueryPredicate;
        }

        //----< process query using queryPredicate >-----------------------

        //Handles metadata search queries
        public bool processSearchQuery(DBEngine<Key, DBElement<Key, Data>> db, Func<Key, bool> searchQueryPredicate, out Dictionary<Key, DBElement<Key, Data>> keyDictionary/*List<Key> keyCollection*/)
        {
            //step through all the keys in the db to see if
            //the queryPredicate is true for one or more keys.

            keyDictionary = new Dictionary<Key, DBElement<Key, Data>>();

            //My code
            IEnumerable<Key> keyList = db.Keys();            //Get list of Keys

            foreach (Key someKey in keyList)
            {
                //Console.Write("testing key : " + someKey + "\n");  //DEBUGGING
                if (searchQueryPredicate(someKey/*(Key)Convert.ChangeType(someKey, typeof(Key)))*/))
                {
                    DBElement<Key, Data> elem;
                    db.getValue((Key)Convert.ChangeType(someKey, typeof(Key)), out elem);

                    keyDictionary[someKey] = (DBElement<Key, Data>)Convert.ChangeType(elem, typeof(DBElement<Key,Data>));
                }
            }
            if (/*keyCollection.Count()*/ keyDictionary.Count > 0)
            { 
                return true;
            }
           return false;
       }

        //Handles time interval queries
        public bool processTimeQuery(DBEngine<Key, DBElement<Key, Data>> db, Func<Key, bool> timeQueryPredicate, out Dictionary<Key, DBElement<Key, Data>> keyDictionary)
        {
            keyDictionary = new Dictionary<Key, DBElement<Key, Data>>();

            IEnumerable<Key> keyList = db.Keys();

            foreach (Key someKey in keyList)
            {
                if (timeQueryPredicate(someKey/*(Key)Convert.ChangeType(someKey, typeof(Key)))*/))
                {
                    DBElement<Key, Data> elem;
                    db.getValue((Key)Convert.ChangeType(someKey, typeof(Key)), out elem);

                    keyDictionary[someKey] = (DBElement<Key, Data>)Convert.ChangeType(elem, typeof(DBElement<Key, Data>));
                }
            }
            if (keyDictionary.Count > 0)
            {
                return true;
            }

            return false;
        }

       //----< show query results >---------------------------------------
       public void showQueryResults(DBEngine<Key, DBElement<Key, Data>> db, bool result, Dictionary<Key, DBElement<Key, Data>> keyDictionary , string queryParam, string queryParam2)
       {
            if (result) // query succeeded for at least one key
            {
                if (queryParam2 == "")        //Display Time Interval?
                {
                    foreach (KeyValuePair<Key, DBElement<Key, Data>> pair in keyDictionary)
                    {
                        //Console.Write("\n  found \"{0}\" in \"{1}\" at key {2}", queryParam, pair.Value.name, pair.Key);
                    }
                }
                else
                {
                    foreach (KeyValuePair<Key, DBElement<Key, Data>> pair in keyDictionary)
                    {
                        //Console.Write("\n  found elements between \"{0}\" and \"{1}\" at key {2}", queryParam, queryParam2, pair.Key);
                    }
                }
           }
           else
           {
               //Console.Write("\n  query failed with queryParam \"{0}\"", queryParam);
           }

            Console.WriteLine();
       }
       

        //1. Search for value of a specified key
        public Dictionary<Key, DBElement<Key, Data>> searchValue(DBEngine<Key, DBElement<Key, Data>> db, Key key)
        {
            Dictionary<Key, DBElement<Key, Data>> dictionary = new Dictionary<Key, DBElement<Key, Data>>();
            //Check if key exists
            if (db.checkKey(key))
            {
                DBElement<Key, Data> elem;
                db.getValue(key, out elem);

                elem.showElement();
                Console.Write("\n");

                //return
                //Dictionary<Key, DBElement<Key, Data>> dictionary = new Dictionary<Key, DBElement<Key, Data>>();
                dictionary[key] = elem;

                return dictionary;
            }
            //Key found
            else
            {
                //Console.Write("Key " + key + " not found...\n");
            }


            //return default(Dictionary<Key, DBElement<Key, Data>>);
            return dictionary;
        }

        //2. The children of specified key
        public Dictionary<Key, DBElement<Key, Data>> /*DBFactory<Key, Value> */ searchChildren(DBEngine<Key, DBElement<Key, Data>> db, Key key)
        {
            Dictionary<Key, DBElement<Key, Data>> dictionary = new Dictionary<Key, DBElement<Key, Data>>();
            //Check if key exists
            if (db.checkKey(key))
            {
                DBElement<Key, Data> elem;
                db.getValue(key, out elem);

                //Dictionary<Key, DBElement<Key, Data>> dictionary = new Dictionary<Key, DBElement<Key, Data>>();
                dictionary[key] = elem;

                //Console.Write("Children of key " + key + " : \n");
                foreach (Key child in elem.children)
                {
                    Console.WriteLine("\t- "+ child);
                }

                Console.Write("\n");
            }
            //Key found
            else
            {
                //Console.Write("Key " + key + " not found...\n");
            }

            //return default(Dictionary<Key, DBElement<Key, Data>>);
            return dictionary;
        }

        //3. Set of all keys matching a specified pattern which defaults to all keys
        public Dictionary<Key, DBElement<Key, Data>> searchKeyPattern(DBEngine<Key, DBElement<Key, Data>> db, string strPattern)
        {
            Dictionary<Key, DBElement<Key, Data>> keyDictionary = new Dictionary<Key, DBElement<Key, Data>>();

            //List<Key> keyCollection = new List<Key>();
            IEnumerable<Key> keyList = db.Keys();

            foreach (Key someKey in keyList)
            {
                DBElement<Key, Data> elem;
                db.getValue(someKey, out elem);

                //Search pattern
                if (strPattern == "")       //no pattern
                {
                    //keyCollection.Add((Key)Convert.ChangeType(someKey, typeof(Key)));
                    keyDictionary[someKey] = elem;

                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(someKey.ToString(), strPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    keyDictionary[someKey] = elem;
                    //System.Console.WriteLine("  (match for '{0}' found)", strPattern);
                }
                else
                {
                    //System.Console.WriteLine(/*"Checking " + someKey + */"  (match for '{0}' NOT found...)", strPattern);
                }
            }

            return keyDictionary;

        }

        //4. All keys that contain a specified string in their metadata
        public Dictionary<Key, DBElement<Key, Data>> searchMetadata(DBEngine<Key, DBElement<Key, Data>> db, string search)
        {
            // build searchQuery
            Func<Key, bool> query = defineSearchQuery(db, search);

            // process searchQuery
            //List<Key> keyCollection;

            Dictionary<Key, DBElement<Key, Data>> keyDictionary;
            bool result = processSearchQuery(db, query, out keyDictionary);

            //Display Results
            //showQueryResults(db, result, keyDictionary, search, "");

            return keyDictionary;
        }

        //5. Search Keys where values written within a specified time-date interval
        public Dictionary<Key, DBElement<Key, Data>> searchTimeInterval(DBEngine<Key, DBElement<Key, Data>> db, DateTime initInterval, DateTime endInterval)
        {
            //Display Interval
            //Console.Write("\nSearch Interval is :");
            //Console.Write("\n\tInit : " + initInterval + "\n");
            //Console.Write("\tEnd : " + endInterval + "\n");

            //Check if both DateTime has been supplied
            if (initInterval == default(DateTime))
            {
                initInterval = DateTime.Now;            //set present as the other end of the interval
            }
            if(endInterval == default(DateTime))
            {
                endInterval = DateTime.Now;             //set present as the other end of the interval
            }

            //build timeQuery
            Func<Key, bool> query = defineTimeQuery(db, initInterval, endInterval);

            // process timeQuery
            List<Key> keyCollection;
            Dictionary<Key, DBElement<Key, Data>> keyDictionary;

            bool result = processTimeQuery(db, query, out keyDictionary);

            //Display Results
            //showQueryResults(db, result, keyDictionary, initInterval.ToString(), endInterval.ToString());

            return keyDictionary;
        }

    }
}
