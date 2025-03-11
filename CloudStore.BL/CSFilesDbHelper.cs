using CloudStore.BL;
using CloudStore.BL.Exceptions;
using CloudStore.BL.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudStore.DAL
{
    public class CSFilesDbHelper
    {
        private readonly CloudStoreDbContext _dbContext;

        public CSFilesDbHelper()
        {
            _dbContext = new CloudStoreDbContextFactory().CreateDbContext();
        }

        public async Task<IEnumerable<FileModel>> GetAllFilesAsync(User user) =>
            await _dbContext.Files.Where(f => f.User == user).ToListAsync();

        public async Task<IEnumerable<FileModel>> GetAllFilesInDirectory(User user, string directory)
        {
            return await _dbContext.Files.Where(f => f.Path == directory + "\\" + f.Name).ToListAsync();
        }

        public async Task<FileModel?> GetFileByIdAsync(int id, User user) =>
             await _dbContext.Files.SingleOrDefaultAsync(f => f.Id == id && f.User == user);

        public async Task AddFileAsync(FileModel file)
        {
            await _dbContext.Files.AddAsync(file);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateFile(FileModel file)
        {
            var temp = await _dbContext.Files.SingleOrDefaultAsync(f => f.Id == file.Id);
            if (temp is not null)
            {
                if (file.Name != temp.Name)
                {
                    temp.Name = file.Name;
                    var newPath = temp.Path.Split('\\');
                    newPath[^1] = file.Name;
                    temp.Path = string.Join('\\', newPath);
                }    
                temp.Extension = file.Extension;
                temp.Path = file.Path;
                
                await _dbContext.SaveChangesAsync();
            }
            else
                throw new NullReferenceException();
        }

        public async Task DeleteFileByIdAsync(int id, User user)
        {
            var temp = _dbContext.Files.FirstOrDefault(f => f.Id == id && f.User == user)
                ?? throw new NullReferenceException();
            _dbContext.Files.Remove(temp);

            await _dbContext.SaveChangesAsync();
        }
    }
}