namespace Buzz.Model
{
    /// <summary>
    /// Representing a dynamic Address Range
    /// </summary>
    class AddressRange
    {
        /// <summary>
        /// Address Range
        /// </summary>
        public string AddressRangeString { get; }

        /// <summary>
        /// Sequence Number
        /// </summary>
        public int Id { get; }

        private AddressRange(int id)
        {
            AddressRangeString = $"10.{id}.0.0/16";
            Id = id;
        }
        
        /// <summary>
        /// Create a dynamic Address Range object
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AddressRange Make(int id)=> 
            new AddressRange(id);
    }
}