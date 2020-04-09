using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Covid.Repository.Model.Users
{
    [Table("User")]
    public class User : CovidBaseEntity
    {
        [Column("Firstname")]
        public string Firstname { get; set; }

        [Column("Surname")]
        public string Surname { get; set; }

        [Column("Dob")]
        public DateTime DateOfBirth { get; set; }
    }
}
