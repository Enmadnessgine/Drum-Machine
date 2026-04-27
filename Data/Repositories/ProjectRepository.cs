using Drum_Machine.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Drum_Machine.Data.Repositories
{
    public class ProjectRepository : BaseRepository<ProjectEntity>
    {
        public ProjectRepository(AppDbContext context) : base(context) { }
        public List<ProjectEntity> GetUserProjects(int userId)
        {
            return _dbSet
                .Where(p => p.UserId == userId)
                .Include(p => p.Tracks)
                .Include(p => p.ExportedTrack)
                .ToList();
        }
    }
}