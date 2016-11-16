/////////////////////////////////////////////////////////////////////////
// ClientWrite.cs - CommService client sends and receives messages     //
// ver 1.2                                                             //
// Application: Demonstration for CSE687-SMA, Project#4                //
// Author:      Young Kyu Kim, Syracuse University                     //
//              (315) 870-8906, ykim127@syr.edu                        //
// Source:      Jim Fawcett, CST 4-187, Syracuse University            //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added reference to System.ServiceModel
 * - Added using System.ServiceModel
 * - Added reference to System.Runtime.Serialization
 * - Added using System.Runtime.Serialization
 */
/*
 * Maintenance History:
 * --------------------
 * ver 1.2 : 22 Nov 2015
 * - updated list of [DataMembers]
 * ver 1.1 : 29 Oct 2015
 * - added comment in data contract
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace Project4Starter
{
  [ServiceContract (Namespace ="Project4Starter")]
  public interface ICommService
  {
    [OperationContract(IsOneWay = true)]
    void sendMessage(Message msg);
  }

    [DataContract]
    public class Message
    {
        [DataMember]
        public string fromUrl { get; set; }
        [DataMember]
        public string toUrl { get; set; }
        [DataMember]
        public string content { get; set; }  // will hold query search parameters, or query response
        [DataMember]
        public string mode { get; set; }     // mode, delete, edit, persist, restore, queries
        [DataMember]
        public string query { get; set; }
        //[DataMember]
        //public int reps { get; set; }      //number of times the add will be performed
        [DataMember]
        public int key { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string descr { get; set; }
        [DataMember]
        public DateTime timestamp1 { get; set; }
        [DataMember]
        public DateTime timestamp2 { get; set; }
        [DataMember]
        public List<int> children { get; set; }  
        [DataMember]
        public string payload { get; set; }
        [DataMember]
        public string response { get; set; }
        [DataMember]
        public bool isLast { get; set; }

        //[DataMember]
        //public DBElement<int, string> dbElement = new DBElement<int, string>() { };
    }
}