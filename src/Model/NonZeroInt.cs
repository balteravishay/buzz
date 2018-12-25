using System;

namespace Buzz.Model
{
    /// <summary>
    /// An Integer which is not zero.
    /// Used to avoid divide by zero exceptions.
    /// </summary>
    public class NonZeroInt
    {
        /// <summary>
        /// Value 
        /// </summary>
        public int IntValue { get; }

        private NonZeroInt(int value)
        {
            if (value == 0)
                throw new ArgumentException("Value cannot be zero");
            IntValue = value;
        }

        /// <summary>
        /// Create a non zero integer. throws exception if value is zero
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NonZeroInt Make(int value) => new NonZeroInt(value);
    }
}
