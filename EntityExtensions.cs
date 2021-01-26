using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EmployeeListLibrary.Helpers
{
  static class EntityExtensions
  {
    public static async Task ClearAsync<T>(this DbSet<T> dbSet) where T : class
    {
      await dbSet.ForEachAsync(v => { dbSet.Remove(v); });
    }

    public static async Task RemoveAllAsync<T>(this DbContext context)
        where T : class
    {
      await context.Set<T>().ForEachAsync(v => { context.Entry(v).State = EntityState.Deleted; });
    }
  }
}
