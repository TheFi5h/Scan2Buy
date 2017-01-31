using System;

namespace Domain
{
    public class TagData
    {
        public TagData(string id, DateTime timeStamp, string data)
        {
            Id = id;
            TimeStamp = timeStamp;
            Data = data;
        }

        public TagData(string id, string data)
        {
            Id = id;
            Data = data;
            TimeStamp = DateTime.Now;
        }

        public string Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Data { get; set; }

    }
}
