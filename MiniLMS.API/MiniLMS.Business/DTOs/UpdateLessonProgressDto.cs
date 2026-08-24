using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class UpdateLessonProgressDto
    {
        public int LastWatchedSeconds { get; set; }
        public decimal WatchPercentage { get; set; }
        public bool? ForceCompleted { get; set; }
    }
}
