using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagement.DAL.Models.Enums;

namespace GymManagement.DAL.Models
{
    public class Trainer : GymUser
    {
        //public DateTime HireDate { get; set; } = CreatedAt of BaseEntity
        public Speciality Speciality { get; set; }
    }
}
