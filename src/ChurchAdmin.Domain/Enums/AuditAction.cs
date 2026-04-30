namespace ChurchAdmin.Domain.Enums;

public enum AuditAction
{
    // core CRUD
    Created = 1,
    Updated = 2,
    Deleted = 3,

    // lifecycle
    Activated = 10,
    Deactivated = 11,

    // relationships
    Assigned = 20,
    Removed = 21,

    // approvals / workflow
    Approved = 30,
    Rejected = 31,
    Verified = 32,
    Retired = 33,

    // finance
    Corrected = 40,
    Reconciled = 41,

    // auth / system
    LoggedIn = 50,
    LoggedOut = 51
}