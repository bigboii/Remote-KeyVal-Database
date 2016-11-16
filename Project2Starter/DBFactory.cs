///////////////////////////////////////////////////////////////
// DBFactory.cs - define noSQL immutable database            //
//                  which holds query output                 //
//                Used to fullfill requirement # 8           //
// Ver 1.0                                                   //
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
 * This package represents an instance of an immutable database constructed from
 * the result of a query. This package is similar with DBEngine in that it holds Key-Value pairs 
 * However, this immutable database does not support inserts, deletes, edits, and other functions.
 * No functions can access them, thus supporting the immutability property.
 * 
 * The package is used to demonstrate Requirement # 8
 */
/*
* Maintenance:
* ------------
* Required Files: DBFactory.cs, and DBElement.cs
*
* Maintenance History:
* --------------------
* ver 1.0 : 06 Oct 15
* - first release
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4Starter
{
    public class DBFactory<Key, Value>
    {
        private Dictionary<Key, Value> dbImmutable;            //private, supporting immutability

        //Initial Constructor
        public DBFactory(Dictionary<Key, Value> newDB)
        {
            dbImmutable = newDB;
        }

        public IEnumerable<Key> Keys()
        {
            return dbImmutable.Keys;
        }

        //Returns value based on key; 
        //   "passed by value", thus supporting immutability
        public Value getValue(Key key)
        {
            Value val;
            if (dbImmutable.Keys.Contains(key))
            {
                val = dbImmutable[key];

                return val;
            }

            return default(Value);
        }
    }
}
