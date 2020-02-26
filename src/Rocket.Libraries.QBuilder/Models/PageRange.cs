namespace Rocket.Libraries.Qurious.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class PageRange
    {
        public int Start { get; set; }

        public int End { get; set; }

        public override bool Equals(object obj)
        {
            var inputObject = obj as PageRange;
            if (inputObject == null)
            {
                return false;
            }

            return inputObject.Start == Start
                && inputObject.End == End;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}