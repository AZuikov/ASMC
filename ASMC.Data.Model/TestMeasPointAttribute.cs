using System;

namespace ASMC.Data.Model
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestMeasPointAttribute : Attribute
    {
        public string Mode;
        public Type MeasPointType;
        public TestMeasPointAttribute(string mode, Type measPointType)
        {
            Mode = mode;
            MeasPointType = measPointType;
        }
    }
}