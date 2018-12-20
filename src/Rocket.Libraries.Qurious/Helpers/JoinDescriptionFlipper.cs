namespace Rocket.Libraries.Qurious.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class JoinDescriptionFlipper
    {
        public void Flip(JoinDescription joinDescription)
        {
            var flippedVersion = new JoinDescription
            {
                LeftField = joinDescription.RightField,
                LeftTable = joinDescription.RightTable,
                RightTable = joinDescription.LeftTable,
                RightField = joinDescription.LeftField,
            };

            joinDescription.LeftField = flippedVersion.LeftField;
            joinDescription.LeftTable = flippedVersion.LeftTable;
            joinDescription.RightField = flippedVersion.RightField;
            joinDescription.RightTable = flippedVersion.RightTable;
        }
    }
}