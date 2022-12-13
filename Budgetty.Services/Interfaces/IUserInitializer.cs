using Microsoft.AspNetCore.Identity;

namespace Budgetty.Services.Interfaces;

public interface IUserInitializer
{
    Task InitializeUser(IdentityUser user);
}