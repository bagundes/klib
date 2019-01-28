using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace klib.model
{
    public class Report
    {
        [BsonId()]
        public int Id;
        [BsonElement("customer id")]
        [BsonRequired()]
        public string CID;
        [BsonElement("code")]
        [BsonRequired()]
        public int Code;
        [BsonElement("message")]
        [BsonRequired()]
        public string Message;
        [BsonElement("technical details")]
        [BsonRequired()]
        public string Details;
        [BsonElement("technical computer")]
        [BsonRequired()]
        public string Computer;
    }
}
