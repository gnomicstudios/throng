// ISensor.cs
//

namespace Throng.AI
{
    public interface ISensor
    {
        bool Enabled { get; set; }

        /// <summary>
        /// Returns a valid SensorObject or null if the sensor was unable to
        /// find anything valid to return.
        /// </summary>
        /// <remarks>
        /// The intent is to enforce 'sensory honesty' to prevent agents from
        /// seeing through walls or otherwise 'cheating'. The object returned
        /// by this function is considered to valid; if no such valid object
        /// could be found then null is returned.
        /// </remarks>
        SensorObject Check();
    }
}