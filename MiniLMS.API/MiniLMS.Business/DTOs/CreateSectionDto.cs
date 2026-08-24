using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class CreateSectionDto
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public int OrderIndex { get; set; }
    }
}
