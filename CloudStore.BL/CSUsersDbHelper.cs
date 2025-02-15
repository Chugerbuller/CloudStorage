using CloudStore.BL.Exceptions;
using CloudStore.BL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Npgsql.TypeMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.BL;

public class CSUsersDbHelper
{
    private readonly CloudStoreDbContext _context;

    public CSUsersDbHelper()
    {
        _context = new CloudStoreDbContextFactory().CreateDbContext();
    }

    public async Task<User?> GetUserAsync(string login) =>
        await _context.Users.FirstOrDefaultAsync(x => x.Login == login);

    public async Task<User?> FindUserByLoginAndPassword(string login, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Login == login)
            ?? throw new LoginException();

        if (user.Password != password)
            throw new PasswordException();

        return user;
    }

    public async Task CreateUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        var temp = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        temp.Login = user.Login;
        temp.Password = user.Password;
        temp.UserDirectory = user.UserDirectory;

        await _context.SaveChangesAsync();
    }
    public async Task DeleteUserAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
    public User? GetUserByApiKey(string apiKey) => 
        _context.Users.FirstOrDefault(u => u.ApiKey == apiKey);   

}