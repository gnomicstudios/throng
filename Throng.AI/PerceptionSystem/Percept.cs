// Percept.cs
//

using System.Collections.Generic;
using System.Linq;

namespace Throng.AI
{
    public abstract class Percept
    {
        private List<Percept> m_children = new List<Percept>();

        public List<Percept> Children
        {
            get { return m_children; }
        }

        public abstract int Salience { get; }
        public abstract int Match(SensorObject sensorObject, out PerceptObject perceptObject);
    }
}