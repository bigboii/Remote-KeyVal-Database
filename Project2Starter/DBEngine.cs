///////////////////////////////////////////////////////////////
// DBEngine.cs - define noSQL database                       //
// Ver 1.3                                                   //
// Application: Demonstration for CSE687-OOD, Project#2      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Alienware 17R2, Core-i7, Windows 10          //
// Author:      Young Kyu Kim, Syracuse University           //
//              (315) 870-8906, ykim127@syr.edu              //
// Source:      Jim Fawcett, CST 4-187, Syracuse University  //
//              (315) 443-3948, jfawcett@twcny.rr.com        //
///////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package implements DBEngine<Key, Value> where Value
 * is the DBElement<key, Data> type.
 *
 * This class was initially a starter for the DBEngine package.
 * Various additional functions were implemented to help satisfy 
 * project requirements 3, 4, 5, 6, and 7.
 *
 * In the starter version, it doesn't implement many of the requirements for the db, e.g.,
 * It doesn't remove elements, doesn't persist to XML, doesn't retrieve
 * elements from an XML file, and it doesn't provide hook methods
 * for scheduled persistance.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: DBEngine.cs, DBElement.cs, and
 *                 UtilityExtensions.cs only if you enable the test stub
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.3 :  7 Oct 15
 * - Added various functions that aids in satisfying requirement 3, 4, 5, 6, and 7 for Project 2
 * ver 1.2 : 24 Sep 15
 * - removed extensions methods and tests in test stub
 * - testing is now done in DBEngineTest.cs to avoid circular references
 * ver 1.1 : 15 Sep 15
 * - fixed a casting bug in one of the extension methods
 * ver 1.0 : 08 Sep 15
 * - first release
 *
 */
//todo add placeholders for Shard
//todo add reference to class text XML content

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Project4Starter
{
    public class DBEngine<Key, Value>
    {
        private Dictionary<Key, Value> dbStore;                    //db represented as Dictionary

        private int keyCount;

        public DBEngine()
        {
            dbStore = new Dictionary<Key, Value>();
            keyCount = 0;
        }

        //Insert into DB if key doesn't exist
        public bool insert(Key key, Value val)
        {
            if (dbStore.Keys.Contains(key))                    //return if already exists
            {
                return false;
            }
            else
            {
                dbStore[key] = val;                            //else insert key
                keyCount++;
                return true;
            }
        }

        //Delete FUnction (Requirement 3)
        public bool remove(Key key/*, Value value*/)
        {
            //delete
            if (dbStore.Keys.Contains(key))
            {
                dbStore.Remove(key);
                keyCount--;
                return true;
            }
            else
            {
                return false;
            }
        }

        //Returns value based on key
        public bool getValue(Key key, out Value val)
        {
            if (dbStore.Keys.Contains(key))
            {
                val = dbStore[key];
                return true;
            }

            val = default(Value);
            return false;
        }



        //Returns # of key/value pairs in db
        public int getKeyCount()
        {
            return keyCount;
        }

        public void setKeyCount(int count)
        {
            keyCount = count;
        }

        //IEnumerable<key> is an interface that requires implementers to provide the getEnumerator method.  
        //In the .Net framework most collections implement this interface and so we can return an instance of any of them with keys, 
        //   e.g., a list of keys, an array of keys, etc.
        public IEnumerable<Key> Keys()
        {
            return dbStore.Keys;
        }

        /*
         * More functions to implement here
         */

        //Requirement 4
        //Edit replace already existing instances with new instance pointed by key(REQUIREMENT 4)
        public bool replaceValue(Key key, Value newValue)
        {
            if (dbStore.Keys.Contains(key))
            {
                dbStore[key] = newValue;                            //else insert key
                return true;
            }
            else
            {
                return false;
            }
        }

        public void editName<Data>(Key key, String newName)
        {
            DBElement<Key, Data> element = new DBElement<Key, Data>();
            Value val;
            if (dbStore.Keys.Contains(key))
            {
                getValue(key, out val);             //search for correct element
                element = val as DBElement<Key, Data>;
                element.name = newName;
            }
        }

        public void editDescr<Data>(Key key, String newDescr)
        {
            DBElement<Key, Data> element = new DBElement<Key, Data>();
            Value val;
            getValue(key, out val);             //search for correct element
            if (dbStore.Keys.Contains(key))
            {
                element = val as DBElement<Key, Data>;
                element.descr = newDescr;
            }
        }

        public void editTimeStamp<Data>(Key key, DateTime newTime)
        {
            DBElement<Key, Data> element = new DBElement<Key, Data>();
            Value val;
            getValue(key, out val);             //search for correct element
            if (dbStore.Keys.Contains(key))
            {
                element = val as DBElement<Key, Data>;
                element.timeStamp = newTime;
            }
        }

        public void editChildren<Data>(Key key, List<Key> newChildren)
        {
            DBElement<Key, Data> element = new DBElement<Key, Data>();
            Value val;
            getValue(key, out val);             //search for correct element
            if (dbStore.Keys.Contains(key))
            {
                element = val as DBElement<Key, Data>;
                element.children = newChildren;
            }
        }

        //payload is String??
        public void editPayload<Data>(Key key, Data newPayload)
        {
            DBElement<Key, Data> element = new DBElement<Key, Data>();
            Value val;
            getValue(key, out val);             //search for correct element
            if (dbStore.Keys.Contains(key))
            {
                element = val as DBElement<Key, Data>;
                element.payload = newPayload;
            }
        }

        /* REQUIREMENT #6 */
        //Clears data, resulting in an empty dictionary
        public void clear()
        {
            dbStore.Clear();
        }

        //Requirement #7 : QUERY ENGINE
        public bool checkKey(Key key)
        {
            if (dbStore.ContainsKey(key))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public Dictionary<Key, Value> getData()
        {
            return dbStore;
        }


    }



#if(TEST_DBENGINE)

  class TestDBEngine
  {
    static void Main(string[] args)
    {
      "Testing DBEngine Package".title('=');
      WriteLine();

      Write("\n  All testing of DBEngine class moved to DBEngineTest package.");
      Write("\n  This allow use of DBExtensions package without circular dependencies.");

      Write("\n\n");
    }
  }
#endif
}
