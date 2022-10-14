using Budgetty.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Budgetty.Mvc.Identity;

public class ApplicationUserManager : UserManager<IdentityUser>
{
    private readonly ISequenceNumberProvider _sequenceNumberProvider;
    private readonly ISnapshotLockManager _snapshotLockManager;

    public ApplicationUserManager(IUserStore<IdentityUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<IdentityUser> passwordHasher, IEnumerable<IUserValidator<IdentityUser>> userValidators, IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<IdentityUser>> logger, ISequenceNumberProvider sequenceNumberProvider, ISnapshotLockManager snapshotLockManager) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _sequenceNumberProvider = sequenceNumberProvider;
        _snapshotLockManager = snapshotLockManager;
    }

    public override async Task<IdentityResult> CreateAsync(IdentityUser user)
    {
        var result = await base.CreateAsync(user);

        if (result.Succeeded)
        {
            await _sequenceNumberProvider.InitialiseSequenceNumberAsync(user.Id);
            await _snapshotLockManager.InitialiseLock(user.Id);
        }

        return result;
    }
}