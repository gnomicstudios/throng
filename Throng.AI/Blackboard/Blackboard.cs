// Blackboard.cs
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gnomic.AI
{
    public sealed class Blackboard
    {
        private Dictionary<Type, List<DataObject>> m_posts =
            new Dictionary<Type, List<DataObject>>();

        public bool AddPost(DataObject dataObject)
        {
            Type objectType = dataObject.GetType();

            if (!m_posts.Keys.Contains(objectType))
            {
                m_posts[objectType] = new List<DataObject>();
            }

            m_posts[objectType].Add(dataObject);

            return true;
        }

        public bool RemovePost(DataObject dataObject)
        {
            Type objectType = dataObject.GetType();

            if (!m_posts.Keys.Contains(objectType) ||
                !m_posts[objectType].Contains(dataObject))
            {
                return false;
            }

            m_posts[objectType].Remove(dataObject);

            return true;
        }
    }
}