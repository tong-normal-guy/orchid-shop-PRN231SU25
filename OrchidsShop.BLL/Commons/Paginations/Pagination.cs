namespace OrchidsShop.BLL.Commons.Paginations;

public class Pagination
{
    public Pagination()
    {
        PageIndex = 0;
        PageSize = 1;
        TotalItemsCount = 1;
    }

    public Pagination(int pageIndex, int pageSize, int count)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalItemsCount = count;
    }

    public int TotalItemsCount { get; set; }
    public int PageSize { get; set; } = -1;
    public int PageIndex { get; set; } = 0;

    public int TotalPagesCount
    {
        get
        {
            if (PageSize == -1)
            {
                return TotalItemsCount;
            }

            var temp = TotalItemsCount / PageSize;
            if (TotalItemsCount % PageSize == 0)
            {
                return temp;
            }

            return temp + 1;
        }
    }
}