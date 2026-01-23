using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string EncryptedPassword { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AlarmEvent> AlarmEvents { get; set; } = new List<AlarmEvent>();

    public virtual ICollection<PartRequest> PartRequestFromUsers { get; set; } = new List<PartRequest>();

    public virtual ICollection<PartRequest> PartRequestToUsers { get; set; } = new List<PartRequest>();

    public virtual ICollection<RequestLog> RequestLogs { get; set; } = new List<RequestLog>();

    public virtual ICollection<ShiftTransfer> ShiftTransferFromUsers { get; set; } = new List<ShiftTransfer>();

    public virtual ICollection<ShiftTransfer> ShiftTransferToUsers { get; set; } = new List<ShiftTransfer>();

    public virtual ICollection<WorkReport> WorkReports { get; set; } = new List<WorkReport>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
