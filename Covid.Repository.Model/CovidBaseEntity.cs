using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Covid.Repository.Model
{
    public abstract class CovidBaseEntity
    {
        [Column("Id")]
        [Range(0, Int64.MaxValue)]
        [Key]
        public long Id { get; set; }
    }
}
