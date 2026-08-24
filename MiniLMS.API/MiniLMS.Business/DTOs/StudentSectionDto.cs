using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class StudentSectionDto
    {
        public int SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public List<LessonProgressDto> Lessons { get; set; } = new();
    }
}
