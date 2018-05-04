using Smallpox.Entities.Information;
using System;

namespace Smallpox.Entities
{
    public class UserInformation
    {
        public UserInformation()
        {
            AddressId = null;
            CompanyId = null;
            CampusId = null;
            PatientId = null;
            PersonalId = null;
        }
        public Guid Id { get; set; }

        public Guid? AddressId { get; set; }
        public Address Address { get; set; }

        public Guid? CampusId { get; set; }
        public Campus Campus { get; set; }

        public Guid? CompanyId { get; set; }
        public Company Company { get; set; }

        public Guid? PatientId { get; set; }
        public Patient Patient { get; set; }

        public Guid? PersonalId { get; set; }
        public Personal Personal { get; set; }
    }
}
