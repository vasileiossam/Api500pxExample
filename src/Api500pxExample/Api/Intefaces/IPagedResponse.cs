namespace Api500pxExample.Api.Interfaces
{
    public interface IPagedResponse
    {
        int CurrentPage { get; set; }
        int TotalItems { get; set; }
        int TotalPages { get; set; }
    }
}