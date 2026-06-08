using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagement.DAL.Data.Models.Enums;

namespace GymManagement.DAL.Data.Models
{
    public class Trainer : GymUser
    {
        //public DateTime HireDate { get; set; } = CreatedAt of BaseEntity
        public Speciality Speciality { get; set; }
        #region Relationships
        public ICollection<Session> Sessions { get; set; } = default!;
        #endregion
    }
}
