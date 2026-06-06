using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagement.DAL.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.DAL.Models
{
    public class GymUser : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public DateOnly DateOfBirth { get; set; }
        // Gender = [Male | Female] => Enum
        public Gender Gender { get; set; }
        // Address = [Building - City - Street] => Owned Entity
        public Address Address { get; set; }
    }

    [Owned]
    public class Address
    {
        public string Street { get; set; } = default!;
        public string City { get; set; } = default!;
        public string BuildingNumber { get; set; } = default!;
    }
}
