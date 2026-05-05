using Drum_Machine.Data.Entities;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Drum_Machine.Data.Repositories
{
    public class SampleRepository : BaseRepository<SampleEntity>
    {
        public SampleRepository(AppDbContext context) : base(context) { }

        public List<SampleEntity> GetUserLibrary(int userId)
        {
            return _dbSet.Where(s => s.UserId == userId).ToList();
        }


        public void AddToUserLibrary(int userId, string originalFilePath, string sampleName)
        {
            string userSamplesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users", userId.ToString(), "Samples");

            if (!Directory.Exists(userSamplesPath))
                Directory.CreateDirectory(userSamplesPath);

            string fileName = Path.GetFileName(originalFilePath);
            string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            string destinationPath = Path.Combine(userSamplesPath, uniqueFileName);

            File.Copy(originalFilePath, destinationPath, true);
            var sampleEntity = new SampleEntity
            {
                Name = sampleName,
                FilePath = destinationPath,
                UserId = userId
            };

            this.Add(sampleEntity);
            this.Save();
        }

        public override void Delete(int id)
        {
            var sample = GetById(id);

            if (sample != null)
            {
                if (!string.IsNullOrEmpty(sample.FilePath) && File.Exists(sample.FilePath))
                {
                    try
                    {
                        File.Delete(sample.FilePath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Не вдалося видалити файл: {ex.Message}");
                    }
                }

                _dbSet.Remove(sample);
                this.Save();
            }
        }
    }
}