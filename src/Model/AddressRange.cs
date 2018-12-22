using System;

namespace Buzz.Model
{
    class AddressRange
    {
        public string AddressRangeString { get; }

        public int Id { get; }

        private AddressRange(int id)
        {
            AddressRangeString = $"10.{id}.0.0/16";
            Id = id;
        }
        
        public static AddressRange Make(int id)=> 
            new AddressRange(id);

        public static AddressRange MakeByInternalIp(string ip) => 
            new AddressRange(Int32.Parse(ip.Split('.')[1]));
    }
}