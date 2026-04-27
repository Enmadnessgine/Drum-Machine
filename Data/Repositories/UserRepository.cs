using Drum_Machine.Data.Entities;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Drum_Machine.Data.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository(AppDbContext context) : base(context) { }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public User Login(string username, string password)
        {
            string hashedPassword = HashPassword(password);
            return _dbSet.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);
        }

        public override void Add(User user)
        {
            user.Password = HashPassword(user.Password);
            base.Add(user);
            Save();

            string userPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users", user.Id.ToString());
            Directory.CreateDirectory(Path.Combine(userPath, "Samples"));
            Directory.CreateDirectory(Path.Combine(userPath, "Projects"));
        }
    }
}