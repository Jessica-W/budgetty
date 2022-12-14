using Microsoft.AspNetCore.Identity;

namespace Budgetty.Mvc.Identity;

public interface IApplicationUserManager
{
    Task<IdentityResult> CreateAsync(IdentityUser user);
}