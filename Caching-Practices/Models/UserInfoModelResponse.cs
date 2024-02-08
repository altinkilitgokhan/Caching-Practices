namespace Caching_Practices.Models
{
    public class UserInfoModelResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsReadFromCache { get; set; }
    }
}
