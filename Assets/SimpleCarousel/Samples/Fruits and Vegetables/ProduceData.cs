using System;
using UnityEngine;

namespace Steft.SimpleCarousel.Samples
{
    [Serializable]
    public class ProduceData
    {
        [SerializeField] private string m_ImageName;
        [SerializeField] private string m_Name;
        [SerializeField] private string m_Description;

        public ProduceData(string imageName, string name, string description)
        {
            m_ImageName   = imageName;
            m_Name        = name;
            m_Description = description;
        }

        public string imageName => m_ImageName;

        public string name => m_Name;

        public string description => m_Description;
    }
}
