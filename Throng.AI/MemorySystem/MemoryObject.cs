// MemoryObject.cs
//

namespace Throng.AI
{
    public abstract class MemoryObject
    {
        private float m_confidence;
        private float m_decayRate;

        public float Confidence
        {
            get { return m_confidence; }
        }

        public bool Update()
        {
            return true;
        }

        protected MemoryObject(float confidence, float decayRate)
        {
            m_confidence = confidence;
            m_decayRate = decayRate;
        }
    }
}