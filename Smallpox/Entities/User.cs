using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smallpox.Entities
{
    [Table("Users")]
    public class User : IdentityUser<Guid>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid Id { get; set; }

        public override string UserName { get; set; }

        public Guid UserInformationId { get; set; }
        public UserInformation UserInformation { get; set; }

        public virtual IList<UserInformation> Roles { get; set; } = new List<UserInformation>();
    }
}
