using EveloraBlogAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EveloraBlogAPI.Repositories
{
    public interface IBlogRepository
    {
        Task<IEnumerable<Blog>> GetAllBlogsAsync();
        Task<Blog?> GetBlogByIdAsync(int id);
        Task<Blog?> GetBlogBySlugAsync(string slug);
        Task<IEnumerable<Blog>> GetBlogsByCategoryAsync(string category);
        Task<Blog> CreateBlogAsync(Blog blog);
        Task<bool> UpdateBlogAsync(Blog blog);
        Task<bool> DeleteBlogAsync(int id);
        Task<bool> BlogExistsAsync(int id);
    }
}
