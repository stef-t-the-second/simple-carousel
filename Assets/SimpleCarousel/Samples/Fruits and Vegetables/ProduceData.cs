using System;
using UnityEngine;

namespace Steft.SimpleCarousel.Samples
{
    [Serializable]
    internal class ProduceData : ICarouselData
    {
        [SerializeField]            private string m_Name;
        [TextArea] [SerializeField] private string m_Description;
        [SerializeField]            private string m_ImageName;

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
