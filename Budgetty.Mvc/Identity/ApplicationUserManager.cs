using System.Diagnostics.CodeAnalysis;
using Budgetty.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Budgetty.Mvc.Identity;

[ExcludeFromCodeCoverage]
public class ApplicationUserManager : UserManager<IdentityUser>
{
    private readonly IUserInitializer _userInitializer;

    public ApplicationUserManager(IUserStore<IdentityUser> store, IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<IdentityUser> passwordHasher, IEnumerable<IUserValidator<IdentityUser>> userValidators,
        IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators, ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<IdentityUser>> logger, IUserInitializer userInitializer) : base(store,
        optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _userInitializer = userInitializer;
    }

    public override async Task<IdentityResult> CreateAsync(IdentityUser user)
    {
        var result = await base.CreateAsync(user);

        if (result.Succeeded)
        {
            await _userInitializer.InitializeUser(user);
        }

        return result;
    }
}