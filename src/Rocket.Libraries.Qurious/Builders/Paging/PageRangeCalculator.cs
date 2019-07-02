namespace Rocket.Libraries.Qurious.Builders.Paging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rocket.Libraries.Qurious.Models;
    using Rocket.Libraries.Validation.Services;

    public static class PageRangeCalculator
    {
        public static PageRange GetPageRange(int page, int pageSize)
        {
            const int pagesAdvanceRate = 1;
            var previousPage = page - pagesAdvanceRate;
            var range = new PageRange
            {
                Start = pagesAdvanceRate + (pageSize * previousPage),
            };
            range.End = range.Start + pageSize - pagesAdvanceRate;
            return range;
        }
    }
}