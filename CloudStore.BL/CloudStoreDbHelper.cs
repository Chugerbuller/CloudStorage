using CloudStore.BL;
using CloudStore.BL.Exceptions;
using CloudStore.BL.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudStore.DAL
{
    public class CloudStoreDbHelper
    {
        private readonly CloudStoreDbContext _dbContext;

        public CloudStoreDbHelper()
        {
            _dbContext = new CloudStoreDbContextFactory().CreateDbContext();
        }

        public async Task<IEnumerable<FileModel>> GetAllFilesAsync() =>
           await _dbContext.Files.ToListAsync();

        public async Task<FileModel?> GetFileByIdAsync(int id) =>
             await _dbContext.Files.SingleOrDefaultAsync(f => f.Id == id);

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
                temp.Extension = file.Extension;
                temp.Path = file.Path;
                temp.Name = file.Name;

                await _dbContext.SaveChangesAsync();
            }
            else
                throw new NullReferenceException();
        }

        public async Task DeleteFileByIdAsync(int id)
        {
            var temp = _dbContext.Files.FirstOrDefault(f => f.Id == id);
            if (temp is null)
                throw new NullReferenceException();
            _dbContext.Files.Remove(temp);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> FindUserByLoginAndPassword(string login, string password)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Login == login);
            
            if (user is null)
                throw new LoginException();

            if (user.Password != password)
                throw new PasswordException();
            
            return user;
        }

        public async Task AddNewUserAsync(User user)
        {
            var check = await _dbContext.Users.SingleOrDefaultAsync(u => u.Login == user.Login);
            if (check is not null)
                throw new ExistentLoginException();

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}