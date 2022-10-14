namespace Budgetty.Domain
{
    public class SnapshotLock
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime? LockedAt { get; set; }
    }
}